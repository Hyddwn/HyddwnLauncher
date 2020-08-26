using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Network;
using HyddwnLauncher.Util;
using Ionic.Zlib;

namespace HyddwnLauncher.Patcher.NxLauncher
{
    internal class PatchDownloader
    {
        private QueueManager _queueManager;
        private List<Patch> _patches;
        private IClientProfile _clientProfile;
        private bool _prepared;
        private string _downloadDirectory;
        private const string DownloadBaseUrl = "https://download2.nexon.net/Game/nxl/games/10200/10200/";
        private PatcherContext _patcherContext;

        public PatchDownloader(List<Patch> patches, IClientProfile clientProfile,
            PatcherContext patcherContext)
        {
            _patches = patches;
            _clientProfile = clientProfile;
            _patcherContext = patcherContext;
            _queueManager = new QueueManager(_patcherContext);
        }

        public void Prepare()
        {
            var copyDirectory = Path.GetDirectoryName(_clientProfile.Location);
            _downloadDirectory = copyDirectory + @"\..\patchdata\Patch";

            _queueManager.Initialize(_patches, _downloadDirectory, copyDirectory);

            _prepared = true;
        }

#pragma warning disable 1998
        public async Task<bool> Patch()
#pragma warning restore 1998
        {
            if (!_prepared)
                throw new InvalidOperationException(Properties.Resources.PatchDownloaderNotInitializedMessage);

            Log.Info(Properties.Resources.BeginObjectDownload, false);

            var downloadTasks = new List<Action>();
            double files = _queueManager.Count;
            int completed = 0;
            bool failed = false;
            int failedCount = 0;

            _patcherContext.UpdateMainProgress(Properties.Resources.DownloadingParts, $"{completed}/{files}", 0, false, true);

            while (_queueManager.Count != 0)
            {
                var filePart = _queueManager.Dequeue();
                Log.Info(
                    string.Format(Properties.Resources.DownloadingPartNumberPartNameForFileName, filePart.Index, filePart.PartName,
                        filePart.FileName), false);
                var downloadUrl = DownloadBaseUrl + $"{filePart.PartName.Substring(0, 2)}/{filePart.PartName}";
                var downloadDirectory = Path.Combine(_downloadDirectory, "_parts");

                downloadTasks.Add(async () =>
                {
                    try
                    {
                        var partFilename = Path.Combine(downloadDirectory, $"{filePart.FileName}.{filePart.Index:D3}");

                        await DownloadPart(downloadUrl, partFilename);
                        _queueManager.AddToFileTable(filePart.FileName, partFilename, filePart.Index);
                        Log.Info(Properties.Resources.DownloadedPartNumberPartNameForFileNameSuccessfully,
                            filePart.Index, filePart.PartName, filePart.FileName);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            var partFilename = Path.Combine(downloadDirectory, $"{filePart.FileName}.{filePart.Index:D3}");

                            await DownloadPart(downloadUrl, partFilename);
                            _queueManager.AddToFileTable(filePart.FileName, partFilename, filePart.Index);
                            Log.Info(Properties.Resources.DownloadedPartNumberPartNameForFileNameSuccessfully,
                                filePart.Index, filePart.PartName, filePart.FileName);
                        }
                        catch (Exception ex1)
                        {
                            Log.Exception(ex1, Properties.Resources.FailedToDownloadPartNumberPartNameForFileName,
                                filePart.Index, filePart.PartName, filePart.FileName);

                            Interlocked.Increment(ref failedCount);
                            failed = true;
                        }

                        Interlocked.Increment(ref failedCount);
                        failed = true;
                    }
                    finally
                    {
                        Interlocked.Increment(ref completed);
                        _patcherContext.UpdateMainProgress(Properties.Resources.DownloadingParts,
                            $"{completed}/{files}", completed / files * 100,
                            false, true);
                    }
                });
            }

            Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = 10 }, downloadTasks.ToArray());

            _patcherContext.UpdateMainProgress(Properties.Resources.DownloadComplete, $"{completed}/{files}", 0, false, false);
            Log.Info(Properties.Resources.DownloadComplete);

            _queueManager.Finish();

            return !failed;
        }

        public async Task DownloadPart(string uri, string filename)
        {
            var progressReporter = _patcherContext.CreateProgressIndicator();

            var fileDirectory = Path.GetDirectoryName(filename);

            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            Task.Run(async () =>
            {
                await AsyncDownloader.DownloadFileWithCallbackAsync(uri, filename, (d, s) =>
                {
                    progressReporter.SetLeftText(Path.GetFileName(filename));
                    progressReporter.SetRightText(s);
                    progressReporter.SetProgressBar(d);
                }, true);
            }).Wait();

            progressReporter.SetIsIndeterminate(true);
            progressReporter.SetRightText(Properties.Resources.Decompressing);

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                var buffer = ms.ToArray();
                buffer = ZlibStream.UncompressBuffer(buffer);
                fs.SetLength(buffer.Length);
                fs.Position = 0;
                fs.Write(buffer, 0, buffer.Length);
                fs.Flush();
            }

            _patcherContext.DestroyProgressIndicator(progressReporter);
        }

        public void Cleanup()
        {
            _patcherContext.UpdateMainProgress(Properties.Resources.CleaningUp, "", 0, true, true);
            TryDeleteDirectory(_downloadDirectory);
        }

        private void TryDeleteDirectory(string p)
        {
            try
            {
                Directory.Delete(p, true);
            }
            catch (Exception ex)
            {
                Log.Info(Properties.Resources.CouldNotDeleteFile, p, ex.Message);
            }
        }

        private class DownloadWrapper
        {
            public Patch Patch { get; }
            private FileStream FileStream { get; set; }

            private string _downloadFilename;
            private string _copyFilename;

            private SortedDictionary<int, string> _fileTable;

            private PatcherContext _patcherContext;

            public DownloadWrapper(Patch patch, PatcherContext patcherContext)
            {
                Patch = patch;
                _patcherContext = patcherContext;
            }

            public void Initialize(string downloadDirectory, string copyDirectory)
            {
                _downloadFilename = Path.Combine(downloadDirectory, Patch.FileDownloadInfo.FileName + ".hyddwndl");
                _copyFilename = Path.Combine(copyDirectory, Patch.FileDownloadInfo.FileName);

                if (Directory.Exists(Path.GetDirectoryName(_downloadFilename)))
                    TryDeleteDirectory(Path.GetDirectoryName(_downloadFilename));

                _fileTable = new SortedDictionary<int, string>();
            }

            public void AddToTable(string path, int index)
            {
                _fileTable[index] = path;
            }

            public void Finish()
            {
                if (!Directory.Exists(Path.GetDirectoryName(_downloadFilename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(_downloadFilename));

                FileStream = new FileStream(_downloadFilename, FileMode.Create, FileAccess.ReadWrite);
                FileStream.SetLength(Patch.FileDownloadInfo.FileSize);

                double parts = _fileTable.Count;
                int completed = 0;

                var progressReporter = _patcherContext.CreateProgressIndicator();
                progressReporter.SetLeftText($"{Patch.FileDownloadInfo.FileName}");

                foreach (var file in _fileTable.Values)
                {
                    byte[] buffer;

                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    using (var ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        buffer = ms.ToArray();
                    }

                    FileStream.Write(buffer, 0, buffer.Length);
                    FileStream.Flush();

                    completed++;

                    progressReporter.SetRightText($"{completed}/{parts}");
                    progressReporter.SetProgressBar(completed / parts * 100);
                }

                FileStream.Flush(true);
                FileStream.Close();
                FileStream.Dispose();
                CopyFile(progressReporter);
            }

            private void CopyFile(IProgressIndicator progressReporter)
            {
                progressReporter.SetIsIndeterminate(true);
                progressReporter.SetRightText(Properties.Resources.Copying);

                // Even though there is a patch reason DoesNotExist check anyways
                if (File.Exists(_copyFilename))
                    // Just in case things need to be reverted before it is finished
                    File.Move(_copyFilename, _downloadFilename + ".old");

                // In case the target file is missing...
                if (!File.Exists(_downloadFilename))
                {
                    Log.Info(Properties.Resources.FileFilenameIsMissing, _downloadFilename);
                    File.Move(_downloadFilename + ".old", _copyFilename);
                }

                if (!Directory.Exists(Path.GetDirectoryName(_copyFilename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(_copyFilename));

                File.Move(_downloadFilename, _copyFilename);

                File.SetLastWriteTimeUtc(_copyFilename, Patch.FileDownloadInfo.LastModifiedDateTime);
                File.SetLastAccessTimeUtc(_copyFilename, Patch.FileDownloadInfo.LastModifiedDateTime);

                _patcherContext.DestroyProgressIndicator(progressReporter);
            }

            private void TryDeleteDirectory(string p)
            {
                try
                {
                    Directory.Delete(p, true);
                }
                catch (Exception ex)
                {
                    Log.Info(Properties.Resources.CouldNotDeleteFile, p, ex.Message);
                }
            }
        }

        private class QueueManager
        {
            private readonly Dictionary<string, DownloadWrapper> _downloadWrappers;
            private readonly Queue<FilePartInfo> _fileParts;

            public int Count => _fileParts?.Count ?? 0;

            private PatcherContext _patcherContext;

            public QueueManager(PatcherContext patcherContext)
            {
                _patcherContext = patcherContext;
                _downloadWrappers = new Dictionary<string, DownloadWrapper>();
                _fileParts = new Queue<FilePartInfo>();
            }

            public void Initialize(IEnumerable<Patch> patches, string downloadDirectory, string copyDirectory)
            {
                Populate(patches);

                if (Directory.Exists(downloadDirectory))
                    TryDeleteDirectory(downloadDirectory);

                if (Directory.Exists(downloadDirectory))
                {
                    throw new InvalidOperationException("An issue occured while attempting to clear previous patch data! Patching will not continue.");
                }

                foreach (var downloadWrapper in _downloadWrappers.Values)
                {
                    downloadWrapper.Initialize(downloadDirectory, copyDirectory);
                }
            }

            public FilePartInfo Dequeue()
            {
                return Count < 1 ? null : _fileParts.Dequeue();
            }

            private void Populate(IEnumerable<Patch> patches)
            {
                var enumerable = patches as IList<Patch> ?? patches.ToList();
                foreach (var patch in enumerable.Where(p => p.FileDownloadInfo.FileInfoType != FileInfoType.Directory))
                {
                    var downloadWrapper = new DownloadWrapper(patch, _patcherContext);
                    _downloadWrappers.Add(downloadWrapper.Patch.FileDownloadInfo.FileName, downloadWrapper);
                    foreach (var filePartInfo in patch.FileDownloadInfo.FileParts)
                    {
                        _fileParts.Enqueue(filePartInfo);
                    }
                }
            }

            private void TryDeleteDirectory(string p)
            {
                try
                {
                    Directory.Delete(p, true);
                }
                catch (Exception ex)
                {
                    Log.Info(Properties.Resources.CouldNotDeleteFile, p, ex.Message);
                }
            }

            public void Finish()
            {
                double count = _downloadWrappers.Count;
                int completed = 0;

                _patcherContext.UpdateMainProgress(Properties.Resources.FinalizingFiles, $"{completed}/{count}", 0, false, true);

                var actions = _downloadWrappers.Values.Select(downloadWrapper => (Action) (() =>
                    {
                        downloadWrapper.Finish();
                        Interlocked.Increment(ref completed);
                        _patcherContext.UpdateMainProgress(Properties.Resources.FinalizingFiles, $"{completed}/{count}", completed / count * 100, false, true);
                    }))
                    .ToList();

                _downloadWrappers.Clear();

                Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = 10 }, actions.ToArray());
            }

            public void AddToFileTable(string filename, string partFileName, int index)
            {
                DownloadWrapper downloadWrapper;
                if (!_downloadWrappers.TryGetValue(filename, out downloadWrapper))
                    throw new KeyNotFoundException(nameof(filename));
                downloadWrapper.AddToTable(partFileName, index);
            }
        }
    }
}
