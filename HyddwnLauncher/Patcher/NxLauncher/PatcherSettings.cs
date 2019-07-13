using System.ComponentModel;
using System.Runtime.CompilerServices;
using HyddwnLauncher.Annotations;
using HyddwnLauncher.Properties;

namespace HyddwnLauncher.Patcher.NxLauncher
{
    public class PatcherSettings : INotifyPropertyChanged
    {

        private bool _ignorePackagehFolder;
        private bool _forceUpdateCheck;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IgnorePackagehFolder
        {
            get => _ignorePackagehFolder;
            set
            {
                if (value == _ignorePackagehFolder) return;
                _ignorePackagehFolder = value;
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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
