using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HyddwnLauncher.Util;
using Ionic.Zip;
using MahApps.Metro.Controls.Dialogs;

namespace HyddwnLauncher.Patching
{
    public class Patcher
    {
        private const int VersionOvershoot = 5;
        private readonly MainWindow _mWindow;

        public Patcher(MainWindow mWindow, OfficialPatchInfo opi)
        {
            _mWindow = mWindow;
            PatchInfo = opi;
        }

        public OfficialPatchInfo PatchInfo { get; }

        public static void WriteVersion(int version)
        {
            Log.Info("Writing version {0} to version.dat", version);
            File.WriteAllBytes("version.dat", BitConverter.GetBytes(version));
        }

        public static int ReadVersion()
        {
            Log.Info("Reading version from version.dat");
            try
            {
                return BitConverter.ToInt32(File.ReadAllBytes("version.dat"), 0);
            }
            catch
            {
                return 0;
            }
        }

        public void Patch(PatchSequence sequence)
        {
            Log.Info("Beginning info of sequence : {0}", sequence);
            foreach (var patch in sequence.Patches)
                Patch(patch);
        }

        private void Patch(PatchInfo patch)
        {
            Log.Info("Beginning info {0}", patch);
            _mWindow.MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Visible);
            _mWindow.MainProgressReporter.SetProgressBar(0.0);
            Directory.CreateDirectory(patch.PatchName);
            var files = patch.Files;
            try
            {
                do
                {
                    files = VerifyFiles(patch, files);
                    DownloadFiles(patch, files);
                } while (files.Count != 0);
                CombineFiles(patch);
                Extract(patch);
                if (PatchInfo["lang"] != null)
                    DownloadAndExtractLanguage(patch);
                Copy(patch);
                WriteVersion(patch.EndVersion);
                _mWindow.ClientVersion.SetTextBlockSafe("{0}", patch.EndVersion);
            }
            catch (Exception ex)
            {
                if (!_mWindow.TokenSource.Token.IsCancellationRequested)
                {
                    Log.Exception(ex, "Error while patching!");
                    throw;
                }
                else
                {
                    try
                    {
                    }
                    finally
                    {
                        Log.Info("Current info cancelled.");
                    }
                }
            }
            finally
            {
                Cleanup(patch);
            }
            Log.Info("Patch completed!");
            _mWindow.MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Hidden);
            _mWindow.MainProgressReporter.SetProgressBar(0.0);
            _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("");
        }

        private void Cleanup(PatchInfo info)
        {
            Log.Info("Beginning clean up");
            if (_mWindow.LauncherContext.Settings.DeletePartFiles)
            {
                Log.Info("Deleting part files...");
                _mWindow.MainProgressReporter.SetProgressBar(0.0);
                _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Cleaning part files");
                _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
                foreach (var file in info.Files.Select(f => Path.Combine(info.PatchName, f.Filename)))
                    TryDeleteFile(file);
                Log.Info("Done deleting part files");
            }
            if (_mWindow.LauncherContext.Settings.DeleteZips)
            {
                _mWindow.MainProgressReporter.SetProgressBar(0.0);
                _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Cleaning zip files");
                _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
                var strArray = new string[2]
                {
                    info.ZipFilePath,
                    info.LanguageFilePath
                };
                foreach (var file in strArray)
                    TryDeleteFile(file);
            }
            if (_mWindow.LauncherContext.Settings.DeleteContent)
            {
                Log.Info("Deleting content");
                _mWindow.MainProgressReporter.SetProgressBar(0.0);
                _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Cleaning temporary content directory");
                _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
                TryDeleteDirectory(info.ContentDirectory);
                Log.Info("Content deleted");
            }
            if (_mWindow.LauncherContext.Settings.DeleteContent && _mWindow.LauncherContext.Settings.DeletePartFiles &&
                _mWindow.LauncherContext.Settings.DeleteZips)
            {
                Log.Info("Removing empty patch folder {0}", info.PatchName);
                _mWindow.MainProgressReporter.SetProgressBar(0.0);
                _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Cleaning patch folder");
                _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
                TryDeleteDirectory(info.PatchName);
            }
            _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(false);
            Log.Info("Clean up finished");
        }

        private void TryDeleteDirectory(string p)
        {
            try
            {
                Directory.Delete(p, true);
            }
            catch (Exception ex)
            {
                Log.Warning("Can't delete {0}: {1}", p, ex.Message);
            }
        }

        private void TryDeleteFile(string file)
        {
            Log.Info("Deleting {0}", file);
            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                Log.Warning("Can't delete {0}: {1}", file, ex.Message);
            }
        }

        private void Copy(PatchInfo info)
        {
            Log.Info("Preparing to copy patch files");
            _mWindow.MainProgressReporter.SetProgressBar(0.0);
            _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Copying patch files...");
            var dst = Environment.CurrentDirectory;
            var src = info.ContentDirectory;
            var files = Directory.GetFiles(src, "*", SearchOption.AllDirectories);
            Log.Info("Files to copy: [{0}]", string.Join(", ", files.Select(Path.GetFileName)));
            var completed = 0;
            Parallel.Invoke(new ParallelOptions
                {
                    MaxDegreeOfParallelism = _mWindow.LauncherContext.Settings.ConnectionLimit,
                    CancellationToken = _mWindow.TokenSource.Token
                },
                files.Select(file => _mWindow.CreateCopierInstance(file, file.Replace(src, dst)))
                    .Select(copier => new
                    {
                        copier
                    }).Select(o => (Action) (() =>
                    {
                        try
                        {
                            Application.Current.Dispatcher.Invoke(
                                () => _mWindow.Reporters.Add(o.copier));
                            o.copier.Start();
                            _mWindow.MainProgressReporter.SetProgressBar(
                                (int) (Interlocked.Increment(ref completed) / (double) files.Length * 100.0));
                            _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe(
                                string.Concat("Copying patch files... ", completed, " / ",
                                    files.Length));
                        }
                        catch (Exception ex)
                        {
                            Log.Exception(ex, ex.Message);
                            throw;
                        }
                        finally
                        {
                            Application.Current.Dispatcher.Invoke(
                                () => _mWindow.Reporters.Remove(o.copier));
                        }
                    })).ToArray());
            Log.Info("Copy done");
        }

        private void DownloadAndExtractLanguage(PatchInfo info)
        {
            Log.Info("Beginning download of language pack to {0}", info.LanguageFilePath);
            _mWindow.MainProgressReporter.SetProgressBar(0.0);
            _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe(
                $"Downloading language pack at {"0b"}/s");
            _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
            var stopwatch = new Stopwatch();
            var buffer = new byte[4096];
            var totalRead = 0;
            var str = PatchInfo.MainFtp;
            if (!str.EndsWith("/"))
                str += "/";
            var address = str + info.EndVersion + "/" + info.EndVersion + "_language.p_";
            using (var fileStream = new FileStream(info.LanguageFilePath, FileMode.Create))
            {
                using (var webClient = new WebClient())
                {
                    using (var stream = webClient.OpenRead(address))
                    {
                        stopwatch.Start();
                        var timer = new Timer(o =>
                        {
                            _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe(
                                $"Downloading language pack at {ByteSizeHelper.ToString(totalRead / stopwatch.Elapsed.TotalSeconds)}/s");
                            _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
                        }, null, 200, 200);
                        int count;
                        while (stream != null && (count = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            _mWindow.TokenSource.Token.ThrowIfCancellationRequested();
                            fileStream.Write(buffer, 0, count);
                            totalRead += count;
                        }
                        stopwatch.Stop();
                        timer.Change(-1, -1);
                        timer.Dispose();
                    }
                }
            }
            Log.Info("Download complete. Speed: {0}/s",
                ByteSizeHelper.ToString(totalRead / stopwatch.Elapsed.TotalSeconds));
            Log.Info("Extracting language pack to {0}", info.LanguageContentDir);
            _mWindow.MainProgressReporter.SetProgressBar(0.0);
            _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Extracting language pack");
            _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(false);
            using (var zipFile = ZipFile.Read(info.LanguageFilePath))
            {
                zipFile.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                zipFile.ExtractAll(info.LanguageContentDir);
            }
            _mWindow.TokenSource.Token.ThrowIfCancellationRequested();
            Log.Info("Extraction complete");
        }

        private void DownloadFiles(PatchInfo info, List<PatchFileInfo> files)
        {
            Log.Info("Beginning download of {0} files: [{1}]", files.Count,
                string.Join(", ", files.Select(f => f.Filename)));
            _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Downloading files...");
            _mWindow.MainProgressReporter.SetProgressBar(0.0);
            var list = new List<Action>();
            var completed = 0;
            Application.Current.Dispatcher.Invoke(() => _mWindow.MainTabControl.SelectedIndex = 1);
            foreach (var patchFileInfo in files)
            {
                var str = PatchInfo.MainFtp;
                if (!str.EndsWith("/"))
                    str += "/";
                var dl =
                    _mWindow.CreateDownloaderInstance(
                        string.Concat(str, info.EndVersion, "/",
                            patchFileInfo.RemoteName), Path.Combine(info.PatchName, patchFileInfo.Filename),
                        patchFileInfo.Size);
                list.Add(() =>
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(
                            () => _mWindow.Reporters.Add(dl));
                        dl.Start();
                        _mWindow.MainProgressReporter.SetProgressBar(
                            (int) (Interlocked.Increment(ref completed) / (double) files.Count * 100.0));
                        _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe(
                            string.Concat("Downloading files... ", completed, " / ",
                                files.Count));
                    }
                    catch
                    {
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(() => _mWindow.Reporters.Remove(dl));
                    }
                });
            }
            Parallel.Invoke(new ParallelOptions
            {
                MaxDegreeOfParallelism = _mWindow.LauncherContext.Settings.ConnectionLimit,
                CancellationToken = _mWindow.TokenSource.Token
            }, list.ToArray());
            Log.Info("All downloads complete.");
        }

        private void Extract(PatchInfo info)
        {
            Log.Info("Extracting patch zip to {0}", info.ContentDirectory);
            Directory.CreateDirectory(info.ContentDirectory);
            using (var zipFile = ZipFile.Read(info.ZipFilePath))
            {
                zipFile.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                zipFile.ExtractProgress += zipFile_ExtractProgress;
                zipFile.ExtractAll(info.ContentDirectory);
            }
            Log.Info("Extract complete");
        }

        private void zipFile_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_BeforeExtractEntry)
            {
                try
                {
                    _mWindow.TokenSource.Token.ThrowIfCancellationRequested();
                }
                catch
                {
                    e.Cancel = true;
                    throw;
                }
                Log.Info("Extracting {0}", e.CurrentEntry.FileName);
                _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Extracting " + e.CurrentEntry.FileName);
            }
            else
            {
                if (e.EventType != ZipProgressEventType.Extracting_AfterExtractEntry)
                    return;
                _mWindow.MainProgressReporter.SetProgressBar(e.EntriesExtracted /
                                                             (double) e.EntriesTotal * 100.0);
            }
        }


        private List<PatchFileInfo> VerifyFiles(PatchInfo info, List<PatchFileInfo> files)
        {
            Log.Info("Beginning verification of {0} files: [{1}]", files.Count,
                string.Join(", ", files.Select(f => f.Filename)));
            _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Verifying files...");
            var list = new List<PatchFileInfo>();
            var num = 0;
            using (var md5 = MD5.Create())
            {
                foreach (var patchFileInfo in files)
                {
                    try
                    {
                        _mWindow.TokenSource.Token.ThrowIfCancellationRequested();
                    }
                    catch (Exception)
                    {
                        break;
                    }
                    _mWindow.MainProgressReporter.RighTextBlock.SetTextBlockSafe("Verifying {0}",
                        patchFileInfo.Filename);

                    var num1 = num;

                    try
                    {
                        using (
                            var fileStream = new FileStream(Path.Combine(info.PatchName, patchFileInfo.Filename),
                                FileMode.Open))
                        {
                            var str = BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLower();
                            if (str != patchFileInfo.Md5Hash)
                            {
                                Log.Warning("MD5 fail for {0}. Expected hash: {1}\tGot hash: {2}",
                                    patchFileInfo.Filename, patchFileInfo.Md5Hash, str);
                                list.Add(patchFileInfo);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("Failed verifaction of {0} due to exception: {1}", patchFileInfo.Filename,
                            ex.Message);
                        list.Add(patchFileInfo);
                    }
                    finally
                    {
                        ++num;
                        _mWindow.MainProgressReporter.SetProgressBar(num1 / (double) files.Count * 100.0);
                    }
                }
                _mWindow.MainProgressReporter.RighTextBlock.SetTextBlockSafe("");
            }
            Log.Info("Verification complete. {0} hashfails.", list.Count);
            return list;
        }

        private void CombineFiles(PatchInfo info)
        {
            Log.Info("Beginning file combine.");
            _mWindow.MainProgressReporter.SetProgressBar(0.0);
            _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe(
                $"Combining patch files... {0} out of {info.Files.Count} at {"0b"}/s");
            var stopwatch = new Stopwatch();
            var buffer = new byte[4096];
            var completed = 0;
            var totalRead = 0L;
            Log.Info("Opening {0} for output", info.ZipFilePath);
            using (var fileStream1 = new FileStream(info.ZipFilePath, FileMode.Create))
            {
                stopwatch.Start();
                var timer = new Timer(o =>
                {
                    _mWindow.MainProgressReporter.SetProgressBar(
                        (int) (completed / (double) info.Files.Count * 100.0));
                    _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe(
                        $"Combining patch files... {completed} out of {info.Files.Count} at {ByteSizeHelper.ToString(totalRead / stopwatch.Elapsed.TotalSeconds)}/s");
                }, null, 200, 200);
                foreach (var patchFileInfo in info.Files)
                {
                    Log.Info("Adding {0}", patchFileInfo.Filename);
                    using (
                        var fileStream2 = new FileStream(Path.Combine(info.PatchName, patchFileInfo.Filename),
                            FileMode.Open))
                    {
                        int count;
                        while ((count = fileStream2.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            _mWindow.TokenSource.Token.ThrowIfCancellationRequested();
                            fileStream1.Write(buffer, 0, count);
                            totalRead += count;
                        }
                    }
                    ++completed;
                }
                timer.Change(-1, -1);
                timer.Dispose();
            }
            Log.Info("Patch combine success");
            _mWindow.MainProgressReporter.SetProgressBar(100.0);
            //TODO: Correct by adding ByteSizeHelper output for speed
            _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe(
                $"Combining patch files... {info.Files.Count} out of {info.Files.Count} at {""}/s");
        }

        public async void RedownloadLanugagePack()
        {
            try
            {
                var num1 = ReadVersion();
                var str1 = PatchInfo.MainFtp;
                if (!str1.EndsWith("/"))
                    str1 += "/";
                var address = str1 + num1 + "/" + num1 + "_language.p_";
                var str2 = "lang.tmp";
                Log.Info("Redownloading language pack from {0} to {1}", address, str2);
                _mWindow.MainProgressReporter.SetProgressBar(0.0);
                _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Downloading language pack");
                _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(address, str2);
                }

                Log.Info("Extracting lang pack");
                _mWindow.MainProgressReporter.SetProgressBar(0.0);
                _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Extracting language pack");
                _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);

                using (var zipFile = ZipFile.Read(str2))
                {
                    zipFile.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                    zipFile.ExtractAll("package");
                }
                Log.Info("Deleting temporary file");
                _mWindow.MainProgressReporter.SetProgressBar(0.0);
                _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Cleaning up");
                _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
                File.Delete(str2);
                Log.Info("Redownload complete");
                _mWindow.MainProgressReporter.SetProgressBar(0.0);
                _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("");
                _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(false);
                _mWindow.MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Hidden);
                await
                    _mWindow.Dispatcher.Invoke(
                        async () => await _mWindow.ShowMessageAsync("Complete", "Redownload coplete."));
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Unable to download launguage pack");
                _mWindow.MainProgressReporter.SetProgressBar(0.0);
                _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("");
                _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(false);
                _mWindow.MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Hidden);
                await _mWindow.Dispatcher.Invoke(async () =>
                    await _mWindow.ShowMessageAsync("Failed", $"Redownload of launguage pack failed:\r\n{ex.Message}"));
            }
        }

        public PatchSequence FindSequence(int bottomVersion = -1, int topVersion = -1)
        {
            if (bottomVersion == -1)
                bottomVersion = ReadVersion();
            if (topVersion == -1)
                topVersion = PatchInfo.MainVersion;
            Log.Info("Attempting to find sequence from {0} to {1}", bottomVersion, topVersion);
            var list = new List<PatchInfo>();
            var format = "Attempting to patch from {0} to {1}. Currently checking {2}.";
            try
            {
                using (var wc = new WebClient())
                {
                    if (bottomVersion == 0)
                    {
                        _mWindow.MainProgressReporter.SetProgressBar(0.0);
                        _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe(string.Format(format, 0,
                            topVersion,
                            topVersion));
                        var patchInfo = TryGetPatchInfo(wc, topVersion,
                            topVersion + "_full.txt");
                        if (patchInfo == null)
                            throw new PatchSequenceNotFoundException(0, topVersion);
                        list.Add(patchInfo);
                    }
                    else
                    {
                        var num = bottomVersion;
                        var version = topVersion;
                        while (num != topVersion)
                        {
                            _mWindow.MainProgressReporter.SetProgressBar(
                                (num - (double) bottomVersion) / (topVersion - bottomVersion));
                            _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe(string.Format(format,
                                bottomVersion, topVersion, version));
                            var patchInfo = TryGetPatchInfo(wc, version,
                                string.Concat(num, "_to_", version, ".txt"));
                            if (patchInfo != null)
                            {
                                list.Add(patchInfo);
                                num = version;
                                version += 5;
                            }
                            else if (--version == num)
                            {
                                return FindSequence(0, topVersion);
                            }
                        }
                    }
                }
            }
            finally
            {
                _mWindow.MainProgressReporter.SetProgressBar(0.0);
                _mWindow.MainProgressReporter.LeftTextBlock.SetTextBlockSafe("");
                _mWindow.MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(false);
            }
            return new PatchSequence(list);
        }

        private PatchInfo TryGetPatchInfo(WebClient wc, int version, string filename)
        {
            var str = PatchInfo.MainFtp;
            if (!str.EndsWith("/"))
                str += "/";
            var address = string.Concat(str, version, "/", filename);
            _mWindow.TokenSource.Token.ThrowIfCancellationRequested();
            Log.Info("Trying to get patch info from {0}", address);
            try
            {
                return new PatchInfo(Path.GetFileNameWithoutExtension(filename), wc.DownloadString(address));
            }
            catch
            {
                return null;
            }
        }
    }
}