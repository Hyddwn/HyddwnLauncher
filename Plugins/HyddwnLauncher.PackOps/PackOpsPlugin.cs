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
		private IClientProfile _clientProfile;
		private Guid _guid;
		private PluginContext _pluginContext;
		private PackOpsPluginUI _pluginUI;
		private IServerProfile _serverProfile;

		public override void Initialize(PluginContext pluginContext, IClientProfile activeClientProfile,
			IServerProfile activeServerProfile)
		{
			Name = "PackOps";
			_guid = Guid.NewGuid();
			_pluginContext = pluginContext;
			_clientProfile = activeClientProfile;
			_serverProfile = activeServerProfile;

			PackOpsSettingsManager.Initialize(_pluginContext);

			_pluginUI = new PackOpsPluginUI(_pluginContext, _clientProfile, _serverProfile);
		}

		public override void ClientProfileChanged(IClientProfile clientProfile)
		{
			_clientProfile = clientProfile;
			_pluginUI.ClientProfileChangedAsync(_clientProfile);
		}

		public override void ServerProfileChanged(IServerProfile serverProfile)
		{
			_serverProfile = serverProfile;
			_pluginUI.ServerProfileChanged(_serverProfile);
		}

		public override Guid GetGuid()
		{
			return _guid;
		}

		public override UserControl GetPluginUi()
		{
			return _pluginUI;
		}
	}
}