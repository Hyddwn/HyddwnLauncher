using System;
using System.Collections.ObjectModel;
using System.IO;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Util;
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

        private PatcherContext _pluginContext;

        public PatchIgnore(PatcherContext patcherContext)
        {
            _pluginContext = patcherContext;
        }

        /// <summary>
        /// Check for patchignore.json file and loads the settings
        /// </summary>
        public void Initialize(string clientPath)
        {
            clientPath = Path.GetDirectoryName(clientPath);

            var patchIgnoreJsonPath = $"{clientPath}\\Hyddwn Launcher\\patchignore.json";

            if (!Directory.Exists($"{clientPath}\\Hyddwn Launcher"))
                Directory.CreateDirectory($"{clientPath}\\Hyddwn Launcher");

            Log.Info ("Looking for {0}", patchIgnoreJsonPath);

            if (!File.Exists(patchIgnoreJsonPath))
            {
                IgnoredFiles = new ObservableCollection<string>();
                var patchIgnoreJson = JsonConvert.SerializeObject(IgnoredFiles, Formatting.Indented);
                File.AppendAllText(patchIgnoreJsonPath, patchIgnoreJson);
                return;
            }

            IgnoredFiles = JsonConvert.DeserializeObject<ObservableCollection<string>>(File.ReadAllText(patchIgnoreJsonPath));
        }
    }
}
