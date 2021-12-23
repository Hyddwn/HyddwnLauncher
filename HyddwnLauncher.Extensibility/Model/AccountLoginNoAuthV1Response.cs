using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    public struct AccountLoginNoAuthV1Response
    {
        /// <summary>
        ///     The user number
        /// </summary>
        [JsonProperty(PropertyName = "userNo")]
        public int UserNumber { get; set; }

        /// <summary>
        ///     The user number
        /// </summary>
        [JsonProperty(PropertyName = "globalUserNo")]
        public int GlobalUserNumber { get; set; }

        /// <summary>
        ///     The user number hashed
        /// </summary>
        [JsonProperty(PropertyName = "hashedUserNo")]
        public string HashedUserNumber { get; set; }

        /// <summary>
        ///     Determines if the user is verified?
        /// </summary>
        [JsonProperty(PropertyName = "isVerified")]
        public bool IsVerified { get; set; }

        /// <summary>
        ///     Gives the country code
        /// </summary>
        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }

        /// <summary>
        ///     When the login session expires
        /// </summary>
        [JsonProperty("loginSessionExpiresIn")]
        public int LoginSessionExpiresIn { get; set; }
    }
}
