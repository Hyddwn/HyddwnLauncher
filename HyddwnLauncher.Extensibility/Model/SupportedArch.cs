using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public class SupportedArch
    {
        [JsonProperty("bit32")]
        public bool Is32BitSupported { get; set; }

        [JsonProperty("bit64")]
        public bool Is64BitSupported { get; set; }
    }
}
