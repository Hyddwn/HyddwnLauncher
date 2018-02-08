using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Patcher.Legacy.Core;
using HyddwnLauncher.Patcher.Legacy.Core.Controls;

namespace HyddwnLauncher.Patcher.Legacy
{
    public class PatcherContext
    {
        public event Func<ProgressReporterViewModel, ProgressReporterViewModel> CreateProgressIndicator;
        public event Action<ProgressReporterViewModel> DestroyProgressIndecator;

        public ProgressReporterViewModel OnCreateProgressIndicator(ProgressReporterViewModel progressIndicator)
        {
            CreateProgressIndicator?.Invoke(progressIndicator);
            return progressIndicator;
        }

        public void OnDestroyProgressIndicator(ProgressReporterViewModel progressIndicator)
        {
            DestroyProgressIndecator?.Invoke(progressIndicator);
        }

        public PluginContext PluginContext { get; private set; }
        public Guid Guid { get; private set; }
        public IClientProfile ClientProfile { get; protected set; }
        public IServerProfile ServerProfile { get; protected set; }

        public static PatcherContext Instance = new PatcherContext();

        public void Initialize(IClientProfile clientProfile, IServerProfile serverProfile, PluginContext pluginContext, Guid guid)
        {
            ClientProfile = clientProfile;
            ServerProfile = serverProfile;
            PluginContext = pluginContext;
            Guid = guid;

            if (Instance == null)
                Instance = this;
        }

        public async void SetCientProfile(IClientProfile clientProfile)
        {
            ClientProfile = clientProfile;

            // Hardcode the check here because we don't want to show the alert when not needed.
            // Below: only patch official clients that are not localized for north america :D
            if (string.IsNullOrWhiteSpace(ClientProfile?.Location) ||
                ClientProfile.Localization == ClientLocalization.NorthAmerica || ServerProfile == null ||
                !ServerProfile.IsOfficial)
                return;

            var legacyPatcher = new LegacyPatcher();

            // If there is an update, UPDATE FOOL
            if (await legacyPatcher.CheckForUpdates())
            {
                await legacyPatcher.Update();
            }
        }

        public void SetServerProfile(IServerProfile serverProfile)
        {
            ServerProfile = serverProfile;
        }

        /// <summary>
        /// Determines if a function or action should or can be performed
        /// </summary>
        /// <returns></returns>
        private bool VerifyAction()
        {
            if (ClientProfile == null)
            {
                PluginContext.ShowDialog("Client Profile Not Selected", 
                    "There must be an active client profile in order to use this function.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(ClientProfile.Location))
            {
                PluginContext.ShowDialog("Malformed Profile",
                    "The current profile does not have a valid location set. This operation could not continue.");
                return false;
            }

            if (ClientProfile.Localization == ClientLocalization.NorthAmerica)
            {
                PluginContext.ShowDialog("Incompatible Localization",
                    $"This patcher is not compatible with the current localization. '{ClientLocalization.NorthAmerica}'");
                return false;
            }

            if (ServerProfile == null)
            {
                PluginContext.ShowDialog("Server Profile Not Selected",
                    "There must be an active server profile in order to use this function.");
                return false;
            }

            if (!ServerProfile.IsOfficial)
            {
                PluginContext.ShowDialog("Server Profile Not Offical",
                    $"This patcher is not compatible with the current profile. '{ServerProfile.Name}'");
                return false;
            }

            return true;
        }
    }
}
