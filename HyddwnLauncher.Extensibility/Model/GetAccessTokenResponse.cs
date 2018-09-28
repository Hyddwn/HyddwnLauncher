using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    /// Object modeling the response from Nexon
    /// </summary>
    public class GetAccessTokenResponse
    {
        [JsonProperty("code")]
        public string Code { get; set;  }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
