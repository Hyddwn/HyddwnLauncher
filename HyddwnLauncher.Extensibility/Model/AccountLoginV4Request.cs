using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    /// Represents the data sent when attempting to log in 
    /// </summary>
    public struct AccountLoginV4Request
    {
        /// <summary>
        ///     Whether or not the user selected automatic login
        /// </summary>
        [JsonProperty("auto_login")]
        public bool AutoLogin { get; set; }

        /// <summary>
        ///     The captcha token
        /// </summary>
        [JsonProperty("captcha_token")]
        public string CaptchaToken { get; set; }

        /// <summary>
        ///     The captcha version
        /// </summary>
        [JsonProperty("captcha_version")]
        public string CaptchaVersion { get; set; }

        /// <summary>
        ///     The client id of the launcher?
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        /// <summary>
        ///     The derived device id
        /// </summary>
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }

        /// <summary>
        ///     The Nexon Id used to log in
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        ///     The UTC Time
        /// </summary>
        [JsonProperty("local_time")]
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
        [JsonProperty("time_offset")]
        public int TimeOffset { get; set; } 
    }
}
