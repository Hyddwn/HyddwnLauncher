using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HyddwnLauncher.Core;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Network;
using HyddwnLauncher.Util;
using HyddwnLauncher.Util.Helpers;
using Ionic.Zip;

namespace HyddwnLauncher.Patcher.Legacy
{
    public class PatchDownloader
    {
        private string _clientDirectory;

        private DownloadQueue _downloadQueue;
        private readonly OfficialPatchInfo _officialPatchInfo;
        private IClientProfile _clientProfile;
        private PatcherContext _patcherContext;
        private bool _prepared;

        /// <summary>
        ///     Creates a new instance of PatcherDownloader that accepts a PatchSequence
        /// </summary>
        /// <param name="patchSequence">PatchSequence representing a collection of patches</param>
        /// <param name="officialPatchInfo"></param>
        /// <param name="clientProfile"></param>
        public PatchDownloader(PatcherContext patcherContext, PatchSequence patchSequence, OfficialPatchInfo officialPatchInfo, IClientProfile clientProfile)
        {
            Patches = new List<PatchInfo>();
            _patcherContext = patcherContext;
            _officialPatchInfo = officialPatchInfo;
            _clientProfile = clientProfile;
            Patches.AddRange(patchSequence.Patches);
        }

        /// <summary>
        ///     Creates a new instance of PatchDownloader that accepts a PatchInfo
        /// </summary>
        /// <param name="patchInfo">PatchInfo representing a single patch.</param>
        public PatchDownloader(PatchInfo patchInfo)
        {
            Patches = new List<PatchInfo>();
            Patches.Add(patchInfo);
        }

        private List<PatchInfo> Patches { get; }

        /// <summary>
        ///     Initializes the download queue and stores each patch in order
        /// </summary>
        public void Prepare()
        {
            // Get the directory for the client for conviencnce
            // TODO: Actually pass to download queue. (Might not need since already in working dir)
            _clientDirectory = Path.GetDirectoryName(_clientProfile.Location);

            _downloadQueue = new DownloadQueue(_patcherContext, _clientProfile, Patches, _officialPatchInfo);
            _downloadQueue.Initialize();

            _prepared = true;
        }

        /// <summary>
        ///     Begins downloading the patches by invoking the download queue's download process.
        /// </summary>
        public void Patch()
        {
            if (!_prepared)
            {
                // Failsafe to ensure when patch is started.... there is something to download!
                Log.Info(Properties.Resources.PatchDownloaderNotInitializedMessage2);
                Prepare();
            }

            // TODO: UI calls!

            _downloadQueue.Start();
        }
    }

    public class PatchTask
    {
        private PatcherContext _patcherContext;
        private IClientProfile _clientProfile;

        /// <summary>
        ///     Creates a task which represent an individual patch operation
        /// </summary>
        /// <param name="clientProfile"></param>
        /// <param name="patchInfo"></param>
        /// <param name="officialPatchInfo"></param>
        /// <param name="patcherContext"></param>
        public PatchTask(PatcherContext patcherContext, IClientProfile clientProfile, PatchInfo patchInfo, OfficialPatchInfo officialPatchInfo)
        {
            _patcherContext = patcherContext;
            _clientProfile = clientProfile;
            PatchInfo = patchInfo;
            OfficialPatchInfo = officialPatchInfo;
        }

        /// <summary>
        ///     The PatchInfo this class wraps
        /// </summary>
        public PatchInfo PatchInfo { get; }

        public OfficialPatchInfo OfficialPatchInfo { get; }

        /// <summary>
        ///     Starts the patching process for this patch set
        /// </summary>
        public void Start()
        {
            var files = PatchInfo.Files;

            do
            {
                files = Verify();
                Download(files);
            } while (files.Count != 0);

            Combine();
            Extract();
            Copy();
            CleanUp();
        }

