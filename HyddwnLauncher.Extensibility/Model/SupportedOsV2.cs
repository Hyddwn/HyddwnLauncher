using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public class SupportedOsV2
    {
        [JsonProperty("windows_vista")]
        public bool WindowsVista { get; set; }

        [JsonProperty("windows_7")]
        public bool Windows7 { get; set; }

        [JsonProperty("windows_8")]
        public bool Windows8 { get; set; }

        [JsonProperty("windows_10")]
        public bool Windows10 { get; set; }
    }
}