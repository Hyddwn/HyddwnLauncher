using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using HyddwnLauncher.Extensibility.Helpers;

namespace HyddwnLauncher.Patcher.Legacy.Core
{
    public class PatchDownloader
    {
        private string _clientDirectory;

        private DownloadQueue _downloadQueue;
        private readonly OfficialPatchInfo _officialPatchInfo;
        private bool _prepared;

        /// <summary>
        ///     Creates a new instance of PatcherDownloader that accepts a PatchSequence
        /// </summary>
        /// <param name="patchSequence">PatchSequence representing a collection of patches</param>
        public PatchDownloader(PatchSequence patchSequence, OfficialPatchInfo officialPatchInfo) : this()
        {
            _officialPatchInfo = officialPatchInfo;
            Patches.AddRange(patchSequence.Patches);
        }

        /// <summary>
        ///     Creates a new instance of PatchDownloader that accepts a PatchInfo
        /// </summary>
        /// <param name="patchInfo">PatchInfo representing a single patch.</param>
        public PatchDownloader(PatchInfo patchInfo) : this()
        {
            Patches.Add(patchInfo);
        }

        private PatchDownloader()
        {
            Patches = new List<PatchInfo>();
        }

        private List<PatchInfo> Patches { get; }

        /// <summary>
        ///     Initializes the download queue and stores each patch in order
        /// </summary>
        public void Prepare()
        {
            // Get the directory for the client for conviencnce
            // TODO: Actually pass to download queue. (Might not need since already in working dir)
            _clientDirectory = Path.GetDirectoryName(PatcherContext.Instance.ClientProfile.Location);

            _downloadQueue = new DownloadQueue(Patches, _officialPatchInfo);
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
                PatcherContext.Instance.PluginContext.LogString(
                    "[WARNING]: Patch Downloader was not prepared ahead of time, calling prepare...");
                Prepare();
            }

            // TODO: UI calls!

