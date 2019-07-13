using System;
using System.ComponentModel;
using System.IO;
using HyddwnLauncher.Extensibility;
using Newtonsoft.Json;

namespace HyddwnLauncher.PackOps.Core
{
	public class PackOpsSettingsManager
	{
		public static PackOpsSettingsManager Instance;

		private readonly string _packOpsJson =
			$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Hyddwn Launcher\\packops.json";

		private readonly PluginContext _pluginContext;

		private PackOpsSettingsManager(PluginContext pluginContext)
		{
			if (!Directory.Exists(Path.GetDirectoryName(_packOpsJson)))
				Directory.CreateDirectory(Path.GetDirectoryName(_packOpsJson));

		    _pluginContext = pluginContext;

			PackOpsSettings = LoadPackOpsSettings();
			PackOpsSettings.PropertyChanged += SaveOnChanged;
		}

		public PackOpsSettings PackOpsSettings { get; protected set; }

		public static void Initialize(PluginContext pluginContext)
		{
			if (Instance == null)
				Instance = new PackOpsSettingsManager(pluginContext);
		}

		private void SaveOnChanged(object sender, PropertyChangedEventArgs e)
		{
			SavePackOpsSettings();
		}

		public void SavePackOpsSettings()
		{
			try
			{
				var json = JsonConvert.SerializeObject(PackOpsSettings, Formatting.Indented);
				if (File.Exists(_packOpsJson))
					File.Delete(_packOpsJson);

				File.WriteAllText(_packOpsJson, json);
			}
			catch (Exception ex)
			{
				_pluginContext.LogString($"Unable to save PackOps settings. {ex.Message}", true);
			}
		}

		public PackOpsSettings LoadPackOpsSettings()
		{
			try
			{
				var fs = new FileStream(_packOpsJson, FileMode.Open);
				var sr = new StreamReader(fs);

				var result = JsonConvert.DeserializeObject<PackOpsSettings>(sr.ReadToEnd());

				sr.Dispose();
				fs.Dispose();

				return result;
			}
			catch (Exception ex)
			{
			    _pluginContext.LogString($"Unable to load PackOps settings. {ex.Message}", true);
                return new PackOpsSettings();
			}
		}

		public void Reset()
		{
			PackOpsSettings = new PackOpsSettings();
			SavePackOpsSettings();
		}
	}
}