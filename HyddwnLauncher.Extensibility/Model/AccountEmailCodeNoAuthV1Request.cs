﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    /// Represent an email code request
    /// </summary>
    public struct AccountEmailCodeNoAuthV1Request
    {
        /// <summary>
        ///     The user's email address
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        ///     The language to send the email in
        /// </summary>
        /// <remarks>
        ///     Currently hard coded to en_US
        /// </remarks>
        [JsonProperty("language")] 
        public string Language => "en_US";
    }
}
