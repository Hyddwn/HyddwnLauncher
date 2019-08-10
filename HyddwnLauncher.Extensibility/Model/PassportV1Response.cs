using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public class GetPassportV1Response
    {
        [JsonProperty(PropertyName = "need_update_session")]
        public string NeedUpdateSession { get; set; }

        [JsonProperty(PropertyName = "passport")]
        public string Passport { get; set; }
    }
}
