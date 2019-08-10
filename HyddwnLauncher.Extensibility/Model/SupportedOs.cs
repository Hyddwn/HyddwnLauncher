using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public class SupportedOs
    {
        [JsonProperty("windowsVista")]
        public bool WindowsVista { get; set; }

        [JsonProperty("windows7")]
        public bool Windows7 { get; set; }

        [JsonProperty("windows8")]
        public bool Windows8 { get; set; }

        [JsonProperty("windows10")]
        public bool Windows10 { get; set; }
    }
}
