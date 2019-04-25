﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public class GetManifestResponse
    {
        [JsonProperty("manifestUrl")]
        public string ManifestUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
