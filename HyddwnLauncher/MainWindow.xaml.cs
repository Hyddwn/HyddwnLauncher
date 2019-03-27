using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using HyddwnLauncher.Core;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Model;
using HyddwnLauncher.Network;
using HyddwnLauncher.Util;
using Ionic.Zip;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace HyddwnLauncher
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region DependancyProperties

        public static readonly DependencyProperty IsUpdateAvailableProperty = DependencyProperty.Register(
            "IsUpdateAvailable", typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));

        #endregion

        #region ctor

        public MainWindow(LauncherContext launcherContext)
        {
            Instance = this;
#if DEBUG
           launcherContext.LauncherSettingsManager.Reset();
#endif
            LauncherContext = launcherContext;
            Settings = launcherContext.LauncherSettingsManager;
            ProfileManager = new ProfileManager();
            ProfileManager.Load();
            //Populate for the first time
            if (ProfileManager.ServerProfiles.Count == 0)
            {
                ProfileManager.ServerProfiles.Insert(0, ServerProfile.OfficialProfile);
                ProfileManager.SaveServerProfiles();
            }

            ChangeAppTheme();

            AccentColors = ThemeManager.Accents
                .Select(a =>
                    new AccentColorMenuData
                    {
                        Name = a.Name,
                        ColorBrush = a.Resources["AccentColorBrush"] as Brush
                    }).ToList();
            AppThemes = ThemeManager.AppThemes
                .Select(a =>
                    new AppThemeMenuData
                    {
                        Name = a.Name,
                        BorderColorBrush = a.Resources["BlackColorBrush"] as Brush,
                        ColorBrush = a.Resources["WhiteColorBrush"] as Brush
                    }).ToList();

            InitializeComponent();
            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Hidden);
            _disableWhilePatching = new Control[]
            {
                LaunchButton,
                NewProfileButton,
                ClientProfileComboBox,
                ServerProfileComboBox
            };

            _updateClose = false;
        }

        #endregion

        #region Properties

        public string Theme { get; set; }
        public string Accent { get; set; }
        private PluginHost PluginHost { get; set; }
        public ProfileManager ProfileManager { get; private set; }
        public ServerProfile ActiveServerProfile { get; set; }
        public ClientProfile ActiveClientProfile { get; set; }

        public LauncherContext LauncherContext { get; private set; }

        // Very bad, will need to adjust the method of access.
        public static MainWindow Instance { get; private set; }
        public LauncherSettingsManager Settings { get; private set; }
        public List<AccentColorMenuData> AccentColors { get; set; }
        public List<AppThemeMenuData> AppThemes { get; set; }
        public GetAccessTokenResponse LastResponseObject { get; set; }
        public bool UsingCredentials { get; set; }

        public bool IsUpdateAvailable
        {
            get => (bool) GetValue(IsUpdateAvailableProperty);
            set => SetValue(IsUpdateAvailableProperty, value);
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

                    if (!value)
                    {
                        PluginHost?.PatchEnd();
                        ClientVersion.SetTextBlockSafe(ReadVersion().ToString());
                    }
                    else
                    {
                        PluginHost?.PatchBegin();
                    }
                });
            }
        }

        #endregion

        #region Fields

        private static readonly string AssemblyLocation = Assembly.GetExecutingAssembly().Location;
        private static readonly string Assemblypath = Path.GetDirectoryName(AssemblyLocation);
        private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
        private readonly BackgroundWorker _backgroundWorker2 = new BackgroundWorker();
        private readonly Control[] _disableWhilePatching;
        private readonly Dictionary<string, string> _updateInfo = new Dictionary<string, string>();
        private volatile bool _patching;
        private bool _updateClose;
        private bool _settingUpProfile;
        private Dictionary<string, MetroTabItem> _pluginTabs;

        public event Action LoginSuccess;
        public event Action LoginCancel;
        public event Action DeviceTrustSuccess;
        public event Action DeviceTrustCancel;

        #endregion

        #region Events

        private void OnClose(object sender, EventArgs e)
        {
            if (_updateClose)
            {
                var processInfo = new ProcessStartInfo
                {
                    Arguments =
                        $"{"\"" + _updateInfo["File"] + "\""} {_updateInfo["SHA256"]} {"\"" + _updateInfo["Execute"] + "\""}",
                    FileName = "Updater.exe"
                };

                // Process.Start is different that var process = new Process(processinfo).Start();
                // It launches the process as it's own process instead of a child process.
                Process.Start(processInfo);
            }

            PluginHost.ShutdownPlugins();

            Application.Current.Shutdown();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            IsPatching = true;

            MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Visible);
            MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Starting...");

            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Update Check...");
            IsUpdateAvailable = await CheckForUpdates();

            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Check Client Profiles...");
            _settingUpProfile = true;
            CheckClientProfiles();

            while (_settingUpProfile)
                await Task.Delay(250);

            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Applying settings...");
            ConfigureLauncher();

            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Getting launcher version...");
            var mblVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Log.Info("Hyddwn Launcher Version {0}", mblVersion);
            LauncherVersion.SetTextBlockSafe(mblVersion);

            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Getting mabinogi version...");
            var mabiVers = ReadVersion();
            Log.Info("Mabinogi Version {0}", mabiVers);
            ClientVersion.SetTextBlockSafe(mabiVers.ToString());

            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Updating server profiles...");
            await Task.Run(() => ProfileManager.UpdateProfiles());

            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Initializing Plugins...");
            await InitializePlugins();

            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Hidden);
            MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(false);
            MainProgressReporter.LeftTextBlock.SetTextBlockSafe("");
            MainProgressReporter.RighTextBlock.SetTextBlockSafe("");

            IsPatching = false;

            if (Settings.ConfigurationDirty)
                await this.ShowMessageAsync("Configuration Error",
                    "An error occured when loading the configuration file that required it to be reset.");
        }

        private void Updater_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeAppTheme();
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            IsPatching = true;

            PluginHost.PreLaunch();

            try
            {
                if (ActiveClientProfile == null || ActiveServerProfile == null)
                {
                    IsPatching = false;
                    throw new ApplicationException("Unable to start client: no client or server profile available!");
                }

                if (ActiveServerProfile.IsOfficial)
                {
                    await DeletePackFiles();

                    if (!NexonApi.Instance.IsAccessTokenValid(ActiveClientProfile.Guid))
                    {
                        var credentials =
                            CredentialsStorage.Instance.GetCredentialsForProfile(ActiveClientProfile.Guid);

                        if (credentials != null)
                        {
                            var success = await NexonApi.Instance.GetAccessToken(credentials.Username,
                                credentials.Password, ActiveClientProfile.Guid);

                            LastResponseObject = success;

                            if (success.Success)
                            {
                                LoginSuccess += LaunchOfficial;
                                LaunchOfficial();
                                return;
                            }

                            IsPatching = false;

                            LoginSuccess += LaunchOfficial;

                            if (success.Code == NexonApi.TrustedDeviceRequired)
                            {
                                NxDeviceTrust.IsOpen = true;
                                UsingCredentials = true;
                                return;
                            }

                            NxAuthLogin.IsOpen = true;

                            NxAuthLoginNotice.Text = success.Message;
                        }

                        IsPatching = false;
                        LoginSuccess += LaunchOfficial;
                        NxAuthLogin.IsOpen = true;

                        return;
                    }

                    LaunchOfficial();
                    return;
                }

                LaunchCustom();
            }
            catch (ApplicationException ex)
            {
                Loading.IsOpen = false;
                await this.ShowMessageAsync("Launch Failed", "Cannot start Mabinogi: " + ex.Message);
                Log.Exception(ex, "Client start finished with errors");
            }
        }

        private void ServerProfileComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveServerProfile = ((ComboBox) sender).SelectedItem as ServerProfile;
            ActiveServerProfile?.GetUpdates();
            if (!IsInitialized || !IsLoaded) return;
            PluginHost.ServerProfileChanged(ActiveServerProfile);
        }

        private void ClientProfileComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveClientProfile = ((ComboBox) sender).SelectedItem as ClientProfile;
            if (!IsInitialized || !IsLoaded) return;
            ConfigureLauncher();
            PluginHost.ClientProfileChanged(ActiveClientProfile);
        }

        private void ProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileEditor.IsOpen = true;
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

        //private void This_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        //    {
        //        LaunchButton.Click -= LaunchButton_Click;
        //        LaunchButton.Click += LaunchButtonOnClick;
        //        LaunchButton.Content = "Rebuild Pack";
        //    }

        //    OnKeyDown(e);
        //}

        //private void This_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        //    {
        //        LaunchButton.Click -= LaunchButtonOnClick;
        //        LaunchButton.Click += LaunchButton_Click;
        //        LaunchButton.Content = "Launch";
        //    }

        //    OnKeyUp(e);
        //}

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
                await AsyncDownloader.DownloadFileWithCallbackAsync(
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

        private async void NxAuthLoginOnSubmit(object sender, RoutedEventArgs e)
        {
            // Requested behavior
            if (NxAuthLoginUsername.IsFocused)
            {
                NxAuthLoginPassword.Focus();
                return;
            }

            if (NxAuthLoginNotice.Visibility == Visibility.Visible)
            {
                NxAuthLoginNotice.Visibility = Visibility.Collapsed;
                NxAuthLoginNotice.Text = "";
            }

            ToggleLoginControls();

            var username = NxAuthLoginUsername.Text;
            var password = NxAuthLoginPassword.Password;

            if (string.IsNullOrWhiteSpace(username))
            {
                ToggleLoginControls();

                NxAuthLoginNotice.Text = "Please enter a Username";
                NxAuthLoginNotice.Visibility = Visibility.Visible;

                NxAuthLoginUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ToggleLoginControls();

                NxAuthLoginNotice.Text = "Please enter a Password";
                NxAuthLoginNotice.Visibility = Visibility.Visible;

                NxAuthLoginPassword.Focus();
                return;
            }

            NxAuthLoginPassword.Password = "";

            NexonApi.Instance.HashPassword(ref password);

            // Store username and Hash
            if (RememberMeCheckBox.IsChecked != null && (bool) RememberMeCheckBox.IsChecked)
                CredentialsStorage.Instance.Add(ActiveClientProfile.Guid, username, password);

            var success = await NexonApi.Instance.GetAccessToken(username, password, ActiveClientProfile.Guid);

            if (!success.Success)
            {
                // TODO: Release+: Add proper support for detection of response codes
                if (success.Code != NexonApi.TrustedDeviceRequired)
                {
                    ToggleLoginControls();

                    NxAuthLoginNotice.Text = "Username or Password is Incorrect";
                    NxAuthLoginNotice.Visibility = Visibility.Visible;

                    NxAuthLoginPassword.Focus();
                    return;
                }

                NxAuthLogin.IsOpen = false;

                NxAuthLoginPassword.Password = password;
                NxAuthLoginPassword.IsEnabled = false;

                NxDeviceTrust.IsOpen = true;

                return;
            }

            NxAuthLogin.IsOpen = false;

            RememberMeCheckBox.IsChecked = false;

            ToggleLoginControls();

            NxAuthLoginNotice.Visibility = Visibility.Collapsed;
            NxAuthLoginNotice.Text = "";

            OnLoginSuccess();
        }

        private void NxAuthLoginOnCancel(object sender, RoutedEventArgs e)
        {
            NxAuthLogin.IsOpen = false;
            OnLoginCancel();
        }

        private async void NxDeviceTrustOnContinue(object sender, RoutedEventArgs e)
        {
            if (NxDeviceTrustNotice.Visibility == Visibility.Visible)
            {
                NxDeviceTrustNotice.Visibility = Visibility.Collapsed;
                NxDeviceTrustNotice.Text = "";
            }

            ToggleDeviceControls();

            if (string.IsNullOrWhiteSpace(NxDeviceTrustVerificationCode.Text))
            {
                ToggleDeviceControls();

                NxDeviceTrustNotice.Text = "Verification code is empty!";
                NxDeviceTrustNotice.Visibility = Visibility.Visible;

                NxDeviceTrustVerificationCode.Focus();
                return;
            }

            var credentials = CredentialsStorage.Instance.GetCredentialsForProfile(ActiveClientProfile.Guid);

            var username = UsingCredentials ? credentials.Username : NxAuthLoginUsername.Text;
            var verification = NxDeviceTrustVerificationCode.Text;
            var saveDevice = NxDeviceTrustRememberMe.IsChecked != null && (bool) NxDeviceTrustRememberMe.IsChecked;

            var success =
                await NexonApi.Instance.PutVerifyDevice(username, verification, NexonApi.GetDeviceUuid(), saveDevice);

            if (!success)
            {
                ToggleDeviceControls();

                NxDeviceTrustNotice.Text = "Verification code error!";
                NxDeviceTrustNotice.Visibility = Visibility.Visible;

                NxDeviceTrustVerificationCode.Focus();
                return;
            }

            var loginSuccess = new GetAccessTokenResponse();

            if (UsingCredentials)
            {
                if (credentials != null)
                    loginSuccess = await NexonApi.Instance.GetAccessToken(credentials.Username, credentials.Password,
                        ActiveClientProfile.Guid);
            }
            else
            {
                loginSuccess = await NexonApi.Instance.GetAccessToken(username, NxAuthLoginPassword.Password,
                    ActiveClientProfile.Guid);
            }

            if (!loginSuccess.Success)
            {
                NxDeviceTrust.IsOpen = false;

                NxAuthLoginNotice.Text = "Username or Password is Incorrect";
                NxAuthLoginNotice.Visibility = Visibility.Visible;

                NxAuthLoginPassword.Focus();

                NxAuthLogin.IsOpen = true;
                return;
            }

            NxAuthLoginNotice.Visibility = Visibility.Collapsed;
            NxAuthLoginNotice.Text = "";

            NxAuthLoginPassword.Password = "";
            NxAuthLoginPassword.IsEnabled = true;

            NxDeviceTrust.IsOpen = false;

            OnDeviceTrustSuccess();
            OnLoginSuccess();
        }

        private void NxDeviceTrustOnCancel(object sender, RoutedEventArgs e)
        {
            NxDeviceTrust.IsOpen = false;

            OnDeviceTrustCancel();
            OnLoginCancel();

            NxAuthLoginPassword.Password = "";
            NxAuthLoginPassword.IsEnabled = true;
        }

        private async void ProfileEditorIsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (ProfileEditor.IsOpen && ProfileManager.ClientProfiles.Count == 0 && _settingUpProfile)
            {
                await this.ShowMessageAsync("No Client Profile",
                    "You have been taken to this window because you do not have a profile configured for Mabinogi. " +
                    "Please configure a profile representing where your Client.exe is to use this launcher.");

                AddClientProfile();
                return;
            }

            if (ProfileEditor.IsOpen || !_settingUpProfile) return;

            var selectedProfileLocation = ((ClientProfile) ClientProfileListBox.SelectedItem).Location;
            if (string.IsNullOrWhiteSpace(selectedProfileLocation) || !File.Exists(selectedProfileLocation))
                await this.ShowMessageAsync("Valid File or Path",
                    "The path you have entered is invalid.");

            _settingUpProfile = false;
        }

        private void ProfileEditorOnClientFrofileListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClientProfileUserControl.CredentialUsername = "";

            if (ClientProfileListBox.SelectedItem is ClientProfile clientProfile)
            {
                var creds = CredentialsStorage.Instance.GetCredentialsForProfile(clientProfile.Guid);

                if (creds != null)
                    ClientProfileUserControl.CredentialUsername = creds.Username;
            }

            ProfileManager.SaveClientProfiles();
        }

        private void ProfileEditorOnServerFrofileListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var serverProfile = ServerProfileListBox.SelectedItem as ServerProfile;
            serverProfile?.GetUpdates();
            ProfileManager.SaveServerProfiles();
        }

        private void ProfileEditorOnAddClientProfileButtonCLick(object sender, RoutedEventArgs e)
        {
            AddClientProfile();
        }

        private void ProfileEditorOnRemoveClientProfileButtonCLick(object sender, RoutedEventArgs e)
        {
            if (ClientProfileListBox.SelectedIndex == -1) return;
            ProfileManager.ClientProfiles.RemoveAt(ClientProfileListBox.SelectedIndex);
        }

        private void ProfileEditorOnAddServerProfileButtonCLick(object sender, RoutedEventArgs e)
        {
            AddServerProfile();
        }

        private void ProfileEditorOnRemoveServerProfileButtonCLick(object sender, RoutedEventArgs e)
        {
            if (ServerProfileListBox.SelectedIndex == -1) return;
            ProfileManager.ServerProfiles.RemoveAt(ServerProfileListBox.SelectedIndex);
        }

        private void ProfileEditorOnProfileCLosingFinished(object sender, RoutedEventArgs e)
        {
            ConfigureLauncher();
            ProfileManager.SaveClientProfiles();
            ProfileManager.SaveServerProfiles();
        }

        private void ResetOptionsResetBottonOnClick(object sender, RoutedEventArgs e)
        {
            if (ResetCredentialsCheckBox.IsChecked != null && (bool) ResetCredentialsCheckBox.IsChecked)
                CredentialsStorage.Instance.Reset();

            if (ResetClietProfilesCheckBox.IsChecked != null && (bool) ResetClietProfilesCheckBox.IsChecked)
                ProfileManager.ResetClientProfiles();

            if (ResetServerProfilesCheckBox.IsChecked != null && (bool) ResetServerProfilesCheckBox.IsChecked)
                ProfileManager.ResetServerProfiles();

            if (ResetLauncherConfigurationCheckBox.IsChecked != null &&
                (bool) ResetLauncherConfigurationCheckBox.IsChecked)
                Settings.Reset();

            ResetCredentialsCheckBox.IsChecked = ResetClietProfilesCheckBox.IsChecked =
                ResetServerProfilesCheckBox.IsChecked = ResetLauncherConfigurationCheckBox.IsChecked = false;
        }

        private async void OpenLogFileOnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(LauncherContext.LogFileLocation);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                await this.ShowMessageAsync("Error opening file.",
                    $"The following error occured when trying to open the log file:\r\n\r\n{ex.Message}");
            }
        }

        #endregion

        #region Methods

        private void AddClientProfile()
        {
            var clientProfile = new ClientProfile {Name = "New Profile", Guid = Guid.NewGuid().ToString()};
            ProfileManager.ClientProfiles.Add(clientProfile);
            ClientProfileListBox.SelectedItem = clientProfile;
        }

        private async void AddServerProfile()
        {
            var serverProfile = ServerProfile.Create();

            var profileUpdateUrl = await this.ShowInputAsync("Server Profile Url",
                                       "Please enter the url where your server profile can be located.") ?? "";

            serverProfile.ProfileUpdateUrl = profileUpdateUrl;
            serverProfile.GetUpdates();

            ProfileManager.ServerProfiles.Add(serverProfile);
            ServerProfileListBox.SelectedItem = serverProfile;
        }

        public void AddToLog(string text)
        {
            LogView?.AppendText(text);
        }

        public void AddToLog(string format, params object[] args)
        {
            AddToLog(string.Format(format, args));
        }

        private async Task BuildServerPackFile()
        {
            if (ActiveServerProfile.IsOfficial) return;

            MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Visible);
            MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Building server pack file...");

            var maxVersion = 0;

            Application.Current.Dispatcher.Invoke(() => { maxVersion = ReadVersion(); });

            if (Settings.LauncherSettings.UsePackFiles)
            {
                if (ActiveServerProfile.PackVersion != maxVersion)
                    await DeletePackFiles();

                if (ActiveServerProfile.PackVersion == maxVersion)
                {
                    MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Building server pack file...");
                    var packEngine = new PackEngine();
                    packEngine.BuildServerPack(ActiveServerProfile, ReadVersion());
                }
            }

            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Hidden);
            MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(false);
            MainProgressReporter.LeftTextBlock.SetTextBlockSafe("");
        }

        private void ChangeAppTheme()
        {
            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent(Settings.LauncherSettings.Accent),
                ThemeManager.GetAppTheme(Settings.LauncherSettings.Theme));
        }

        private void CheckClientProfiles()
        {
            if (ProfileManager.ClientProfiles.Count == 0)
            {
                ProfileEditor.IsOpen = true;
                return;
            }

            foreach (var clientProfile in ProfileManager.ClientProfiles.Where(p => string.IsNullOrWhiteSpace(p.Guid)))
                clientProfile.Guid = Guid.NewGuid().ToString();

            _settingUpProfile = false;
        }


        private async Task<bool> CheckForUpdates()
        {
            try
            {
                var webClient = new WebClient();
                var current = Assembly.GetExecutingAssembly().GetName().Version;
                using (
                    var fileReader =
                        new FileReader(await
                            webClient.OpenReadTaskAsync(
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

        private async void ConfigureLauncher()
        {
            if (_settingUpProfile || ActiveClientProfile == null) return;

            var newWorkingDir = Path.GetDirectoryName(ActiveClientProfile.Location);

            try
            {
                Environment.CurrentDirectory =
                    newWorkingDir ?? throw new Exception("Error in \"Path to Client\" specification.");

                Log.Info("Setting Current Working Direcotry to {0}", newWorkingDir);
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

            if (Settings.LauncherSettings.WarnIfRootIsNotMabiRoot &&
                !File.Exists(testpath))
            {
                Log.Warning("The path set for this profile does not appear to be the root folder for Mabinogi.");
                MessageBox.Show(
                    "The path set for this profile does not appear to be the root folder for Mabinogi.\r\n\r\nThis will most likely result in improper operations.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            if (!IsInitialized) return;
            var mabiVers = ReadVersion();
            Log.Info("Mabinogi Version {0}", mabiVers);
            ClientVersion.SetTextBlockSafe(mabiVers.ToString());
        }


        private async Task DeletePackFiles()
        {
            MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Cleaning generated pack files...");
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
        }

        private bool DependencyObjectIsValid(DependencyObject node)
        {
            if (node == null)
                return LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>().All(DependencyObjectIsValid);
            if (!Validation.GetHasError(node))
                return LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>().All(DependencyObjectIsValid);
            var element = node as IInputElement;
            if (element != null)
                Keyboard.Focus(element);
            return false;
        }

        private async Task InitializePlugins()
        {
            PluginHost = new PluginHost();
            _pluginTabs = new Dictionary<string, MetroTabItem>();

            if (PluginHost.Plugins == null) return;

            foreach (var plugin in PluginHost.Plugins)
                await Task.Run(() =>
                {
                    try
                    {
                        var pluginContext = new PluginContext();
                        pluginContext.MainUpdaterInternal += PluginMainUpdator;
                        pluginContext.SetPatcherStateInternal +=
                            isPatching => Dispatcher.Invoke(() => IsPatching = isPatching);
                        pluginContext.LogExceptionInternal += async (exception, b) =>
                        {
                            Log.Exception(exception);
                            if (b)
                                await Dispatcher.Invoke(async () =>
                                    await this.ShowMessageAsync("Error", exception.Message));
                        };
                        pluginContext.LogStringInternal += async (s, b) =>
                        {
                            Log.Info(s);
                            if (b)
                                await Dispatcher.Invoke(async () => await this.ShowMessageAsync("Info", s));
                        };
                        pluginContext.GetNexonApiInternal += () => NexonApi.Instance;
                        pluginContext.CreatePackEngineInternal += () => new PackEngine();
                        pluginContext.RequestUserLoginInternal += async (successAction, cancelAction) =>
                        {
                            var credentials =
                                CredentialsStorage.Instance.GetCredentialsForProfile(ActiveClientProfile.Guid);

                            if (credentials != null)
                            {
                                var success = await NexonApi.Instance.GetAccessToken(credentials.Username,
                                    credentials.Password,
                                    ActiveClientProfile.Guid);

                                if (success.Success)
                                {
                                    successAction.Raise();
                                    return;
                                }

                                LoginSuccess += successAction;
                                LoginCancel += cancelAction;

                                if (success.Code == NexonApi.TrustedDeviceRequired)
                                {
                                    NxDeviceTrust.IsOpen = true;
                                    UsingCredentials = true;
                                    return;
                                }
                            }

                            LoginSuccess += successAction;
                            LoginCancel += cancelAction;
                            NxAuthLogin.IsOpen = true;
                        };
                        pluginContext.GetPatcherStateInternal += () => IsPatching;
                        pluginContext.SetActiveTabInternal += guid =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (_pluginTabs.TryGetValue($"{guid}", out var tab))
                                    MainTabControl.SelectedItem = tab;
                            });
                        };
                        pluginContext.ShowDialogInternal += (title, message) =>
                        {
                            var returnResult = false;

                            Dispatcher.Invoke(async () =>
                            {
                                var result = await this.ShowMessageAsync(title, message,
                                    MessageDialogStyle.AffirmativeAndNegative);

                                returnResult = result == MessageDialogResult.Affirmative;
                            });

                            return returnResult;
                        };
                        pluginContext.CreateSettingsManagerInternal += (configPath, settingsSuffix) =>
                            new SettingsManager(configPath, settingsSuffix);

                        Dispatcher.Invoke(() =>
                        {
                            plugin.Initialize(pluginContext, ActiveClientProfile, ActiveServerProfile);

                            var pluginUi = plugin.GetPluginUi();

                            if (pluginUi == null) return;

                            var pluginTabItem = new MetroTabItem
                            {
                                Header = plugin.Name,
                                Content = pluginUi
                            };

                            _pluginTabs.Add($"{plugin.GetGuid()}", pluginTabItem);

                            MainTabControl.Items.Add(pluginTabItem);
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex, $"Error occured when loading plugin: {plugin.Name}");
                        MessageBox.Show($"Error loading {plugin.Name}, {ex.GetType().Name}: {ex.Message}", "Error");
                    }
                });

            PluginHost.ClientProfileChanged(ActiveClientProfile);
            PluginHost.ServerProfileChanged(ActiveServerProfile);
        }

        private async void LaunchCustom()
        {
            var arguments =
                $"code:1622 verstr:{ReadVersion()} ver:{ReadVersion()} logip:{ActiveServerProfile.LoginIp} logport:{ActiveServerProfile.LoginPort} chatip:{ActiveServerProfile.ChatIp} chatport:{ActiveServerProfile.ChatPort} locale:USA env:Regular setting:file://data/features.xml";

            Log.Info("Beginning client launch...");

            Log.Info("Starting client.exe with the following args: {0}", arguments);
            try
            {
                Process.Start(ActiveClientProfile.Location, arguments);
                PluginHost.PostLaunch();
            }
            catch (Exception ex)
            {
                IsPatching = false;
                Log.Error("Cannot start Mabbinogi: {0}", ex.ToString());
                throw new ApplicationException(ex.ToString());
            }

            Log.Info("Client start success");
            Settings.SaveLauncherSettings();

            PluginHost.ShutdownPlugins();

            Application.Current.Shutdown();
        }

        private async void LaunchOfficial()
        {
            IsPatching = true;

            MainProgressReporter.LeftTextBlock.SetTextBlockSafe("Launching...");
            MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(true);
            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Visible);

            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Getting passport...");
            var passport = await NexonApi.Instance.GetNxAuthHash();

            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Starting client...");
            var launchArgs =
                "code:1622 verstr:248 ver:248 locale:USA env:Regular setting:file://data/features.xml " +
                $"logip:208.85.109.35 logport:11000 chatip:208.85.109.37 chatport:8002 /P:{passport} -bgloader";

            try
            {
                try
                {
                    Log.Info($"Starting client with the following parameters: {launchArgs}");
                    Process.Start(ActiveClientProfile.Location, launchArgs);
                    PluginHost.PostLaunch();
                }
                catch (Exception ex)
                {
                    Log.Error("Cannot start Mabbinogi: {0}", ex.ToString());

                    throw new IOException();
                }

                Log.Info("Client start success");
                Settings.SaveLauncherSettings();
                PluginHost.ShutdownPlugins();
                Application.Current.Shutdown();
            }
            catch (IOException ex)
            {
                MainProgressReporter.LeftTextBlock.SetTextBlockSafe("");
                MainProgressReporter.RighTextBlock.SetTextBlockSafe("");
                MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(false);
                MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Hidden);
                IsPatching = false;
                await this.ShowMessageAsync("Launch Failed", "Cannot start Mabinogi: " + ex.Message);
                Log.Exception(ex, "Client start finished with errors");
            }
        }

        public void OnLoginCancel()
        {
            LoginCancel?.Raise();
            if (LoginCancel != null)
                foreach (var d in LoginCancel.GetInvocationList())
                    LoginCancel -= d as Action;

            if (LoginSuccess == null) return;
            foreach (var d in LoginSuccess.GetInvocationList()) LoginCancel -= d as Action;
        }

        public void OnLoginSuccess()
        {
            LoginSuccess?.Raise();
            if (LoginSuccess == null) return;
            foreach (var d in LoginSuccess.GetInvocationList()) LoginSuccess -= d as Action;
        }

        public void OnDeviceTrustSuccess()
        {
            DeviceTrustSuccess?.Raise();
            if (DeviceTrustSuccess == null) return;
            foreach (var d in DeviceTrustSuccess.GetInvocationList()) DeviceTrustSuccess -= d as Action;
        }

        public void OnDeviceTrustCancel()
        {
            DeviceTrustCancel?.Raise();
            if (DeviceTrustCancel == null) return;
            foreach (var d in DeviceTrustCancel.GetInvocationList()) DeviceTrustCancel -= d as Action;
        }

        private void PluginMainUpdator(string leftText, string rightText, double value, bool isIntermediate,
            bool isVisible)
        {
            Dispatcher.Invoke(() =>
            {
                MainProgressReporter.LeftTextBlock.SetTextBlockSafe(leftText);
                MainProgressReporter.RighTextBlock.SetTextBlockSafe(rightText);
                MainProgressReporter.ReporterProgressBar.SetMetroProgressSafe(value);
                MainProgressReporter.ReporterProgressBar.SetMetroProgressIndeterminateSafe(isIntermediate);
                MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(isVisible
                    ? Visibility.Visible
                    : Visibility.Hidden);
            });
        }

        public int ReadVersion()
        {
            try
            {
                return File.Exists("version.dat") ? BitConverter.ToInt32(File.ReadAllBytes("version.dat"), 0) : 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<bool> SelfUpdate()
        {
            IsPatching = true;
            Closing += Updater_Closing;
            ImporterTextBlock.SetTextBlockSafe("Self Update Check...");
            ImportWindow.IsOpen = true;
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

        private void ToggleLoginControls()
        {
            NxAuthLoginUsername.IsEnabled = !NxAuthLoginUsername.IsEnabled;
            NxAuthLoginPassword.IsEnabled = !NxAuthLoginPassword.IsEnabled;
            NxAuthLoginSubmit.IsEnabled = !NxAuthLoginSubmit.IsEnabled;
            NxAuthLoginCancel.IsEnabled = !NxAuthLoginCancel.IsEnabled;
            RememberMeCheckBox.IsEnabled = !RememberMeCheckBox.IsEnabled;

            NxAuthLoginLoadingIndicator.IsActive = !NxAuthLoginLoadingIndicator.IsActive;
        }

        private void ToggleDeviceControls()
        {
            NxDeviceTrustVerificationCode.IsEnabled = !NxDeviceTrustVerificationCode.IsEnabled;
            NxDeviceTrustRememberMe.IsEnabled = !NxDeviceTrustRememberMe.IsEnabled;
            NxDeviceTrustContinue.IsEnabled = !NxDeviceTrustContinue.IsEnabled;
            NxDeviceTrustCancel.IsEnabled = !NxDeviceTrustCancel.IsEnabled;

            NxDeviceTrustLoadingIndicator.IsActive = !NxAuthLoginLoadingIndicator.IsActive;
        }

        private async void UpdateAvailableLinkClick(object sender, RoutedEventArgs e)
        {
            var response = await this.ShowMessageAsync("Update Available",
                "A new version of Hyddwn Launcher is available." +
                "\r\n" +
                "\r\n" +
                $"Current Version: {Assembly.GetExecutingAssembly().GetName().Version}\r\n" +
                $"New Version: {_updateInfo["Version"]}", MessageDialogStyle.AffirmativeAndNegative);

            if (response != MessageDialogResult.Affirmative) return;

            _backgroundWorker.DoWork += _backgroundWorker_DoWork;

            _backgroundWorker.RunWorkerAsync();
        }

        private async void UpdaterUpdate()
        {
            if (File.Exists("Updater.exe")) return;
            MainProgressReporter.RighTextBlock.SetTextBlockSafe("Downloading App Updater...");
            MainProgressReporter.SetProgressBar(0);
            MainProgressReporter.ReporterProgressBar.SetVisibilitySafe(Visibility.Visible);
            await AsyncDownloader.DownloadFileWithCallbackAsync("http://www.imabrokedude.com/Updater.zip",
                "Updater.zip",
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

        #endregion
    }
}