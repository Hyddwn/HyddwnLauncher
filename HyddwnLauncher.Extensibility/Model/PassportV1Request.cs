using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public class PassportV1Request
    {
        [JsonProperty(PropertyName = "product_id")]
        public string ProductId { get; set; }
    }
}
