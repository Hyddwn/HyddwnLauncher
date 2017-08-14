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
        private bool _deleteContent;
        private bool _deletePartFiles;
        private bool _deleteZips;
        private bool _firstRun;
        private bool _hyddwnProfileUpgrade;
        private string _locale;
        private bool _requiresAdmin;
        private int _serverProfileSelectedIndex;

        private bool _usePackFiles;
        private string _uuid;
        private bool _warnIfRootIsNotMabiRoot;

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

        public bool DeletePartFiles
        {
            get => _deletePartFiles;
            set
            {
                if (value == _deletePartFiles) return;
                _deletePartFiles = value;
                OnPropertyChanged();
            }
        }

        public bool DeleteZips
        {
            get => _deleteZips;
            set
            {
                if (value == _deleteZips) return;
                _deleteZips = value;
                OnPropertyChanged();
            }
        }

        public bool DeleteContent
        {
            get => _deleteContent;
            set
            {
                if (value == _deleteContent) return;
                _deleteContent = value;
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

        public string Locale
        {
            get => _locale;
            set
            {
                if (value == _locale) return;
                _locale = value;
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

        public int MaxConnectionLimit => Settings.Default.MaxConnectionLimit;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Load()
        {
            UsePackFiles = Settings.Default.UsePackFiles;
            DeletePartFiles = Settings.Default.DeletePartFiles;
            DeleteContent = Settings.Default.DeleteContent;
            DeleteZips = Settings.Default.DeleteZips;
            RequiresAdmin = Settings.Default.RequireAdmin;
            WarnIfRootIsNotMabiRoot = Settings.Default.WarnIfNotInMabiDir;
            FirstRun = Settings.Default.FirstRun;
            Locale = Settings.Default.Locale;
            ClientProfileSelectedIndex = Settings.Default.ClientProfileSelectedIndex;
            ServerProfileSelectedIndex = Settings.Default.ServerProfileSelectedIndex;
            ConnectionLimit = Settings.Default.DefaultConnectionLimit;
            HyddwnProfileUpgrade = Settings.Default.HyddwnProfileUpgrade;
            Uuid = Settings.Default.UUID;
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
            Settings.Default.DeletePartFiles = DeletePartFiles;
            Settings.Default.DeleteContent = DeleteContent;
            Settings.Default.DeleteZips = DeleteZips;
            Settings.Default.RequireAdmin = RequiresAdmin;
            Settings.Default.WarnIfNotInMabiDir = WarnIfRootIsNotMabiRoot;
            Settings.Default.FirstRun = FirstRun;
            Settings.Default.Locale = Locale;
            Settings.Default.ClientProfileSelectedIndex = ClientProfileSelectedIndex;
            Settings.Default.ServerProfileSelectedIndex = ServerProfileSelectedIndex;
            Settings.Default.DefaultConnectionLimit = ConnectionLimit;
            Settings.Default.HyddwnProfileUpgrade = HyddwnProfileUpgrade;
            Settings.Default.UUID = Uuid;
            Settings.Default.Save();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}