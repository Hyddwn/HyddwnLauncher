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
    public struct RegionalAccountLoginNoAuthV1Request
    {
        /// <summary>
        ///     Whether or not the user selected automatic login
        /// </summary>
        [JsonProperty("autoLogin")]
        public bool AutoLogin { get; set; }

        /// <summary>
        ///     The captcha token
        /// </summary>
        [JsonProperty("captchaToken")]
        public string CaptchaToken { get; set; }

        /// <summary>
        ///     The captcha version
        /// </summary>
        [JsonProperty("captchaVersion")]
        public string CaptchaVersion { get; set; }

        /// <summary>
        ///     The client id of the launcher?
        /// </summary>
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        /// <summary>
        ///     The derived device id
        /// </summary>
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        /// <summary>
        ///     The Nexon Id used to log in
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        ///     The UTC Time
        /// </summary>
        [JsonProperty("localTime")]
        public long LocalTime { get; set; }

        /// <summary>
        ///     The derived password
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        ///     The scope requested in order to perform account related actions
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>
        ///     The UTC Time Offset (Timezone)
        /// </summary>
        [JsonProperty("timeOffset")]
        public int TimeOffset { get; set; }
    }
}
