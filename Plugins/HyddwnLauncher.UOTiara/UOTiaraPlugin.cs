using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Extensibility.Primitives;

namespace HyddwnLauncher.UOTiara
{
    [Export(typeof(IPlugin))]
    public class UOTiaraPlugin : PluginBase
    {
        private UOTiaraPluginUI _pluginUi;

        public override void Initialize(PluginContext pluginContext, IClientProfile activeClientProfile,
            IServerProfile activeServerProfile)
        {
            base.Initialize(pluginContext, activeClientProfile, activeServerProfile);

            Name = "UOTiara";

            _pluginUi = new UOTiaraPluginUI(PluginContext, ClientProfile, ServerProfile);
        }


        public override void ClientProfileChanged(IClientProfile clientProfile)
        {
            base.ClientProfileChanged(clientProfile);
            _pluginUi.ClientProfileChanged(ClientProfile);
        }

        public override UserControl GetPluginUi()
        {
            return _pluginUi;
        }

        public override void PreLaunch()
        {
            _pluginUi.PreLaunch();
        }
    }
}
