using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public struct RegionalLoginValidateV1Request
    {
        /// <summary>
        ///     The Nexon Id used to log in
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        ///     The derived device id
        /// </summary>
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }
    }
}
