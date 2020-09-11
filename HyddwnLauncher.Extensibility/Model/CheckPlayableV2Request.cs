using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    ///     Represents the object used to make a request to check playability
    /// </summary>
    public struct CheckPlayableV2Request
    {
        /// <summary>
        ///     The device id associated with <see cref="IdToken"/>
        /// </summary>
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }

        /// <summary>
        ///     The token for the account
        /// </summary>
        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        /// <summary>
        ///     The product id for the game being checked
        /// </summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; }
    }
}
