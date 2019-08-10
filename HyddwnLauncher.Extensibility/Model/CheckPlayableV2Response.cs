using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public struct CheckPlayableV2Response
    {
        [JsonProperty(PropertyName = "product_id")]
        public string ProductId { get; set; }
    }
}
