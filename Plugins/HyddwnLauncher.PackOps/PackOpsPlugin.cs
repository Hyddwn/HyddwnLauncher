using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Extensibility.Primitives;
using HyddwnLauncher.PackOps.Core;

namespace HyddwnLauncher.PackOps
{
	[Export(typeof(IPlugin))]
	public class PackOpsPlugin : PluginBase
	{
		private PackOpsPluginUI _pluginUi;

		public override void Initialize(PluginContext pluginContext, IClientProfile activeClientProfile,
			IServerProfile activeServerProfile)
		{
		    base.Initialize(pluginContext, activeClientProfile, activeServerProfile);

            Name = "PackOps";

			PackOpsSettingsManager.Initialize(PluginContext);

			_pluginUi = new PackOpsPluginUI(PluginContext, ClientProfile, ServerProfile);
		}

		public override void ClientProfileChanged(IClientProfile clientProfile)
		{
		    base.ClientProfileChanged(clientProfile);
            _pluginUi.ClientProfileChangedAsync(ClientProfile);
		}

		public override void ServerProfileChanged(IServerProfile serverProfile)
		{
		    base.ServerProfileChanged(serverProfile);
            _pluginUi.ServerProfileChanged(ServerProfile);
		}

		public override void PatchEnd()
		{
			_pluginUi.GetPacksForClientProfile();
		}

		public override UserControl GetPluginUi()
		{
			return _pluginUi;
		}
	}
}
