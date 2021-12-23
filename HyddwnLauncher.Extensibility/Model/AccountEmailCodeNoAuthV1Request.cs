using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    /// Represent an auto-login request
    /// </summary>
    public struct AccountEmailCodeNoAuthV1Request
    {
        /// <summary>
        /// The device id of the requesting system
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("language")] 
        public string Language => "en_US";
    }
}
