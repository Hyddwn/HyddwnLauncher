using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public class LaunchConfig
    {
        [JsonProperty("requireUAC")]
        public bool RequireUAC { get; set; }

        [JsonProperty("exePath")]
        public string ClientPath { get; set; }

        [JsonProperty("args")]
        public List<string> Arguments { get; set; }

        [JsonProperty("workingDir")]
        public string WorkingDirectory { get; set; }
    }
}
