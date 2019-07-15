using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using HyddwnLauncher.Annotations;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Properties;

namespace HyddwnLauncher.Core
{
    public class ClientProfile : INotifyPropertyChanged, IClientProfile
    {
        private string _guid;
        private string _localization;
        private string _location;
        private string _name;
        private string _profileUsername;
        private string _profileImageUri;
        private string _arguments;
        private bool _enableMultiClientMemoryEdit;
        private int _lastVersionForPatternSearch;
        private IntPtr _lastAddressForPatternSearch;
        [field:NonSerialized] private bool _hasError;

        public ClientProfile()
        {
            Localization = "North America";
            EnableMultiClientMemoryEdit = false;
        }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Location
        {
            get => _location;
            set
            {
                if (value == _location) return;
                _location = value;
                OnPropertyChanged();
            }
        }

        public string Guid
        {
            get => _guid;
            set
            {
                if (value == _guid) return;
                _guid = value;
                OnPropertyChanged();
            }
        }

        public string Localization
        {
            get => _localization;
            set
            {
                if (value == _localization) return;
                _localization = value;
                OnPropertyChanged();
            }
        }

        public string ProfileUsername
        {
            get => _profileUsername;
            set
            {
                if (value == _profileUsername) return;
                _profileUsername = value;
                OnPropertyChanged();
            }
        }

        public string ProfileImageUri
        {
            get => _profileImageUri;
            set
            {
                if (value == _profileImageUri) return;
                _profileImageUri = value;
                OnPropertyChanged();
            }
        }

        public string Arguments
        {
            get => _arguments;
            set
            {
                if (value == _arguments) return;
                _arguments = value;
                OnPropertyChanged();
            }
        }

        public bool EnableMultiClientMemoryEdit
        {
            get => _enableMultiClientMemoryEdit;
            set
            {
                if (value == _enableMultiClientMemoryEdit) return;
                _enableMultiClientMemoryEdit = value;
                OnPropertyChanged();
            }
        }

        public int LastVersionForPatternSearch
        {
            get => _lastVersionForPatternSearch;
            set
            {
                if (value == _lastVersionForPatternSearch) return;
                _lastVersionForPatternSearch = value;
                OnPropertyChanged();
            }
        }

        public IntPtr LastAddressForPatterSearch
        {
            get => _lastAddressForPatternSearch;
            set
            {
                if (value == _lastAddressForPatternSearch) return;
                _lastAddressForPatternSearch = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}