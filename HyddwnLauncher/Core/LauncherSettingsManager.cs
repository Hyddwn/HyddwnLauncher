using System;
using System.IO;
using System.Net;
using System.Windows;
using HyddwnLauncher.Util;
using Newtonsoft.Json;

namespace HyddwnLauncher.Core
{
	public class LauncherSettingsManager
	{
		private readonly string _configurationJson = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Hyddwn Launcher\\configuration.json";

		public LauncherSettings LauncherSettings { get; protected set; }
		private SettingsManager SettingsManager {  get; }

		public LauncherSettingsManager()
		{
			if (!Directory.Exists(Path.GetDirectoryName(_configurationJson)))
				Directory.CreateDirectory(Path.GetDirectoryName(_configurationJson));

			SettingsManager = new SettingsManager(_configurationJson);
			LauncherSettings = LoadLauncherSettings();
			LauncherSettings.SaveOnChanged += SaveOnChanged;
		}

		private void SaveOnChanged(string propertyName)
		{
			if (propertyName == "ConnectionLimit")
				ServicePointManager.DefaultConnectionLimit = LauncherSettings.ConnectionLimit;

			SaveLauncherSettings();
		}

		public void Reset()
		{
			LauncherSettings = new LauncherSettings();
			SaveLauncherSettings();
		}

		public LauncherSettings LoadLauncherSettings()
		{
			return SettingsManager.Load<LauncherSettings>();
		}

		public void SaveLauncherSettings()
		{
			SettingsManager.Save(LauncherSettings);
		}
	}
}
