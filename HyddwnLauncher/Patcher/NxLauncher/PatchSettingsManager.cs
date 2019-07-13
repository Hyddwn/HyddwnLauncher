using System;
using System.ComponentModel;
using System.IO;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Util;
using Newtonsoft.Json;

namespace HyddwnLauncher.Patcher.NxLauncher
{
    public class PatchSettingsManager
    {
        private static readonly string Assembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
        private static readonly string Assemblypath = Path.GetDirectoryName(Assembly);

        private readonly string _patcherSettingsJson = Assemblypath + "\\PatcherSettings.json";

        public static PatchSettingsManager Instance;

        public PatcherSettings PatcherSettings { get; protected set; }

        private PatcherSettings DefaultSettings => new PatcherSettings
        {
            ForceUpdateCheck = false,
            IgnorePackagehFolder = true
        };

        public static void Initialize()
        {
            if (Instance == null)
                Instance = new PatchSettingsManager();
        }

        private PatchSettingsManager()
        {
            PatcherSettings = LoadPatcherSettings();
            PatcherSettings.PropertyChanged += SaveOnChanged;
            // Hack for now...
            PatcherSettings.IgnorePackagehFolder = true;
        }

        private void SaveOnChanged(object sender, PropertyChangedEventArgs e)
        {
            SavePatcherSettings();
        }

        public void Reset()
        {
            PatcherSettings = DefaultSettings;
            SavePatcherSettings();
        }

        public PatcherSettings LoadPatcherSettings()
        {
            try
            {
                var fs = new FileStream(_patcherSettingsJson, FileMode.Open);
                var sr = new StreamReader(fs);

                var result = JsonConvert.DeserializeObject<PatcherSettings>(sr.ReadToEnd());

                sr.Dispose();
                fs.Dispose();

                return result;
            }
            catch (Exception)
            {
                return new PatcherSettings();
            }
        }

        public void SavePatcherSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(PatcherSettings, Formatting.Indented);
                if (File.Exists(_patcherSettingsJson))
                    File.Delete(_patcherSettingsJson);

                File.WriteAllText(_patcherSettingsJson, json);
            }
            catch (Exception ex)
            {
                Log.Info(string.Format("Unable to save Patcher settings. {0}", ex.Message), true);
            }
        }
    }
}
