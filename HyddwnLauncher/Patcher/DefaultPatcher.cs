using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HyddwnLauncher.Annotations;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Patcher
{
    public class DefaultPatcher : IPatcher
    {
        private IClientProfile _clientProfile;
        private IServerProfile _serverProfile;
        private string _patcherType;
        private PatcherContext _patcherContext;

        public IClientProfile ClientProfile
        {
            get => _clientProfile;
            set
            {
                if (Equals(value, _clientProfile)) return;
                _clientProfile = value;
                OnPropertyChanged();
            }
        }

        public IServerProfile ServerProfile
        {
            get => _serverProfile;
            set
            {
                if (Equals(value, _serverProfile)) return;
                _serverProfile = value;
                OnPropertyChanged();
            }
        }

        public string PatcherType
        {
            get => _patcherType;
            set
            {
                if (value == _patcherType) return;
                _patcherType = value;
                OnPropertyChanged();
            }
        }

        public PatcherContext PatcherContext
        {
            get => _patcherContext;
            set
            {
                if (Equals(value, _patcherContext)) return;
                _patcherContext = value;
                OnPropertyChanged();
            }
        }

        public DefaultPatcher(IClientProfile clientProfile, IServerProfile serverProfile, PatcherContext patcherContext)
        {
            ClientProfile = clientProfile;
            ServerProfile = serverProfile;
            PatcherContext = patcherContext;
            PatcherType = DefaultPatcherTypes.Default;
        }

        public virtual Task<bool> CheckForUpdatesAsync()
        {
            return Task.Run(() => false);
        }

        public virtual Task<bool> ApplyUpdatesAsync()
        {
            return Task.Run(() => true);
        }

        public virtual Task<bool> RepairInstallAsync()
        {
            return Task.Run(() => true);
        }

        public virtual int ReadVersion()
        {
            try
            {
                if (File.Exists("version.dat"))
                {
                    return BitConverter.ToInt32(File.ReadAllBytes("version.dat"), 0);
                }

                WriteVersion(0);
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public virtual void WriteVersion(int version)
        {
            Log.Info(Properties.Resources.WritingVersionToVersionDat, version);
            File.WriteAllBytes("version.dat", BitConverter.GetBytes(version));
        }

        public virtual Task<string> GetLauncherArgumentsAsync()
        {
            return Task.Run(() => $"code:1622 verstr:{ReadVersion()} ver:{ReadVersion()} logip:{ServerProfile.LoginIp} logport:{ServerProfile.LoginPort} chatip:{ServerProfile.ChatIp} chatport:{ServerProfile.ChatPort} {ClientProfile.Localization.ToExtendedLaunchArguments()} {ClientProfile.Arguments}");
        }

        public virtual Task<bool> GetMaintenanceStatusAsync()
        {
            return Task.Run(() => false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
