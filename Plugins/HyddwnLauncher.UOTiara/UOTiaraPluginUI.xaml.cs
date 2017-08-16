using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
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
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.UOTiara.Util;

namespace HyddwnLauncher.UOTiara
{
    /// <summary>
    /// Interaction logic for UOTiaraPluginUI.xaml
    /// </summary>
    public partial class UOTiaraPluginUI : UserControl
    {
        public ObservableCollection<ModInfo> ModInfoList { get; set; } = new ObservableCollection<ModInfo>();
        private readonly PluginContext _pluginContext;
        private IClientProfile _clientProfile;

        private string MabiRoot => Path.Combine(Path.GetDirectoryName(_clientProfile.Location), "..");

        public UOTiaraPluginUI(PluginContext pluginContext, IClientProfile clientProfile)
        {
            _pluginContext = pluginContext;
            _clientProfile = clientProfile;

            InitializeComponent();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            LoadUOTiaraTOList();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            //RunUpdate();
        }

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void LoadUOTiaraTOList()
        {
            if (_clientProfile == null) return;

            _pluginContext.MainUpdater("Loading Mods...", "Retrieving local config", 0, true, true);

            try
            {
                ModInfoList.Clear();

                var registryHelper = new RegistryHelper();
                var subkeyTemp = registryHelper.SubKey + "\\Components";
                registryHelper.SubKey = subkeyTemp;

                foreach (var subkeyName in registryHelper.GetSubKeyNames())
                {
                    registryHelper.SubKey = $"{subkeyTemp}\\{subkeyName}";

                    var isEnabled = Convert.ToBoolean(registryHelper.Read<int>("Installed"));
                    var modName = registryHelper.Read();
                    var fileCount = registryHelper.ReadInt("FILES");

                    if (string.IsNullOrWhiteSpace(modName)) continue;

                    var modInfo = new ModInfo(isEnabled, modName);

                    for (var x = 1; x <= fileCount; x++)
                    {
                        var filename = registryHelper.Read($"FILE{x}");

                        var modFileInfo = new ModFileInfo(modName, filename);
                        modInfo.ModFiles.Add(modFileInfo);
                    }

                    ModInfoList.Add(modInfo);
                }

                _pluginContext.MainUpdater("Loaded Mods!", "", 0, false, false);
            }
            catch (Exception ex)
            {
                _pluginContext.LogString($"Failed to read data for an entry: {ex.Message}" , false);
            }
        }

        public void PreLaunch()
        {
            // Create the pack here :D
            // var packer = _pluginContext.GetPackEngine();
            // blah set up data folder for pack blah
            // packer.Pack("path to data folder", "packfile name", version, [optional] compresstionLevel);
        }

        public void ClientProfileChanged(IClientProfile clientProfile) => _clientProfile = clientProfile;
    }
}
