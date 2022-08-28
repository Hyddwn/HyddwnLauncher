using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    ///     Represents the JSON body for the trusted device request
    /// </summary>
    public struct TrustDeviceNoAuthV1Request
    {
        /// <summary>
        /// The authentication type for this request
        /// 0 = ???
        /// 1 = authenticator
        /// </summary>
        [JsonProperty("authType")]
        public int AuthType { get; set; }

        /// <summary>
        /// Represents the email the code was sent to (typically the id)
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The verification code the user is using to verify the device
        /// </summary>
        [JsonProperty("verificationCode")]
        public string VerificationCode { get; set; }

        /// <summary>
        /// The device id being verified
        /// </summary>
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        /// <summary>
        /// Set whether the device should be remembered or verified for one time
        /// </summary>
        [JsonProperty("rememberMe")]
        public bool RememberMe { get; set; }
    }
}
