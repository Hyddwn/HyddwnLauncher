using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.UOTiara.Util;

namespace HyddwnLauncher.UOTiara
{
    /// <summary>
    ///     Interaction logic for UOTiaraPluginUI.xaml
    /// </summary>
    public partial class UOTiaraPluginUI : UserControl
    {
        private readonly PluginContext _pluginContext;
        private IClientProfile _clientProfile;

        public UOTiaraPluginUI(PluginContext pluginContext, IClientProfile clientProfile)
        {
            _pluginContext = pluginContext;
            _clientProfile = clientProfile;

            InitializeComponent();
        }

        public ObservableCollection<ModInfo> ModInfoList { get; set; } = new ObservableCollection<ModInfo>();

        private string MabiRoot => Path.Combine(Path.GetDirectoryName(_clientProfile.Location), "..");

        private async void button3_Click(object sender, RoutedEventArgs e)
        {
            await LoadModList();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            //RunUpdate();
        }

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public async Task LoadModList()
        {
            if (_clientProfile == null) return;

            Dispatcher.Invoke(() =>
            {
                ModInfoList.Clear();
                LoadingOverlayText.Text = "Loading Mod List...";
                _pluginContext.SetPatcherState(true);
                LoadingOverlay.IsOpen = true;
            });

            await Task.Delay(250);

            await Task.Run(() =>
            {
                try
                {
                    var registryHelper = new RegistryHelper();
                    var subkeyTemp = registryHelper.SubKey + "\\Components";
                    registryHelper.SubKey = subkeyTemp;

                    var list = new List<ModInfo>();

                    foreach (var subkeyName in registryHelper.GetSubKeyNames())
                    {
                        registryHelper.SubKey = $"{subkeyTemp}\\{subkeyName}";

                        var modName = registryHelper.Read();
                        var creator = registryHelper.Read("CREATOR");
                        var description = registryHelper.Read("DESCRIPTION");
                        var fileCount = registryHelper.ReadInt("FILES");
                        var isEnabled = Convert.ToBoolean(registryHelper.Read<int>("Installed"));

                        if (string.IsNullOrWhiteSpace(modName)) continue;

                        var modInfo = new ModInfo(isEnabled, modName, creator, description);

                        for (var x = 1; x <= fileCount; x++)
                        {
                            var filename = registryHelper.Read($"FILE{x}");

                            var modFileInfo = new ModFileInfo(modName, filename);
                            modInfo.ModFiles.Add(modFileInfo);
                        }

                        list.Add(modInfo);
                    }

                    // ParseModInfo to build the structure

                    Dispatcher.Invoke(() => { LoadingOverlayText.Text = "Processing..."; });
                }
                catch (Exception ex)
                {
                    _pluginContext.LogString($"Failed to read data for an entry: {ex.Message}", false);
                }
            });

            Dispatcher.Invoke(() =>
            {
                LoadingOverlayText.Text = "";
                _pluginContext.SetPatcherState(false);
                LoadingOverlay.IsOpen = false;
            });
        }

        public void PreLaunch()
        {
            // Create the pack here :D
            // var packer = _pluginContext.GetPackEngine();
            // blah set up data folder for pack blah
            // packer.Pack("path to data folder", "packfile name", version, [optional] compresstionLevel);
        }

        public void ClientProfileChanged(IClientProfile clientProfile)
        {
            _clientProfile = clientProfile;
        }
    }
}