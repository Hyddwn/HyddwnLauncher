using System.Collections.Generic;
using System.ComponentModel;

namespace HyddwnLauncher.UOTiara.Util
{
    public class ModInfo : INotifyPropertyChanged
    {
        private bool _enabled;
        public string Name { get; private set; }
        public bool IsEnabled { get { return _enabled; } set { _enabled = value; NotifyPropertyChanged("IsEnabled"); } }
        public List<ModFileInfo> ModFiles { get; } = new List<ModFileInfo>();
        public ModInfo(bool enabled, string name)
        {
            IsEnabled = enabled;
            Name = name;
        }

        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ModFileInfo
    {
        public string ModName { get; }
        public string FileName { get; }

        public ModFileInfo(string modName, string fileName)
        {
            ModName = modName;
            FileName = fileName;
        }
    }
}
