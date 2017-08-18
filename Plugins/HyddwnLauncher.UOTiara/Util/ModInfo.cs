using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HyddwnLauncher.UOTiara.Annotations;

namespace HyddwnLauncher.UOTiara.Util
{
    public class ModInfo : INotifyPropertyChanged
    {
        private bool _enabled;
        public string Name { get; }
        public string Creator { get; }
        public string Description { get; }

        public bool IsEnabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                NotifyPropertyChanged("IsEnabled");
            }
        }

        public List<ModFileInfo> ModFiles { get; } = new List<ModFileInfo>();
        public ModInfo(bool enabled, string name, string creator, string description)
        {
            IsEnabled = enabled;
            Name = name;
            Creator = creator;
            Description = description;

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName != nameof(IsEnabled)) return;

            foreach (var modFile in ModFiles)
                modFile.IsEnabled = IsEnabled;
        }

        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ModFileInfo : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public string ModName { get; }
        public string FileName { get; }

        public bool IsEnabled   
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public ModFileInfo(string modName, string fileName)
        {
            ModName = modName;
            FileName = fileName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
