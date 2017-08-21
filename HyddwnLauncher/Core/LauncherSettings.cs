using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using HyddwnLauncher.Properties;

namespace HyddwnLauncher.Core
{
    // A wrapper around the .NET Settings system 
    // that supports INotifyPropertyChanged and Binding
    public class LauncherSettings : INotifyPropertyChanged
    {
        private int _clientProfileSelectedIndex;
        private int _connectionLimit;
        private bool _firstRun;
        private bool _hyddwnProfileUpgrade;
        private bool _requiresAdmin;
        private int _serverProfileSelectedIndex;
        private bool _rememberLogin;
        private bool _usePackFiles;
        private string _uuid;
        private bool _warnIfRootIsNotMabiRoot;
        private string _nxUsername;
        private string _nxPassword;

        public LauncherSettings()
        {
            Load();
        }

        public bool UsePackFiles
        {
            get => _usePackFiles;
            set
            {
                if (value == _usePackFiles) return;
                _usePackFiles = value;
                OnPropertyChanged();
            }
        }

        public bool RequiresAdmin
        {
            get => _requiresAdmin;
            set
            {
                if (value == _requiresAdmin) return;
                _requiresAdmin = value;
                OnPropertyChanged();
            }
        }

        public bool WarnIfRootIsNotMabiRoot
        {
            get => _warnIfRootIsNotMabiRoot;
            set
            {
                if (value == _warnIfRootIsNotMabiRoot) return;
                _warnIfRootIsNotMabiRoot = value;
                OnPropertyChanged();
            }
        }

        public bool FirstRun
        {
            get => _firstRun;
            set
            {
                if (value == _firstRun) return;
                _firstRun = value;
                OnPropertyChanged();
            }
        }

        public int ClientProfileSelectedIndex
        {
            get => _clientProfileSelectedIndex;
            set
            {
                if (value == _clientProfileSelectedIndex) return;
                _clientProfileSelectedIndex = value;
                OnPropertyChanged();
            }
        }

        public int ServerProfileSelectedIndex
        {
            get => _serverProfileSelectedIndex;
            set
            {
                if (value == _serverProfileSelectedIndex) return;
                _serverProfileSelectedIndex = value;
                OnPropertyChanged();
            }
        }

        public bool RememberLogin
        {
            get => _rememberLogin;
            set
            {
                if (value == _rememberLogin) return;
                _rememberLogin = value;
                OnPropertyChanged();
            }
        }

        public int ConnectionLimit
        {
            get => _connectionLimit;
            set
            {
                if (value == _connectionLimit) return;
                _connectionLimit = value;
                OnPropertyChanged();
            }
        }

        public bool HyddwnProfileUpgrade
        {
            get => _hyddwnProfileUpgrade;
            set
            {
                if (value == _hyddwnProfileUpgrade) return;
                _hyddwnProfileUpgrade = value;
                OnPropertyChanged();
            }
        }

        public string Uuid
        {
            get => _uuid;
            set
            {
                if (value == _uuid) return;
                _uuid = value;
                OnPropertyChanged();
            }
        }

        public string NxUsername
        {
            get => _nxUsername;
            set
            {
                if (value == _nxUsername) return;
                _nxUsername = value;
                OnPropertyChanged();
            }
        }

        public string NxPassword
        {
            get => _nxPassword;
            set
            {
                if (value == _nxPassword) return;
                _nxPassword = value;
                OnPropertyChanged();
            }
        }

        public int MaxConnectionLimit => Settings.Default.MaxConnectionLimit;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Load()
        {
            UsePackFiles = Settings.Default.UsePackFiles;
            RequiresAdmin = Settings.Default.RequireAdmin;
            WarnIfRootIsNotMabiRoot = Settings.Default.WarnIfNotInMabiDir;
            ClientProfileSelectedIndex = Settings.Default.ClientProfileSelectedIndex;
            ServerProfileSelectedIndex = Settings.Default.ServerProfileSelectedIndex;
            ConnectionLimit = Settings.Default.DefaultConnectionLimit;
            HyddwnProfileUpgrade = Settings.Default.HyddwnProfileUpgrade;
            RememberLogin = Settings.Default.RememberLogin;
            Uuid = Settings.Default.UUID;
            NxUsername = Settings.Default.NxUsername;
            NxPassword = Settings.Default.NxPassword;
            if (PropertyChanged == null)
                PropertyChanged += SaveOnChanged;
        }

        private void SaveOnChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "ConnectionLimit")
                ServicePointManager.DefaultConnectionLimit = ConnectionLimit;

            Save();
        }

        public void Reset()
        {
            Settings.Default.Reset();
            Settings.Default.Save();
            Load();
        }

        public void Save()
        {
            Settings.Default.UsePackFiles = UsePackFiles;
            Settings.Default.RequireAdmin = RequiresAdmin;
            Settings.Default.WarnIfNotInMabiDir = WarnIfRootIsNotMabiRoot;
            Settings.Default.ClientProfileSelectedIndex = ClientProfileSelectedIndex;
            Settings.Default.ServerProfileSelectedIndex = ServerProfileSelectedIndex;
            Settings.Default.DefaultConnectionLimit = ConnectionLimit;
            Settings.Default.HyddwnProfileUpgrade = HyddwnProfileUpgrade;
            Settings.Default.UUID = Uuid;
            Settings.Default.RememberLogin = RememberLogin;
            Settings.Default.NxUsername = NxUsername;
            Settings.Default.NxPassword = NxPassword;
            Settings.Default.Save();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}