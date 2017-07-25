using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using HyddwnLauncher.Controls;
using HyddwnLauncher.Network;
using HyddwnLauncher.Patching;
using HyddwnLauncher.Util;
using Ionic.Zip;
using MahApps.Metro.Controls.Dialogs;

namespace HyddwnLauncher
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region ctor

        public MainWindow(LauncherContext launcherContext)
        {
            Instance = this;
#if DEBUG
            launcherContext.Settings.Reset();
#endif
            LauncherContext = launcherContext;
            ProfileManager = new ProfileManager();
            ProfileManager.Load();
            //Populate for the first time
            if (ProfileManager.ServerProfiles.Count == 0)
            {
                ProfileManager.ServerProfiles.Insert(0, ServerProfile.OfficialProfile);
                ProfileManager.SaveServerProfiles();
            }

            InitializeComponent();
            MinVersion = 0;
            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Hidden);
            _disableWhilePatching = new[]
            {
                (Control) LaunchButton,
                MabiVersionComboBox,
                NewProfileButton,
                ClientProfileComboBox,
                ServerProfileComboBox
            };

            _updateClose = false;
        }

        #endregion

        #region DependancyProperties

        public static readonly DependencyProperty MaxVersionProperty = DependencyProperty.Register("MaxVersion",
            typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public static readonly DependencyProperty MinVersionProperty = DependencyProperty.Register("MinVersion",
            typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        #endregion

        #region Properties

        public ProfileManager ProfileManager { get; private set; }
        public ServerProfile ActiveServerProfile { get; set; }
        public ClientProfile ActiveClientProfile { get; set; }

        public ObservableCollection<IProgressReporter> Reporters { get; private set; } =
            new ObservableCollection<IProgressReporter>();

        public LauncherContext LauncherContext { get; private set; }

        // Very bad, will need to adjust the method of access.
        public static MainWindow Instance { get; private set; }

        public CancellationTokenSource TokenSource { get; private set; }

        public int MaxVersion
        {
            get => (int) GetValue(MaxVersionProperty);
            set => SetValue(MaxVersionProperty, value);
        }

        public int MinVersion
        {
            get => (int) GetValue(MinVersionProperty);
            set => SetValue(MinVersionProperty, value);
        }

        public bool IsPatching
        {
            get => _patching;
            protected set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _patching = value;
                    foreach (var uiElement in _disableWhilePatching)
                        uiElement.IsEnabled = !value;
                    ActionButton.Content = value ? "Cancel" : "Check For Updates";
                    if (value)
                    {
                        ActionButton.Click -= UpdateButton_Click;
                        ActionButton.Click += MainCancelButton_Click;
                    }
                    else
                    {
                        ActionButton.IsEnabled = true;
                        ActionButton.Click -= MainCancelButton_Click;
                        ActionButton.Click += UpdateButton_Click;
                    }

                    TokenSource?.Dispose();
                    TokenSource = new CancellationTokenSource();
                });
            }
        }

        #endregion

        #region Fields

        private static readonly string ChatIpAddressPattern =
            @"\bchatip\:(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";

        private static readonly string AssemblyLocation = Assembly.GetExecutingAssembly().Location;
        private static readonly string Assemblypath = Path.GetDirectoryName(AssemblyLocation);
        private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
        private readonly BackgroundWorker _backgroundWorker2 = new BackgroundWorker();
        private readonly Control[] _disableWhilePatching;
        private readonly Dictionary<string, string> _updateInfo = new Dictionary<string, string>();
        private Patcher _patcher;
        private volatile bool _patching;
        private bool _updateClose;
        private bool _settingUpProfile;
        private bool _configured;

        internal ClientAuth ClientAuth;

        #endregion

        #region Events

        private void OnClose(object sender, EventArgs e)
        {
            if (_updateClose)
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
            }
            Application.Current.Shutdown();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            IsPatching = true;

            ImporterTextBlock.SetTextBlockSafe("Starting...");

            ImportWindow.IsOpen = true;

            // Trick to get the UI to display
            await Task.Delay(2000);

            ImporterTextBlock.SetTextBlockSafe("Self Update Check..");
            if (await SelfUpdate()) return;

            ImporterTextBlock.SetTextBlockSafe("Check Client Profiles...");
            CheckClientProfiles();

            ImporterTextBlock.SetTextBlockSafe("Loading settings...");
            LauncherContext.Settings.Load();

            ImporterTextBlock.SetTextBlockSafe("Applying settings...");
            ConfigureLauncher();

            ImporterTextBlock.SetTextBlockSafe("Getting launcher version...");
            var mblVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Log.Info("Hyddwn Launcher Version {0}", mblVersion);
            LauncherVersion.SetTextBlockSafe(mblVersion);

            ImporterTextBlock.SetTextBlockSafe("Getting mabinogi version...");
            var mabiVers = Patcher.ReadVersion();
            MaxVersion = mabiVers;
            VersionUpDown.Value = mabiVers;
            Log.Info("Mabinogi Version {0}", mabiVers);
            ClientVersion.SetTextBlockSafe(mabiVers.ToString());

            MabiVersionComboBox.SelectedIndex =
                MabiVersion.Versions.IndexOf(
                    MabiVersion.Versions.FirstOrDefault(v => v.ToString() == LauncherContext.Settings.Locale) ??
                    MabiVersion.Versions.FirstOrDefault(v => v.Name.StartsWith("North Am")) ??
                    MabiVersion.Versions.First());


            ImporterTextBlock.Text = "Updating server profiles...";
            await Task.Run(() => ProfileManager.UpdateProfiles());

            ImportWindow.IsOpen = false;
            if (!LauncherContext.Settings.FirstRun)
                return;

            ChangesWindow.IsOpen = true;
            LauncherContext.Settings.FirstRun = false;

            _backgroundWorker2.DoWork += _backgroundWorker2_DoWork;

            if (ClientProfileComboBox.SelectedIndex == -1) return;

            _backgroundWorker2.RunWorkerAsync();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            _backgroundWorker2.RunWorkerAsync();
        }

        private void MainCancelButton_Click(object sender, RoutedEventArgs e)
        {
            ActionButton.Content = "Cancelling...";
            ActionButton.IsEnabled = false;
            TokenSource.Cancel();
        }

        private void Updater_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private async void MabiVersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
                try
                {
                    _patcher = new Patcher(this, ((MabiVersion) comboBox.SelectedItem).GetPatchInfo());
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, "Unable to fetch patch info updates, failing as mabi is offline.");

                    _patcher = new Patcher(this, OfficialPatchInfo.OfflinePatchInfo);
                }
            if (_patcher.PatchInfo["patch_accept"] == "1")
                return;
            await this.ShowMessageAsync("Notice", "Mabinogi is currently offline or unreachable!");
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ActiveClientProfile == null || ActiveServerProfile == null)
                    throw new ApplicationException("Unable to start client: no client or server profile available!");

                if (ActiveServerProfile.IsOfficial)
                {
                    await DeletePackFiles();
                    NxAuthLogin.IsOpen = true;

                    return;
                }

                var arguments =
                    $"code:1622 ver:{Patcher.ReadVersion()} logip:{_patcher.PatchInfo["login"]} logport:11000 {_patcher.PatchInfo["arg"]}";

                var ipAddress = ActiveServerProfile.LoginIp;
                IPAddress address;
                if (ipAddress != null && IPAddress.TryParse(ipAddress, out address))
                {
                    arguments = arguments.Replace(_patcher.PatchInfo["login"], address.ToString());
                    arguments = Regex.Replace(arguments, ChatIpAddressPattern,
                        $"chatip:{address}");

                    await BuildServerPackFile();
                }
                else
                {
                    Loading.IsOpen = false;

                    //TODO Replace all instances of ShowMessageAsync with ChildWindow Messages
                    await
                        this.ShowMessageAsync("Launch Failed",
                            "IP address is not valid. Please update you profile with a valid IP address.");

                    return;
                }


                Log.Info("Beginning client launch...");

                Log.Info("Starting client.exe with the following args: {0}", arguments);
                try
                {
                    Process.Start(ActiveClientProfile.Location, arguments);
                }
                catch (Exception ex)
                {
                    Log.Error("Cannot start Mabbinogi: {0}", ex.ToString());
                    throw new ApplicationException(ex.ToString());
                }
                Log.Info("Client start success");
                LauncherContext.Settings.Save();
                Application.Current.Shutdown();
            }
            catch (ApplicationException ex)
            {
                Loading.IsOpen = false;
                await this.ShowMessageAsync("Launch Failed", "Cannot start Mabinogi: " + ex.Message);
                Log.Exception(ex, "Client start finished with errors");
            }
        }

        private void VersionButtonClick(object sender, RoutedEventArgs e)
        {
            if (VersionUpDown.Value != null) Patcher.WriteVersion((int) VersionUpDown.Value);
            MaxVersion = Patcher.ReadVersion();
            ClientVersion.Text = MaxVersion.ToString();
        }

        private void ServerProfileComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveServerProfile = ((ComboBox) sender).SelectedItem as ServerProfile;
            ActiveServerProfile?.GetUpdates();
        }

        private void ClientProfileComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveClientProfile = ((ComboBox) sender).SelectedItem as ClientProfile;
            if (IsInitialized && IsLoaded)
                ConfigureLauncher();
        }

        private void ProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            var editor = new ProfileEditor(ProfileManager);
            editor.ShowDialog();
            ConfigureLauncher();
        }

        private void ReLangPackButtonOnClick(object sender, RoutedEventArgs e)
        {
            IsPatching = true;
            _patcher.RedownloadLanugagePack();
            IsPatching = false;
        }

        private void LogViewOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tbb = sender as TextBoxBase;
            tbb?.ScrollToEnd();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await DeletePackFiles();
        }

        private async void LaunchButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            await Dispatcher.Invoke(async () => await BuildServerPackFile());
        }

        private void This_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                LaunchButton.Click -= LaunchButton_Click;
                LaunchButton.Click += LaunchButtonOnClick;
                LaunchButton.Content = "Rebuild Pack";
            }

            OnKeyDown(e);
        }

        private void This_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                LaunchButton.Click -= LaunchButtonOnClick;
                LaunchButton.Click += LaunchButton_Click;
                LaunchButton.Content = "Launch";
            }

            OnKeyUp(e);
        }

        private async void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MainProgressReporter.SetProgressBar(0.0);
            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Visible);
            Application.Current.Dispatcher.Invoke(() => { IsPatching = true; });
            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Downloading Update...");
            try
            {
                UpdaterUpdate();
                MainProgressReporter.SetProgressBar(0.0);
                MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Visible);
                await AsyncDownloader.DownloadWithCallbackAsync(
                    _updateInfo["Link"],
                    _updateInfo["File"],
                    (d, s) =>
                    {
                        MainProgressReporter.SetProgressBar(d);
                        MainProgressReporter.LeftTextBlock.SetTextBlockSafe(s);
                    }
                );
                MainProgressReporter.RighTextBlock.SetTextBlockSafe("Download Successful. Launching Updater.");
                MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Hidden);
                Closing -= Updater_Closing;
                _updateClose = true;
                Dispatcher.Invoke(Close);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Error occured durring update");
                MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Failed to download update. Continuing...");
                MainProgressReporter.RighTextBlock.SetTextBlockSafe("");
                MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Hidden);
                IsPatching = false;
            }
        }

        private async void _backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ActiveClientProfile == null || !_configured) return;
                IsPatching = true;
                //Force NA
                MabiVersionComboBox.SelectedItem =
                    MabiVersion.Versions.FirstOrDefault(v => v.Name.StartsWith("North Am"));
            });
            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Visible);
            if (_patcher.PatchInfo.MainVersion > Patcher.ReadVersion())
                try
                {
                    await Task.Run(() => { _patcher.Patch(_patcher.FindSequence()); });
                }
                catch (PatchSequenceNotFoundException ex)
                {
                    await
                        this.ShowMessageAsync("Can't Fine Patch",
                            ex.Message);
                }
                catch (Exception ex)
                {
                    await Dispatcher.Invoke(async () =>
                    {
                        Log.Warning("Failed to patch to version: {0}", ex.ToString());
                        await
                            this.ShowMessageAsync("Update Failed",
                                "Failed to patch Mabinogi. (See the log for more details)");
                    });
                }

            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Hidden);
            Application.Current.Dispatcher.Invoke(() =>
            {
                Dispatcher.Invoke(() => MaxVersion = Patcher.ReadVersion());
                IsPatching = false;
            });
        }

        private async void NxAuthLoginOnSubmit(object sender, RoutedEventArgs e)
        {
            if (NxAuthLoginNotice.Visibility == Visibility.Visible)
            {
                NxAuthLoginNotice.Visibility = Visibility.Collapsed;
                NxAuthLoginNotice.Text = "";
            }

            ToggleLoginControls();

            var username = NxAuthLoginUsername.Text;
            var password = NxAuthLoginPassword.Password;

            NxAuthLoginPassword.Password = "";

            ClientAuth = new ClientAuth();

            ClientAuth.HashPassword(ref password);

            var passport = await ClientAuth.GetNxAuthHash(username, password);

            if (passport == ClientAuth.LoginFailed)
            {
                ToggleLoginControls();

                NxAuthLoginNotice.Text = "Username or Password is Incorrect";
                NxAuthLoginNotice.Visibility = Visibility.Visible;

                return;
            }

            if (passport == ClientAuth.DevError)
            {
                ToggleLoginControls();

                NxAuthLoginNotice.Text = "Error Retrieving Data";
                NxAuthLoginNotice.Visibility = Visibility.Visible;

                return;
            }

            NxAuthLogin.IsOpen = false;

            SpecialThanks.IsOpen = true;
            await Task.Delay(2000);
            SpecialThanks.IsOpen = false;

            Text.Text = "Launching...";
            Loading.IsOpen = true;
            await Task.Delay(500);

            var launchArgs =
                "code:1622 verstr:248 ver:248 locale:USA env:Regular setting:file://data/features.xml " +
                $"logip:{_patcher.PatchInfo["login"]} logport:11000 chatip:208.85.109.37 chatport:8002 /P:{passport} -bgloader";

            try
            {
                try
                {
                    Log.Info($"Starting client with the following parameters: {launchArgs}");
                    Process.Start(ActiveClientProfile.Location, launchArgs);
                }
                catch (Exception ex)
                {
                    Log.Error("Cannot start Mabbinogi: {0}", ex.ToString());
                    throw new IOException();
                }
                Log.Info("Client start success");
                LauncherContext.Settings.Save();
                Application.Current.Shutdown();
            }
            catch (IOException ex)
            {
                Loading.IsOpen = false;
                await this.ShowMessageAsync("Launch Failed", "Cannot start Mabinogi: " + ex.Message);
                Log.Exception(ex, "Client start finished with errors");
            }
        }

        private void NxAuthLoginOnCancel(object sender, RoutedEventArgs e)
        {
            NxAuthLogin.IsOpen = false;
        }

        #endregion

        #region Methods

        public void AddToLog(string text)
        {
            LogView.AppendText(text);
        }

        public void AddToLog(string format, params object[] args)
        {
            AddToLog(string.Format(format, args));
        }

        private void ToggleLoginControls()
        {
            NxAuthLoginUsername.IsEnabled = !NxAuthLoginUsername.IsEnabled;
            NxAuthLoginPassword.IsEnabled = !NxAuthLoginPassword.IsEnabled;
            NxAuthLoginSubmit.IsEnabled = !NxAuthLoginSubmit.IsEnabled;
            NxAuthLoginCancel.IsEnabled = !NxAuthLoginCancel.IsEnabled;

            NxAuthLoginLoadingIndicator.IsActive = !NxAuthLoginLoadingIndicator.IsActive;
        }

        private async void CheckClientProfiles()
        {
            _settingUpProfile = true;

            if (ProfileManager.ClientProfiles.Count == 0)
            {
                ProfileEditor pe;
                while (_settingUpProfile)
                {
                    pe = new ProfileEditor(ProfileManager, true);
                    pe.ShowDialog();
                    if (
                        string.IsNullOrWhiteSpace(
                            ((ClientProfile) pe.ClientProfileListBox.SelectedItem).Location))
                        while (
                            string.IsNullOrWhiteSpace(
                                ((ClientProfile) pe.ClientProfileListBox.SelectedItem).Location))
                        {
                            //TODO: REALLY NEED TO CHANGE THIS!!!
                            var output =
                                await
                                    this.ShowInputAsync("Profile Error",
                                        "Each profile must have a location. Without a location your Mabinogi instance cannot be managed. Please set your location (client.exe) now.");

                            ((ClientProfile) pe.ClientProfileListBox.SelectedItem).Location = output;
                            ProfileManager.SaveClientProfiles();
                        }

                    if (!File.Exists(((ClientProfile) pe.ClientProfileListBox.SelectedItem).Location))
                    {
                        var fileOkay = false;

                        while (!fileOkay)
                        {
                            var output =
                                await
                                    this.ShowInputAsync("Profile Error",
                                        "The file could not be found at this location, please specify the correct location.");

                            if (!File.Exists(output))
                                continue;

                            ((ClientProfile) pe.ClientProfileListBox.SelectedItem).Location = output;
                            ProfileManager.SaveClientProfiles();

                            fileOkay = true;
                        }
                    }

                    _settingUpProfile = false;

                    LauncherContext.Settings.ClientProfileSelectedIndex =
                        pe.ClientProfileListBox.SelectedIndex;
                }
            }

            _settingUpProfile = false;
        }

        private async void ConfigureLauncher()
        {
            _configured = false;

            if (_settingUpProfile || ActiveClientProfile == null) return;

            var newWorkingDir = Path.GetDirectoryName(ActiveClientProfile.Location);

            try
            {
                Environment.CurrentDirectory =
                    newWorkingDir ?? throw new Exception("Error in \"Path to Client\" specification.");

                Log.Info("Setting Current Working Direcotry to {0}", newWorkingDir);

                _configured = true;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Error Configuring Launcher");
                await
                    this.ShowMessageAsync("Configuration Error",
                        "Failed to configure launcher possibly due to erroneous setting in your client profile. Please edit your profile.");
                return;
            }

            var testpath = Path.GetDirectoryName(ActiveClientProfile.Location) + "\\eTracer.exe";

            if (LauncherContext.Settings.WarnIfRootIsNotMabiRoot &&
                !File.Exists(testpath))
            {
                Log.Warning("The path set for this profile does not appear to be the root folder for Mabinogi.");
                MessageBox.Show(
                    "The path set for this profile does not appear to be the root folder for Mabinogi.\r\n\r\nThis will most likely result in improper operations.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }


            if (IsInitialized)
            {
                var mabiVers = Patcher.ReadVersion();
                MaxVersion = mabiVers;
                VersionUpDown.Value = mabiVers;
                Log.Info("Mabinogi Version {0}", mabiVers);
                ClientVersion.SetTextBlockSafe(mabiVers.ToString());

                //while (_backgroundWorker2.IsBusy)
                //{
                //    await Task.Delay(200);
                //}

                //_backgroundWorker2.RunWorkerAsync();
            }
        }

        private async Task DeletePackFiles()
        {
            Text.Text = "Cleaning generated pack files...";
            Loading.IsOpen = true;
            await Task.Delay(300);
            await Task.Run(() =>
            {
                var files = Directory.GetFiles(Path.GetDirectoryName(ActiveClientProfile.Location) + "\\package")
                    .Where(f => f.StartsWith("hl_"));
                foreach (var file in files)
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex, $"Failed to delete '{file}'!");
                    }
            });
            Loading.IsOpen = false;
        }

        private async Task BuildServerPackFile()
        {
            Text.Text = "Building server pack file...";
            Loading.IsOpen = true;
            await Task.Delay(300);

            var maxVersion = 0;

            Application.Current.Dispatcher.Invoke(() => { maxVersion = MaxVersion; });

            if (LauncherContext.Settings.UsePackFiles &&
                ActiveServerProfile.PackVersion == maxVersion)
            {
                var packEngine = new PackEngine();
                packEngine.BuildServerPack(ActiveServerProfile);
            }

            if (ActiveServerProfile.PackVersion != MaxVersion)
                await DeletePackFiles();

            Loading.IsOpen = false;
        }

        private async void UpdaterUpdate()
        {
            if (File.Exists("Updater.exe")) return;
            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Downloading App Updater...");
            MainProgressReporter.SetProgressBar(0);
            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Visible);
            await
                AsyncDownloader.DownloadWithCallbackAsync("http://www.imabrokedude.com/Updater.zip", "Updater.zip",
                    (d, s) =>
                    {
                        MainProgressReporter.SetProgressBar(d);
                        MainProgressReporter.LeftTextBlock.SetTextBlockSafe(s);
                    });
            MainProgressReporter.SetProgressBar(0);
            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Visible);
            MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Extracting Updater...");
            MainProgressReporter.LeftTextBlock.SetTextBlockSafe("");
            using (var zipFile = ZipFile.Read(Path.GetFullPath("Updater.zip")))
            {
                zipFile.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                zipFile.ExtractAll(Path.GetDirectoryName(Path.GetFullPath("Updater.zip")));
            }
            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Cleaning up...");
            File.Delete("Updater.zip");
            MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(false);
            MainProgressReporter.RighTextBlock.SetTextBlockSafe("");
        }

        private async Task<bool> SelfUpdate()
        {
            Closing += Updater_Closing;
            ImporterTextBlock.SetTextBlockSafe("Self Update Check...");
            if (await CheckForUpdates())
            {
                IsPatching = false;
                ImportWindow.IsOpen = false;
                _backgroundWorker.DoWork += _backgroundWorker_DoWork;
                _backgroundWorker.RunWorkerAsync();
                return true;
            }
            IsPatching = false;
            Closing -= Updater_Closing;
            return false;
        }

        private async Task<bool> CheckForUpdates()
        {
            try
            {
                await Task.Delay(100);
                var webClient = new WebClient();
                var current = Assembly.GetExecutingAssembly().GetName().Version;
                using (
                    var fileReader =
                        new FileReader(
                            webClient.OpenRead(
                                "http://launcher.hyddwnproject.com/version")))
                {
                    foreach (var str in fileReader)
                    {
                        int length;
                        if ((length = str.IndexOf(':')) >= 0)
                            _updateInfo[str.Substring(0, length).Trim()] = str.Substring(length + 1).Trim();
                    }
                }
                _updateInfo["Current"] = current.ToString();
                var version2 = new Version(_updateInfo["Version"]);
                var updateAvailable = current < version2;

                return updateAvailable;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Unable to check for updates.");
                IsPatching = false;
                return false;
            }
        }

        public FileCopier CreateCopierInstance(string source, string destination)
        {
            FileCopier copier = null;
            Dispatcher.Invoke(() => { copier = new FileCopier(source, destination, TokenSource.Token); });
            return copier;
        }

        public FileDownloader CreateDownloaderInstance(string remote, string local, int size)
        {
            FileDownloader downloader = null;
            Dispatcher.Invoke(() => { downloader = new FileDownloader(remote, local, size, TokenSource.Token); });
            return downloader;
        }

        #endregion
    }
}