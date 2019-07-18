using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public struct IdTokenRefreshRequest
    {
        [JsonProperty(PropertyName = "id_token")]
        public string IdToken { get; set; }

        [JsonProperty(PropertyName = "device_id")]
        public string DeviceId { get; set; }

        [JsonProperty(PropertyName = "client_id")]
        public string ClientId { get; set; }
    }
}
