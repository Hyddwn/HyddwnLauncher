using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HyddwnLauncher.Core;
using HyddwnLauncher.Network;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Model
{
    // TODO: check for updates to the small little update utility
    /// <summary>
    /// Logic for application update system
    /// </summary>
    public class Updator
    {
        private readonly Dictionary<string, string> _updateInfo;

        /// <summary>
        /// Called to notify the UI of progress changes
        /// </summary>
        public event Action<double, string, bool, bool> ProgressChanged;

        /// <summary>
        /// Called to notify UI of status changes
        /// </summary>
        public event Action<string> StatusChanged;

        /// <summary>
        /// Called when the updater is finished and is ready to close wrapping object.
        /// </summary>
        public event Action CloseRequested;


        /// <summary>
        /// Create and updator to be used anywehere within the application
        /// </summary>
        public Updator()
        {
            _updateInfo = new Dictionary<string, string>();
        }

        /// <summary>
        /// Check for updates to the launcher
        /// </summary>
        public async void Run()
        {
            OnStatusChanged("Checking for updates...");
            OnProgressChanged();

            // Minor hack because the following operation makes the splashscreen lag a little
            await Task.Delay(1500);

            var utilityUpdateAvailable = await CheckForUtilityUpdate();
            if (utilityUpdateAvailable)
            {
                OnStatusChanged("Downloading Utility Update...");
                OnProgressChanged(isIndeterminate: false);

                DownloadUpdate(_updateInfo["UpdaterLink"], _updateInfo["UpdaterFile"]);
            }

            var updateAvaialble = await CheckForApplicationUpdate();
            if (updateAvaialble)
            {
                var result = MessageBox.Show(
                    "There is a newer version of this launcher avaiable for download.\r\n\r\n" +
                    $"Current: {_updateInfo["Current"]}\r\n" +
                    $"New: {_updateInfo["Version"]}\r\n\r\n" +
                    " Would you like to download it now?", "Update Available!", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    //TODO: Skip update and launch application
                    await FinializeUpdateCheck();
                    return;
                }

                OnStatusChanged("Downloading Update...");
                OnProgressChanged(isIndeterminate: false);

                DownloadUpdate(_updateInfo["Link"], _updateInfo["File"], true);
                //TODO: Download update and run updater.
                return;
            }

            await FinializeUpdateCheck();
        }

        private async Task FinializeUpdateCheck()
        {
            OnStatusChanged("Launching...");
            OnProgressChanged(isVisible: false);
            await Task.Delay(3000);

            LaunchApplication();
        }

        private void LaunchApplication()
        {
            OnCloseRequested();
        }

        private async void DownloadUpdate(string url, string filename, bool requiresRestart = false)
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
            OnStatusChanged("Download complete!");
            OnProgressChanged(progressText: "Updating...");

            if (requiresRestart)
            {
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
            }
        }

        private void OnProgressChanged(double progress = 0, string progressText = "", bool isVisible = true, bool isIndeterminate = true)
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

        private async Task<bool> CheckForUtilityUpdate()
        {
            return await UpdateCheckInternal("http://launcher.hyddwnproject.com/updater/version", "UpdaterCurrent", "UpdaterNew");
        }

        private async Task<bool> CheckForApplicationUpdate()
        {
            return await UpdateCheckInternal("http://launcher.hyddwnproject.com/version", "Current", "Version");
        }

        private async Task<bool> UpdateCheckInternal(string url, string currentVersionString, string remoteVersionString)
        {
            try
            {
                await Task.Delay(100);
                var webClient = new WebClient();
                var localVersion = Assembly.GetExecutingAssembly().GetName().Version;

                using (var fileReader = new FileReader(webClient.OpenRead(url)))
                {
                    foreach (var str in fileReader)
                    {
                        int length;
                        if ((length = str.IndexOf(':')) >= 0)
                            _updateInfo[str.Substring(0, length).Trim()] = str.Substring(length + 1).Trim();
                    }
                }

                _updateInfo[currentVersionString] = localVersion.ToString();

                var remoteVersion = new Version(_updateInfo[remoteVersionString]);

                return localVersion < remoteVersion;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Unable to check for updates.");
                return false;
            }
        }
    }
}
