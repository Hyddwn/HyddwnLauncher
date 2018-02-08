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

namespace HyddwnLauncher.Patcher.Legacy
{
    [Export(typeof(IPlugin))]
    public class LegacyPatcherPlugin : PluginBase
    {
        private LegacyPatcherUI _legacyPatcherUi;

        public override void Initialize(PluginContext pluginContext, IClientProfile activeClientProfile, IServerProfile activeServerProfile)
        {
            base.Initialize(pluginContext, activeClientProfile, activeServerProfile);

            Name = "Patcher (Legacy)";

            PatcherContext.Instance.Initialize(ClientProfile, ServerProfile, PluginContext, Guid);

            _legacyPatcherUi = new LegacyPatcherUI(ServerProfile);
        }

        public override void ClientProfileChanged(IClientProfile clientProfile)
        {
            base.ClientProfileChanged(clientProfile);
            _legacyPatcherUi.ClientProfileUpdated(clientProfile);
        }

        public override void ServerProfileChanged(IServerProfile serverProfile)
        {
            base.ServerProfileChanged(serverProfile);
            _legacyPatcherUi.ServerProfileUpdated(serverProfile);
        }

        public override UserControl GetPluginUi()
        {
            return _legacyPatcherUi;
        }
    }
}
