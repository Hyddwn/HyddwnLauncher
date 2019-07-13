using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using HyddwnLauncher.Core;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Util;
using Newtonsoft.Json;

namespace HyddwnLauncher.Patcher.NxLauncher
{
    public class PatchSettingsManager
    {
        private readonly string _patcherSettingsJson = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Hyddwn Launcher\\PatcherSettings.json";

        public static PatchSettingsManager Instance;
        private SettingsManager SettingsManager { get; }

        public PatcherSettings PatcherSettings { get; protected set; }
        public bool ConfigurationDirty { get; }

        private PatcherSettings DefaultSettings => new PatcherSettings
        {
            ForceUpdateCheck = false,
            IgnorePackageFolder = true
        };

        public static void Initialize()
        {
            if (Instance == null)
                Instance = new PatchSettingsManager();
        }

        private PatchSettingsManager()
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(_patcherSettingsJson)))
                    Directory.CreateDirectory(Path.GetDirectoryName(_patcherSettingsJson)
                                              ?? throw new ApplicationException(
                                                  "An error occured when attempting to load the patch settings data: Path is Null!!!"));

                SettingsManager = new SettingsManager(_patcherSettingsJson);
                PatcherSettings = LoadPatcherSettings();
                PatcherSettings.SaveOnChanged += SaveOnChanged;
            }
            catch (Exception e)
            {
                try
                {
                    Log.Debug(e.Message);
                    Log.Debug(e.StackTrace);

                    File.Delete(_patcherSettingsJson);

                    if (File.Exists(_patcherSettingsJson + ".backup"))
                        File.Move(_patcherSettingsJson + ".backup", _patcherSettingsJson);
                    else
                        ConfigurationDirty = true;

                    SettingsManager = new SettingsManager(_patcherSettingsJson);
                    PatcherSettings = LoadPatcherSettings();
                    PatcherSettings.SaveOnChanged += SaveOnChanged;

                    Log.Error("An error occured with the the patch settings file that required it to be reset.");
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, $"While attempting to recover from corrupt or malformed the patch settings file: {ex.Message}");
                    throw;
                }
            }
        }

        private void SaveOnChanged(string propertyName)
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
            return SettingsManager.Load<PatcherSettings>();
        }

        public void SavePatcherSettings()
        {
            SettingsManager.Save(PatcherSettings);
        }
    }
}
