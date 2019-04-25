using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    /// Represents the GetNxAuthHash Response
    /// </summary>
    public class NexonPassport
    {
        /// <summary>
        /// The user number
        /// </summary>
        [JsonProperty("user_no")]
        public string UserNumber { get; set; }

        /// <summary>
        /// The passport hash
        /// </summary>
        [JsonProperty("passport")]
        public string Passport { get; set; }

        /// <summary>
        /// The authentication token
        /// </summary>
        [JsonProperty("auth_token")]
        public string AuthToken { get; set; }
    }
}
