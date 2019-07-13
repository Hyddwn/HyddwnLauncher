using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Patcher
{
    public class DefaultPatcher : IPatcher
    {
        public IClientProfile ClientProfile { get; set; }
        public IServerProfile ServerProfile { get; set; }
        public string PatcherType { get; set; } 
        public PatcherContext PatcherContext { get; set; }

        public DefaultPatcher(IClientProfile clientProfile, IServerProfile serverProfile, PatcherContext patcherContext)
        {
            ClientProfile = clientProfile;
            ServerProfile = serverProfile;
            PatcherContext = patcherContext;
            PatcherType = DefaultPatcherTypes.Default;
        }

        public virtual async Task<bool> CheckForUpdates()
        {
            return false;
        }

        public virtual async Task<bool> ApplyUpdates()
        {
            return true;
        }

        public virtual int ReadVersion()
        {
            try
            {
                return File.Exists("version.dat") ? BitConverter.ToInt32(File.ReadAllBytes("version.dat"), 0) : 0;
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

        public virtual async Task<string> GetLauncherArguments()
        {
            return $"code:1622 verstr:{ReadVersion()} ver:{ReadVersion()} logip:{ServerProfile.LoginIp} logport:{ServerProfile.LoginPort} chatip:{ServerProfile.ChatIp} chatport:{ServerProfile.ChatPort} {ClientProfile.Localization.ToExtendedLaunchArguments()}";
        }

        public virtual async Task<bool> GetMaintenanceStatus()
        {
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
