using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    /// Represents the data sent when attempting to log in 
    /// </summary>
    public struct AccountLoginRequest
    {
        /// <summary>
        /// Whether or not the user selected automatic login
        /// </summary>
        [JsonProperty(PropertyName = "auto_login")]
        public bool AutoLogin { get; set; }

        /// <summary>
        /// The client id of the launcher?
        /// </summary>
        [JsonProperty(PropertyName = "client_id")]
        public string ClientId { get; set; }

        /// <summary>
        /// The derived device id
        /// </summary>
        [JsonProperty(PropertyName = "device_id")]
        public string DeviceId { get; set; }

        /// <summary>
        /// The Nexon Id used to log in
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// The derived password
        /// </summary>
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        /// <summary>
        /// The scope requested in order to perform account related actions
        /// </summary>
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }
    }
}