        public List<PatchFileInfo> Verify()
        {
            Log.Info(Properties.Resources.BeginningFileVerification);
            _patcherContext.UpdateMainProgress(Properties.Resources.BeginningFileVerification,
                isIndeterminate: true,
                isProgressbarVisible: true);

            var list = new List<PatchFileInfo>();

            using (var md5 = MD5.Create())
            {
                foreach (var patchFileInfo in PatchInfo.Files)
                {
                    try
                    {
                        // TODO: Global Cancellation token
                        CancellationToken.None.ThrowIfCancellationRequested();
                    }
                    catch
                    {
                        break;
                    }

                    _patcherContext.UpdateMainProgress(string.Format(Properties.Resources.VerifyingFileName, patchFileInfo.Filename),
                        isIndeterminate: true,
                        isProgressbarVisible: true);

                    try
                    {
                        using (var fileStream =
                            new FileStream(Path.Combine(PatchInfo.PatchName, patchFileInfo.Filename), FileMode.Open))
                        {
                            var actualHash = BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "")
                                .ToLower();

                            if (actualHash != patchFileInfo.Md5Hash)
                            {
                                Log.Info(Properties.Resources.VerifyFailedMD5Hash, patchFileInfo.Filename, patchFileInfo.Md5Hash, actualHash);
                                list.Add(patchFileInfo);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                       Log.Info(Properties.Resources.VerifyFailedException, patchFileInfo.Filename, ex.Message);
                        list.Add(patchFileInfo);
                    }
                }
            }

            Log.Info(Properties.Resources.FileVerificationComplete);
            _patcherContext.UpdateMainProgress(Properties.Resources.FileVerificationComplete);

            return list;
        }

        private void Download(List<PatchFileInfo> files)
        {
            // Log the beginning of the download
            Log.Info(Properties.Resources.BeginningDownload);

            _patcherContext.UpdateMainProgress(Properties.Resources.BeginningDownload, isIndeterminate: true,
                isProgressbarVisible: true);

            // create the actions that will run in parrallel
            var list = new List<Action>();

            var completed = 0;
            int failedCount = 0;

            foreach (var patchFileInfo in files)
            {
                var ftpAddress = OfficialPatchInfo.MainFtp;
                if (!ftpAddress.EndsWith("/")) ftpAddress += "/";

                // Im kinda stuck on how I wanna do this haha

                //var downloader = new FileDownloader(new ProgressReporterViewModel(), patchFileInfo.RemoteName,
                //    patchFileInfo.Filename, patchFileInfo.Size, CancellationToken.None);

                list.Add( () =>
                {
                    try
                    {
                        var progressIndicator = _patcherContext.CreateProgressIndicator();

                        var fileDirectory = Path.GetDirectoryName(Path.Combine(PatchInfo.PatchName, patchFileInfo.Filename));

                        if (!Directory.Exists(fileDirectory))
                            Directory.CreateDirectory(fileDirectory);

                        Task.Run(async () =>
                        {
                            await AsyncDownloader.DownloadFileWithCallbackAsync(string.Concat(ftpAddress, PatchInfo.EndVersion, "/",
                                    patchFileInfo.RemoteName),
                                Path.Combine(PatchInfo.PatchName, patchFileInfo.Filename), (d, s) =>
                                {
                                    progressIndicator.SetLeftText(Path.GetFileName(patchFileInfo.Filename));
                                    progressIndicator.SetRightText(s);
                                    progressIndicator.SetProgressBar(d);
                                });
                        }).Wait();

                        _patcherContext.DestroyProgressIndicator(progressIndicator);
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                        Interlocked.Increment(ref failedCount);
                    }
                    finally
                    {
                        Interlocked.Increment(ref completed);

                        _patcherContext.UpdateMainProgress(Properties.Resources.DownloadingFiles,
                            $"{completed}/{files.Count}", completed / (double)files.Count * 100.0,
                            isProgressbarVisible: true);
                    }
                });
            }

            Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = 10}, list.ToArray());

            Log.Info(Properties.Resources.DownloadComplete);

            _patcherContext.UpdateMainProgress(Properties.Resources.DownloadComplete);
        }