            _downloadQueue.Start();
        }
    }

    public class PatchTask
    {
        /// <summary>
        ///     Creates a task which represent an individual patch operation
        /// </summary>
        /// <param name="patchInfo"></param>
        /// <param name="officialPatchInfo"></param>
        public PatchTask(PatchInfo patchInfo, OfficialPatchInfo officialPatchInfo)
        {
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

            Copy();
            CleanUp();
        }

        public List<PatchFileInfo> Verify()
        {
            PatcherContext.Instance.PluginContext.LogString("Beginning file verification");
            PatcherContext.Instance.PluginContext.UpdateMainProgress("Beginning file verification...",
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

                    PatcherContext.Instance.PluginContext.UpdateMainProgress($"Verifying {patchFileInfo.Filename}...",
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
                                PatcherContext.Instance.PluginContext.LogString(
                                    $"MD5 hash failed for {patchFileInfo.Filename}. Expected {patchFileInfo.Md5Hash}, got {actualHash}");
                                list.Add(patchFileInfo);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        PatcherContext.Instance.PluginContext.LogString(
                            $"Failed to verify {patchFileInfo.Filename} due to exception: {ex.Message}");
                        list.Add(patchFileInfo);
                    }
                }
            }

            PatcherContext.Instance.PluginContext.LogString("File verification complete");
            PatcherContext.Instance.PluginContext.UpdateMainProgress("File verification complete!");

            return list;
        }

        private void Download(List<PatchFileInfo> files)
        {
            // Log the beginning of the download
            PatcherContext.Instance.PluginContext.LogString(
                "Beginnning download");

            PatcherContext.Instance.PluginContext.UpdateMainProgress("Beginnning download...", isIndeterminate: true,
                isProgressbarVisible: true);

            // create the actions that will run in parrallel
            var list = new List<Action>();

            var completed = 0;

            foreach (var patchFileInfo in files)
            {
                var ftpAddress = OfficialPatchInfo.MainFtp;
                if (!ftpAddress.EndsWith("/")) ftpAddress += "/";

                // Im kinda stuck on how I wanna do this haha

                var downloader = new FileDownloader(new ProgressReporterViewModel(), patchFileInfo.RemoteName,
                    patchFileInfo.Filename, patchFileInfo.Size, CancellationToken.None);

                list.Add(() =>
                {
                    try
                    {
                        // Create the individual indicator for the file action
                        PatcherContext.Instance.OnCreateProgressIndicator(downloader.ProgressReporter);

                        downloader.Start();

                        // since this is ran in parrallel make sure we safely increase the completed count
                        Interlocked.Increment(ref completed);

                        PatcherContext.Instance.PluginContext.UpdateMainProgress("Downloading files...",
                            $"{completed}/{files.Count}", completed / (double) files.Count * 100.0,
                            isProgressbarVisible: true);
                    }
                    catch
                    {
                        // Don't log anything here right away, we try again until we can't go without an exception since
                        // This allows us to check for partial files and redownload them if needed.
                    }
                    finally
                    {
                        // Destroy the individual indicator for the file action
                        PatcherContext.Instance.OnDestroyProgressIndicator(downloader.ProgressReporter);
                    }
                });
            }

            Parallel.Invoke(new ParallelOptions
            {
                MaxDegreeOfParallelism = 10,
                CancellationToken = CancellationToken.None
            }, list.ToArray());

            PatcherContext.Instance.PluginContext.LogString(
                "Download complete");

            PatcherContext.Instance.PluginContext.UpdateMainProgress("Download complete!");
        }

        private void Combine()
        {
            PatcherContext.Instance.PluginContext.LogString("Beginning file combine");
            PatcherContext.Instance.PluginContext.UpdateMainProgress("Beginning file combine...", isIndeterminate: true,
                isProgressbarVisible: true);

            var stopwatch = new Stopwatch();
            var buffer = new byte[4096];
            var completed = 0;
            var totalRead = 0L;
            PatcherContext.Instance.PluginContext.LogString($"Opening {PatchInfo.ZipFilePath} for output.");

            using (var fileStream = new FileStream(PatchInfo.ZipFilePath, FileMode.Create))
            {
                stopwatch.Start();
                var timer = new Timer(callback =>
                {
                    PatcherContext.Instance.PluginContext.UpdateMainProgress("Combining files...",
                        $"{completed}/{PatchInfo.Files.Count} at {ByteSizeHelper.ToString(totalRead / stopwatch.Elapsed.TotalSeconds)}/s",
                        completed / (double) PatchInfo.Files.Count * 100.0);
                }, null, 200, 200);

                foreach (var patchFileInfo in PatchInfo.Files)
                {
                    PatcherContext.Instance.PluginContext.LogString($"Adding {patchFileInfo.Filename}");
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

            PatcherContext.Instance.PluginContext.LogString($"Patch combine successful.");
            PatcherContext.Instance.PluginContext.UpdateMainProgress("Patch combine complete!");
        }

        private void Extract()
        {
            // TODO: Fill out
        }

        private void Copy()
        {
            PatcherContext.Instance.PluginContext.LogString("Preparing to copy patch files");
            PatcherContext.Instance.PluginContext.UpdateMainProgress("Preparing file copy...", isIndeterminate: true,
                isProgressbarVisible: true);

            var destination = Path.GetDirectoryName(PatcherContext.Instance.ClientProfile.Location);
            var source = PatchInfo.ContentDirectory;
            var files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);

            PatcherContext.Instance.PluginContext.LogString(
                $"Files to copy: {string.Join(", ", files.Select(Path.GetFileName))}");

            var completed = 0;

            Parallel.Invoke(new ParallelOptions
                {
                    MaxDegreeOfParallelism = 10, // TODO: Setting for max downloads at one time
                    CancellationToken = new CancellationToken() // TODO: Store and pass global cancellation token
                },
                files.Select(file =>
                        PatcherContext.Instance.OnCreateProgressIndicator(
                            new ProgressReporterViewModel(file, file.Replace(source, destination))))
                    .Select(reporter => new FileCopier(reporter))
                    .Select(copier => (Action) (() =>
                    {
                        try
                        {
                            // Create the individual indicator for the file action
                            PatcherContext.Instance.OnCreateProgressIndicator(copier.ProgressReporter);

                            copier.Start();

                            // since this is ran in parrallel make sure we safely increase the completed count
                            Interlocked.Increment(ref completed);

                            PatcherContext.Instance.PluginContext.UpdateMainProgress("Copying patch files...",
                                $"{completed}/{files.Length}", completed / (double) files.Length * 100.0,
                                isProgressbarVisible: true);
                        }
                        catch (Exception ex)
                        {
                            // Log the exception in the main application's log file
                            PatcherContext.Instance.PluginContext.LogException(ex, false);
                        }
                        finally
                        {
                            // Destroy the individual indicator the the file action
                            // Do it in finally so that even if there is an exception it is destoryed and 
                            // frees up memory and other stuff.
                            PatcherContext.Instance.OnDestroyProgressIndicator(copier.ProgressReporter);
                        }
                    })).ToArray()
            );

            PatcherContext.Instance.PluginContext.LogString(
                "Copy complete");

            PatcherContext.Instance.PluginContext.UpdateMainProgress("Copy complete!");
        }

        private void CleanUp()
        {
            PatcherContext.Instance.PluginContext.LogString("Beginning cleanup");
            PatcherContext.Instance.PluginContext.UpdateMainProgress("Cleaning up...", isIndeterminate: true,
                isProgressbarVisible: true);

            // TODO: Setting for each part of the cleanup process
            {
                PatcherContext.Instance.PluginContext.LogString("Deleting part files");
                PatcherContext.Instance.PluginContext.UpdateMainProgress("Cleaning up...", "Deleting part files",
                    isIndeterminate: true,
                    isProgressbarVisible: true);
                foreach (var file in PatchInfo.Files.Select(f => Path.Combine(PatchInfo.PatchName, f.Filename)))
                    TryDeleteFile(file);
            }

            {
                PatcherContext.Instance.PluginContext.LogString("Deleting zip files");
                PatcherContext.Instance.PluginContext.UpdateMainProgress("Cleaning up...", "Deleting zip files",
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
                PatcherContext.Instance.PluginContext.LogString("Deleting content");
                PatcherContext.Instance.PluginContext.UpdateMainProgress("Cleaning up...", "Deleting content",
                    isIndeterminate: true,
                    isProgressbarVisible: true);
                TryDeleteDirectory(PatchInfo.ContentDirectory);
            }

            {
                PatcherContext.Instance.PluginContext.LogString("Deleting empty patch folder");
                PatcherContext.Instance.PluginContext.UpdateMainProgress("Cleaning up...",
                    "Deleting empty patch folder", isIndeterminate: true,
                    isProgressbarVisible: true);
                TryDeleteDirectory(PatchInfo.PatchName);
            }

            PatcherContext.Instance.PluginContext.LogString("Cleanup completed");
            PatcherContext.Instance.PluginContext.UpdateMainProgress("Cleaning up completed!");

            // === local functions ===
            void TryDeleteFile(string filename)
            {
                PatcherContext.Instance.PluginContext.LogString($"Deleting {filename}");
                try
                {
                    File.Delete(filename);
                }
                catch (Exception ex)
                {
                    PatcherContext.Instance.PluginContext.LogString(
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
                    PatcherContext.Instance.PluginContext.LogString(
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


        /// <summary>
        ///     Creates a download queue but does not initialize it
        /// </summary>
        /// <param name="patchInfos"></param>
        /// <param name="officialPatchInfo"></param>
        public DownloadQueue(List<PatchInfo> patchInfos, OfficialPatchInfo officialPatchInfo)
        {
            _patchInfos = patchInfos;
            _officialPatchInfo = officialPatchInfo;
        }

        /// <summary>
        ///     Sets up the download queue for downloading and ensures patch ordering
        /// </summary>
        public void Initialize()
        {
            foreach (var patchInfo in _patchInfos)
            {
                _patchTasks.Add(new PatchTask(patchInfo, _officialPatchInfo));
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