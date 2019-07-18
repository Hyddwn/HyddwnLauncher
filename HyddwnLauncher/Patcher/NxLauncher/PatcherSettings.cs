using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HyddwnLauncher.Annotations;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Patcher.NxLauncher
{
    public class PatcherSettings : INotifyPropertyChanged
    {
        private bool _ignorePackageFolder;
        private bool _forceUpdateCheck;
        private bool _promptBeforePatching;

        public PatcherSettings()
        {
            ForceUpdateCheck = false;
            IgnorePackageFolder = true;
            PromptBeforePatching = false;
        }

        public bool IgnorePackageFolder
        {
            get => _ignorePackageFolder;
            set
            {
                if (value == _ignorePackageFolder) return;
                _ignorePackageFolder = value;
                OnPropertyChanged();
            }
        }

        public bool ForceUpdateCheck
        {
            get => _forceUpdateCheck;
            set
            {
                if (value == _forceUpdateCheck) return;
                _forceUpdateCheck = value;
                OnPropertyChanged();
            }
        }

        public bool PromptBeforePatching
        {
            get => _promptBeforePatching;
            set
            {
                if (value == _promptBeforePatching) return;
                _promptBeforePatching = value;
                OnPropertyChanged();
            }
        }

        public event Action<string> SaveOnChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SaveOnChanged?.Raise(propertyName);
        }
    }
}
