using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;

namespace HyddwnLauncher.UOTiara
{
    public class UOTiaraPlugin : PluginBase
    {
        private Guid _guid;
        private PluginContext _pluginContext;
        private IClientProfile _clientProfile;
        private UOTiaraPluginUI _pluginUi;

        public override void Initialize(PluginContext pluginContext, IClientProfile clientProfile, IServerProfile serverProfile)
        {
            Name = "UOTiara";
            _guid = Guid.NewGuid();
            _pluginContext = pluginContext;
            _clientProfile = clientProfile;
            _pluginUi = new UOTiaraPluginUI(_pluginContext, _clientProfile);
        }

        public override Guid GetGuid()
        {
            return _guid;
        }

        public override void ClientProfileChanged(IClientProfile clientProfile)
        {
            _clientProfile = clientProfile;
            _pluginUi.ClientProfileChanged(_clientProfile);
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
