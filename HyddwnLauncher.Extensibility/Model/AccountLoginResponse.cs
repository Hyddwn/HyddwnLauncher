using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    ///     Represents the response from <see cref="AccountLoginRequest"/>
    /// </summary>
    public struct AccountLoginResponse
    {
        /// <summary>
        ///     The Id Token
        /// </summary>
        [JsonProperty(PropertyName = "id_token")]
        public string IdToken { get; set; }

        /// <summary>
        ///     The Access Token
        /// </summary>
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        ///     The user number
        /// </summary>
        [JsonProperty(PropertyName = "user_no")]
        public string UserNumber { get; set; }

        /// <summary>
        ///     The user number hashed
        /// </summary>
        [JsonProperty(PropertyName = "hashed_user_no")]
        public string HashedUserNumber { get; set; }

        /// <summary>
        ///     The expiration time frame for the id token
        /// </summary>
        [JsonProperty(PropertyName = "id_token_expires_in")]
        public int IdTokenExpiresIn { get; set; }

        /// <summary>
        ///     The expiration time frame for the access token
        /// </summary>
        [JsonProperty(PropertyName = "access_token_expires_in")]
        public int AccessTokenExpiresIn { get; set; }

        /// <summary>
        ///     Determines if the user is verified?
        /// </summary>
        [JsonProperty(PropertyName = "is_verified")]
        public bool IsVerified { get; set; }

        /// <summary>
        ///     Gives the country code
        /// </summary>
        [JsonProperty(PropertyName = "country_code")]
        public string CountryCode { get; set; }
    }
}
