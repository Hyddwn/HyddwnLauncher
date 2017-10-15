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
		    try
		    {
		        if (!Directory.Exists(Path.GetDirectoryName(_configurationJson)))
		            Directory.CreateDirectory(Path.GetDirectoryName(_configurationJson) ?? throw new ApplicationException("An error occured when attempting to load the configuration data: Path is Null!!!"));

		        SettingsManager = new SettingsManager(_configurationJson);
		        LauncherSettings = LoadLauncherSettings();
		        LauncherSettings.SaveOnChanged += SaveOnChanged;
            }
		    catch (ApplicationException e)
		    {
		        try
		        {
                    Log.Debug(e.Message);
                    Log.Debug(e.StackTrace);

		            File.Delete(_configurationJson);
		            SettingsManager = new SettingsManager(_configurationJson);
		            LauncherSettings = LoadLauncherSettings();
		            LauncherSettings.SaveOnChanged += SaveOnChanged;

                    Log.Error("An error occured with the configuration file that required it to be reset.");
		        }
		        catch (Exception ex)
		        {
		            Log.Exception(ex, $"While attempting to recover from corrupt or malformed config: {ex.Message}");
		            throw;
		        }
		    }
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
