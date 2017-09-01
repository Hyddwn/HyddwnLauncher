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

        public LauncherSettingsManager()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_configurationJson)))
                Directory.CreateDirectory(Path.GetDirectoryName(_configurationJson));

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
            try
            {
                var fs = new FileStream(_configurationJson, FileMode.Open);
                var sr = new StreamReader(fs);

                var result = JsonConvert.DeserializeObject<LauncherSettings>(sr.ReadToEnd());

                sr.Dispose();
                fs.Dispose();

                return result;
            }
            catch (Exception)
            {
                return new LauncherSettings();
            }
        }

        public void SaveLauncherSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(LauncherSettings, Formatting.Indented);
                if (File.Exists(_configurationJson))
                    File.Delete(_configurationJson);

                File.WriteAllText(_configurationJson, json);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Unable to save configuration data");
                MessageBox.Show("Unable to save configuration data.\r\n\r\n" + ex.Message, "Error");
            }
        }
    }
}
