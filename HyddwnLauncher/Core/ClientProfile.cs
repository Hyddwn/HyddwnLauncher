using System.ComponentModel;
using System.Runtime.CompilerServices;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Properties;

namespace HyddwnLauncher.Core
{
    public class ClientProfile : INotifyPropertyChanged, IClientProfile
    {
        private string _guid;
        private string _location;
        private string _name;

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}