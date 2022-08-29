using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    /// Represents an sms code request
    /// </summary>
    public struct AccountSmsCodeNoAuthV1Request
    {
        /// <summary>
        ///     The user's email address
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
