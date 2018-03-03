using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HyddwnLauncher.Properties;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Core
{
    public class LauncherSettings : INotifyPropertyChanged
    {
        private string _accent;
        private int _clientProfileSelectedIndex;
        private int _connectionLimit;
        private bool _firstRun;
        private bool _hyddwnProfileUpgrade;
        private bool _rememberLogin;
        private bool _requiresAdmin;
        private int _serverProfileSelectedIndex;
        private string _theme;
        private bool _usePackFiles;
        private bool _warnIfRootIsNotMabiRoot;
        private bool _automaticallyCheckForUpdates;

        public LauncherSettings()
        {
            ClientProfileSelectedIndex = -1;
            ConnectionLimit = 10;
            FirstRun = true;
            HyddwnProfileUpgrade = true;
            RequiresAdmin = true;
            ServerProfileSelectedIndex = -1;
            RememberLogin = false;
            UsePackFiles = false;
            WarnIfRootIsNotMabiRoot = true;
            Theme = "BaseDark";
            Accent = "Cobalt";
            AutomaticallyCheckForUpdates = false;
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

        public string Theme
        {
            get => _theme;
            set
            {
                if (value == _theme) return;
                _theme = value;
                OnPropertyChanged();
            }
        }

        public string Accent
        {
            get => _accent;
            set
            {
                if (value == _accent) return;
                _accent = value;
                OnPropertyChanged();
            }
        }

        public bool AutomaticallyCheckForUpdates
        {
            get => _automaticallyCheckForUpdates;
            set
            {
                if (value == _automaticallyCheckForUpdates) return;
                _automaticallyCheckForUpdates = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<string> SaveOnChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            SaveOnChanged?.Raise(propertyName);
        }
    }
}