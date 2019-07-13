using System;
using System.Collections.ObjectModel;
using System.IO;
using HyddwnLauncher.Extensibility;
using Newtonsoft.Json;

namespace HyddwnLauncher.Patcher.NxLauncher
{
    /// <summary>
    /// Wrapper class around the 'patchignore.json' file for this plugin.
    /// </summary>
    public class PatchIgnore
    {
        /// <summary>
        /// The files that have been ignored
        /// </summary>
        public ObservableCollection<string> IgnoredFiles;

        private PluginContext _pluginContext;

        public PatchIgnore(PluginContext pluginContext)
        {
            _pluginContext = pluginContext;
        }

        /// <summary>
        /// Check for .patchignore file and loads the settings
        /// </summary>
        public void Initialize(string clientPath)
        {
            var patchIgnoreJsonPath = $"{clientPath}\\patchignore.json";

            _pluginContext.LogString(string.Format("Looking for {0}", patchIgnoreJsonPath), false);

            if (!File.Exists(patchIgnoreJsonPath))
            {
                IgnoredFiles = new ObservableCollection<string>();
                var patchignoreJson = JsonConvert.SerializeObject(IgnoredFiles, Formatting.Indented);
                File.AppendAllText(patchIgnoreJsonPath, patchignoreJson);
                return;
            }

            IgnoredFiles = JsonConvert.DeserializeObject<ObservableCollection<string>>(File.ReadAllText(patchIgnoreJsonPath));
        }
    }
}
