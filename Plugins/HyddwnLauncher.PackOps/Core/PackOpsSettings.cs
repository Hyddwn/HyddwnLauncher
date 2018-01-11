using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HyddwnLauncher.PackOps.Core
{
    public class PackOpsSettings : INotifyPropertyChanged
    {
        private bool _deletePackFilesAfterMerge;

        public PackOpsSettings()
        {
            DeletePackFilesAfterMerge = true;
        }

        public bool DeletePackFilesAfterMerge
        {
            get => _deletePackFilesAfterMerge;
            set
            {
                if (value == _deletePackFilesAfterMerge) return;
                _deletePackFilesAfterMerge = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}