        private void Combine()
        {
            Log.Info(Properties.Resources.BeginningFileCombine);
            _patcherContext.UpdateMainProgress(Properties.Resources.BeginningFileCombine, isIndeterminate: true,
                isProgressbarVisible: true);

            var stopwatch = new Stopwatch();
            var buffer = new byte[4096];
            var completed = 0;
            var totalRead = 0L;
           Log.Info(Properties.Resources.OpeningZipForOutput, PatchInfo.ZipFilePath);

            using (var fileStream = new FileStream(PatchInfo.ZipFilePath, FileMode.Create))
            {
                stopwatch.Start();
                var timer = new Timer(callback =>
                {
                    _patcherContext.UpdateMainProgress(Properties.Resources.CombiningFiles,
                        $"{completed}/{PatchInfo.Files.Count} at {ByteSizeHelper.ToString(totalRead / stopwatch.Elapsed.TotalSeconds)}/s",
                        completed / (double)PatchInfo.Files.Count * 100.0);
                }, null, 200, 200);

                foreach (var patchFileInfo in PatchInfo.Files)
                {
                    Log.Info(Properties.Resources.AddingFileName, patchFileInfo.Filename);
                    using (var inputFileStream =
                        new FileStream(Path.Combine(PatchInfo.PatchName, patchFileInfo.Filename), FileMode.Open))
                    {
                        int count;
                        while ((count = inputFileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            // Placeholder for when I add the cancellation logic yoooooooo.
                            CancellationToken.None.ThrowIfCancellationRequested();

                            fileStream.Write(buffer, 0, count);
                            totalRead += count;
                        }
                    }

                    completed++;
                }

                timer.Change(-1, -1);
                timer.Dispose();
            }

            Log.Info(Properties.Resources.PatchCombineComplete);
            _patcherContext.UpdateMainProgress(Properties.Resources.PatchCombineComplete);
        }

        private void Extract()
        {
            Log.Info(Properties.Resources.ExtractingZipTo, PatchInfo.ContentDirectory);
            Directory.CreateDirectory(PatchInfo.ContentDirectory);
            using (var zipFile = ZipFile.Read(PatchInfo.ZipFilePath))
            {
                zipFile.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                zipFile.ExtractProgress += zipFile_ExtractProgress;
                zipFile.ExtractAll(PatchInfo.ContentDirectory);
            }
            Log.Info(Properties.Resources.ExtractComplete);
        }

        private void zipFile_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_BeforeExtractEntry)
            {
                try
                {
                    // Placeholder for when I add the cancellation logic yoooooooo.
                    CancellationToken.None.ThrowIfCancellationRequested();
                }
                catch
                {
                    e.Cancel = true;
                    throw;
                }

                Log.Info(Properties.Resources.ExtractingFileName, e.CurrentEntry.FileName);
                _patcherContext.UpdateMainProgress(string.Format(Properties.Resources.ExtractingFileName, e.CurrentEntry.FileName), isProgressbarVisible: true,
                    isIndeterminate: true);
            }
            else
            {
                if (e.EventType != ZipProgressEventType.Extracting_AfterExtractEntry)
                    return;
                _patcherContext.UpdateMainProgress(string.Format(Properties.Resources.ExtractingFileName, e.CurrentEntry.FileName),
                    progress: e.EntriesExtracted / (double) e.EntriesTotal * 100.0, isProgressbarVisible: true,
                    isIndeterminate: true);
            }
        }

        private void Copy()
        {
            Log.Info(Properties.Resources.PreparingFileCopy);
            _patcherContext.UpdateMainProgress(Properties.Resources.PreparingFileCopy, isIndeterminate: true,
                isProgressbarVisible: true);

            var copyTasks = new List<Action>();

            var destination = Path.GetDirectoryName(_clientProfile.Location);
            var source = PatchInfo.ContentDirectory;
            var files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);

            Log.Info(Properties.Resources.FilesToCopy, string.Join(", ", files.Select(Path.GetFileName)));

            var completed = 0;

            _patcherContext.UpdateMainProgress(Properties.Resources.CopyingPatchFiles,
                $"{completed}/{files.Length}", 0,
                isProgressbarVisible: true);

            foreach (var file in files)
            {
                copyTasks.Add(async () =>
                {
                    try
                    {
                        var progressReporter = _patcherContext.CreateProgressIndicator();
                        progressReporter.SetIsIndeterminate(true);
                        progressReporter.SetLeftText(Path.GetFileName(file));
                        progressReporter.SetRightText(Properties.Resources.Copying);

                        var destinationFileName = file.Replace(source, destination);

                        if (File.Exists(destinationFileName))
                            File.Move(destinationFileName, destinationFileName + ".old");

                        // In case the target file is missing...
                        if (!File.Exists(file))
                        {
                            Log.Info(Properties.Resources.FileFilenameIsMissing, file);
                            File.Move(destinationFileName + ".old", destinationFileName);
                        }

                        if (!Directory.Exists(Path.GetDirectoryName(destinationFileName)))
                            Directory.CreateDirectory(Path.GetDirectoryName(destinationFileName));

                        File.Move(file, destinationFileName);

                        File.Delete(destinationFileName + ".old");

                        Interlocked.Increment(ref completed);

                        _patcherContext.UpdateMainProgress(Properties.Resources.CopyingPatchFiles,
                            $"{completed}/{files.Length}", completed / (double)files.Length * 100.0,
                            isProgressbarVisible: true);

                        _patcherContext.DestroyProgressIndicator(progressReporter);
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                    }
                });
            }

            // TODO: Make use tasks... no more Parallel.Invoke either
            Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = 10 }, copyTasks.ToArray());

            //Parallel.Invoke(new ParallelOptions
            //{
            //    MaxDegreeOfParallelism = 10, // TODO: Setting for max downloads at one time
            //    CancellationToken = new CancellationToken() // TODO: Store and pass global cancellation token
            //},
            //    files.Select(file =>
            //            _patcherContext.CreateProgressIndicator(
            //                new ProgressReporterViewModel(file, file.Replace(source, destination))))
            //        .Select(reporter => new FileCopier(reporter))
            //        .Select(copier => (Action)(() =>
            //        {
            //            try
            //            {
            //                // Create the individual indicator for the file action
            //                PatcherContext.Instance.OnCreateProgressIndicator(copier.ProgressReporter);

            //                copier.Start();

            //                // since this is ran in parrallel make sure we safely increase the completed count
            //                Interlocked.Increment(ref completed);

            //                _patcherContext.UpdateMainProgress("Copying patch files...",
            //                    $"{completed}/{files.Length}", completed / (double)files.Length * 100.0,
            //                    isProgressbarVisible: true);
            //            }
            //            catch (Exception ex)
            //            {
            //                // Log the exception in the main application's log file
            //                PatcherContext.Instance.PluginContext.LogException(ex, false);
            //            }
            //            finally
            //            {
            //                // Destroy the individual indicator the the file action
            //                // Do it in finally so that even if there is an exception it is destoryed and 
            //                // frees up memory and other stuff.
            //                PatcherContext.Instance.OnDestroyProgressIndicator(copier.ProgressReporter);
            //            }
            //        })).ToArray()
            //);

            Log.Info(Properties.Resources.CopyComplete);

            _patcherContext.UpdateMainProgress(Properties.Resources.CopyComplete);
        }

        private void CleanUp()
        {
            Log.Info("Beginning cleanup");
            _patcherContext.UpdateMainProgress(Properties.Resources.CleaningUp, isIndeterminate: true,
                isProgressbarVisible: true);

            // TODO: Setting for each part of the cleanup process
            {
                Log.Info("Deleting part files");
                _patcherContext.UpdateMainProgress("Cleaning up...", "Deleting part files",
                    isIndeterminate: true,
                    isProgressbarVisible: true);
                foreach (var file in PatchInfo.Files.Select(f => Path.Combine(PatchInfo.PatchName, f.Filename)))
                    TryDeleteFile(file);
            }

            {
                Log.Info("Deleting zip files");
                _patcherContext.UpdateMainProgress("Cleaning up...", "Deleting zip files",
                    isIndeterminate: true,
                    isProgressbarVisible: true);
                var strArray = new string[2]
                {
                    PatchInfo.ZipFilePath,
                    PatchInfo.LanguageFilePath
                };
                foreach (var file in strArray)
                    TryDeleteFile(file);
            }

            {
                Log.Info("Deleting content");
                _patcherContext.UpdateMainProgress("Cleaning up...", "Deleting content",
                    isIndeterminate: true,
                    isProgressbarVisible: true);
                TryDeleteDirectory(PatchInfo.ContentDirectory);
            }

            {
                Log.Info("Deleting empty patch folder");
                _patcherContext.UpdateMainProgress("Cleaning up...",
                    "Deleting empty patch folder", isIndeterminate: true,
                    isProgressbarVisible: true);
                TryDeleteDirectory(PatchInfo.PatchName);
            }

            Log.Info("Cleanup completed");
            _patcherContext.UpdateMainProgress("Cleaning up completed!");

            // === local functions ===
            void TryDeleteFile(string filename)
            {
                Log.Info($"Deleting {filename}");
                try
                {
                    File.Delete(filename);
                }
                catch (Exception ex)
                {
                    Log.Info(
                        $"Warning: Couldn't delete '{filename}': {ex.Message}");
                }
            }

            void TryDeleteDirectory(string directory)
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (Exception ex)
                {
                    Log.Info(
                        $"Warning: Couldn't delete '{directory}': {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    ///     Manages patches so that
    /// </summary>
    public class DownloadQueue
    {
        private readonly OfficialPatchInfo _officialPatchInfo;
        private readonly List<PatchInfo> _patchInfos;
        private List<PatchTask> _patchTasks;
        private readonly PatcherContext _patcherContext;
        private readonly IClientProfile _clientProfile;

        /// <summary>
        ///     Creates a download queue but does not initialize it
        /// </summary>
        /// <param name="patcherContext"></param>
        /// <param name="patchInfos"></param>
        /// <param name="officialPatchInfo"></param>
        public DownloadQueue(PatcherContext patcherContext, IClientProfile clientProfile, List<PatchInfo> patchInfos, OfficialPatchInfo officialPatchInfo)
        {
            _patcherContext = patcherContext;
            _clientProfile = clientProfile;
            _patchInfos = patchInfos;
            _patchTasks = new List<PatchTask>();
            _officialPatchInfo = officialPatchInfo;
        }

        /// <summary>
        ///     Sets up the download queue for downloading and ensures patch ordering
        /// </summary>
        public void Initialize()
        {
            foreach (var patchInfo in _patchInfos)
            {
                _patchTasks.Add(new PatchTask(_patcherContext, _clientProfile, patchInfo, _officialPatchInfo));
                _patchTasks = _patchTasks.OrderBy(task => task.PatchInfo.EndVersion).ToList();
            }
        }

        /// <summary>
        ///     Begins downloading each "patch" in order
        /// </summary>
        public void Start()
        {
            foreach (var patchTask in _patchTasks) patchTask.Start();
        }
    }
}
