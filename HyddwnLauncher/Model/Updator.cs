using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using HyddwnLauncher.Network;
using HyddwnLauncher.Util;
using Ionic.Zip;

namespace HyddwnLauncher.Model
{
    /// <summary>
    ///     Logic for application update system
    /// </summary>
    public class Updator
    {
        private readonly Dictionary<string, string> _updateInfo;


        /// <summary>
        ///     Create and updator to be used anywehere within the application
        /// </summary>
        public Updator()
        {
            _updateInfo = new Dictionary<string, string>();
        }

        /// <summary>
        ///     Called to notify the UI of progress changes
        /// </summary>
        public event Action<double, string, bool, bool> ProgressChanged;

        /// <summary>
        ///     Called to notify UI of status changes
        /// </summary>
        public event Action<string> StatusChanged;

        /// <summary>
        ///     Called when the updater is finished and is ready to close wrapping object.
        /// </summary>
        public event Action CloseRequested;

        /// <summary>
        ///     Check for updates to the launcher
        /// </summary>
        public async void Run()
        {
            OnStatusChanged("Checking for updates...");
            OnProgressChanged();

            // Minor hack because the following operation makes the splashscreen lag a little
            await Task.Delay(1500);

            var updaterUpdateAvailable = await CheckForUpdaterUpdate();
            if (updaterUpdateAvailable)
            {
                OnStatusChanged("Downloading Updater Update...");
                OnProgressChanged(isIndeterminate: false);

                await DownloadUpdate(_updateInfo["UpdateLink"], _updateInfo["UpdateFile"]);
            }

            var launcherUpdateAvailable = await CheckForLauncherUpdate();
            if (launcherUpdateAvailable)
            {
                var result = MessageBox.Show(
                    "There is a newer version of this launcher avaiable for download.\r\n\r\n" +
                    $"Current: {_updateInfo["Current"]}\r\n" +
                    $"New: {_updateInfo["Version"]}\r\n\r\n" +
                    " Would you like to download it now?", "Update Available!", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    await FinializeUpdateCheck();
                    return;
                }

                OnStatusChanged("Downloading Update...");
                OnProgressChanged(isIndeterminate: false);

                await DownloadUpdate(_updateInfo["Link"], _updateInfo["File"], true);
                return;
            }

            await FinializeUpdateCheck();
        }

        private async Task FinializeUpdateCheck()
        {
            OnStatusChanged("Launching...");
            OnProgressChanged(isVisible: false);
            await Task.Delay(500);

            LaunchApplication();
        }

        private void LaunchApplication()
        {
            OnCloseRequested();
        }

        private async Task DownloadUpdate(string url, string filename, bool requiresRestart = false)
        {
            OnProgressChanged(isIndeterminate: false);

            await AsyncDownloader.DownloadFileWithWaitlessCallbackAsync(url, filename, DownloadUpdateProgressChanged);

            DownloadUpdateCompleted(requiresRestart);
        }

        //Stub
        private void DownloadUpdateProgressChanged(double progress, string status)
        {
            OnProgressChanged(progress, status, isIndeterminate: false);
        }

        //Stub
        private void DownloadUpdateCompleted(bool requiresRestart)
        {
            if (requiresRestart)
            {
                OnStatusChanged("Download complete!");
                OnProgressChanged(progressText: "Updating...");
                var processinfo = new ProcessStartInfo
                {
                    Arguments =
                        $"{"\"" + _updateInfo["File"] + "\""} {_updateInfo["SHA256"]} {"\"" + _updateInfo["Execute"] + "\""}",
                    FileName = "Updater.exe"
                };

                // Process.Start is different that var process = new Process(processinfo).Start();
                // It launches the process as it's own process instead of a child process.
                Process.Start(processinfo);

                Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
                return;
            }

            OnStatusChanged("Updater Download complete!");
            OnProgressChanged(progressText: "Applying...");

            using (var zipFile = ZipFile.Read($".\\{_updateInfo["UpdateFile"]}"))
            {
                zipFile.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                zipFile.ExtractAll(".\\");
            }

            File.Delete($".\\{_updateInfo["UpdateFile"]}");

            OnProgressChanged(isVisible: false);
            OnStatusChanged("Updater Update Complete!");
        }

        private void OnProgressChanged(double progress = 0, string progressText = "", bool isVisible = true,
            bool isIndeterminate = true)
        {
            ProgressChanged?.Raise(progress, progressText, isVisible, isIndeterminate);
        }

        private void OnStatusChanged(string text)
        {
            StatusChanged?.Raise(text);
        }

        private void OnCloseRequested()
        {
            CloseRequested?.Raise();
        }

        private async Task<bool> CheckForUpdaterUpdate()
        {
            return await UpdateCheckInternal("http://launcher.hyddwnproject.com/update/version", "UpdateCurrent", "UpdateVersion", DownloadType.Updater);
        }

        private async Task<bool> CheckForLauncherUpdate()
        {
            return await UpdateCheckInternal("http://launcher.hyddwnproject.com/version", "Current", "Version", DownloadType.Launcher);
        }

        private async Task<bool> UpdateCheckInternal(string url, string currentVersionString,
            string remoteVersionString, DownloadType downloadType)
        {
            try
            {
                await Task.Delay(100);
                var webClient = new WebClient();

                using (var fileReader = new FileReader(webClient.OpenRead(url)))
                {
                    foreach (var str in fileReader)
                    {
                        int length;
                        if ((length = str.IndexOf(':')) >= 0)
                            _updateInfo[str.Substring(0, length).Trim()] = str.Substring(length + 1).Trim();
                    }
                }

                Version localVersion = new Version();

                switch (downloadType)
                {
                    case DownloadType.Launcher:
                        localVersion = Assembly.GetExecutingAssembly().GetName().Version;
                        break;
                    case DownloadType.Updater:
                        if (File.Exists(".\\Updater.exe"))
                        {
                            try
                            {
                                localVersion = AssemblyName.GetAssemblyName(".\\Updater.exe").Version;
                            }
                            catch (Exception ex)
                            {
                                Log.Warning($"Failed to detect the version of the updater: {ex.Message}");
                                // Just in case
                                localVersion = new Version();
                            }
                        }   
                        break;
                    case DownloadType.Plugin:
                    case DownloadType.PluginData:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(downloadType), downloadType, null);
                }

                _updateInfo[currentVersionString] = localVersion.ToString();

                var remoteVersion = new Version(_updateInfo[remoteVersionString]);

                return localVersion < remoteVersion;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"Unable to check for {downloadType} updates.");
                return false;
            }
        }
    }
}