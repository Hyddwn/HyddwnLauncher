using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Extensibility.Util;
using HyddwnLauncher.Patcher.Legacy.Core;
using HyddwnLauncher.Patcher.Legacy.Core.Controls;

namespace HyddwnLauncher.Patcher.Legacy
{
    // TODO: Throwing comments everywhere for source divers. (so lazy right now)
    /// <summary>
    /// Interaction logic for LegacyPatcherUI.xaml
    /// </summary>
    public partial class LegacyPatcherUI : UserControl
    {
        private IClientProfile _clientProfile;
        private IServerProfile _serverProfile;

        private LegacyPatcher _legacyPatcher;

        public ObservableDictionary<ProgressReporterViewModel, ProgressReporter> Reporters { get; set; }

        public LegacyPatcherUI(IServerProfile serverProfile)
        {
            _serverProfile = serverProfile;
            Reporters = new ObservableDictionary<ProgressReporterViewModel, ProgressReporter>();

            InitializeComponent();

            PatcherContext.Instance.CreateProgressIndicator += model =>
            {
                Reporters.Add(model, new ProgressReporter(model));
                return model;
            };

            PatcherContext.Instance.DestroyProgressIndecator += model => { Reporters.Remove(model); };
        }

        public async void ClientProfileUpdated(IClientProfile clientProfile)
        {
            // set the client profile in the codebehind for the GUI
            _clientProfile = clientProfile;

            // Set the client profile in the global patcher context
            PatcherContext.Instance.SetCientProfile(clientProfile);

            // Client profile and server profile must be selected.
            // If Client profile's location is invalid OR server profile is not offical don't patch!
            if (string.IsNullOrWhiteSpace(_clientProfile?.Location) 
                || _serverProfile?.IsOfficial == false) return;

            // I call this legacy patcher but.... final build this class will be renamed
            // Create a new patcher (though theoretically I don't need to)
            _legacyPatcher = new LegacyPatcher();


            var updateAvailable = await _legacyPatcher.CheckForUpdates();

            if (updateAvailable)
            {
                await _legacyPatcher.Update();
            }
        }

        public void ServerProfileUpdated(IServerProfile serverProfile)
        {
            _serverProfile = serverProfile;
        }
    }
}
