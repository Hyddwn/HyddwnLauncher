using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Extensibility.Model
{
    /// <summary>
    /// Object modeling the response from Nexon
    /// </summary>
    public class GetAccessTokenResponse
    {
        /// <summary>
        ///     Creates an empty <see cref="GetAccessTokenResponse"/>
        /// </summary>
        public GetAccessTokenResponse()
        {
            
        }


        /// <summary>
        ///     Creates an <see cref="GetAccessTokenResponse"/> which accepts a <see cref="ErrorResponse"/>
        /// </summary>
        public GetAccessTokenResponse(ErrorResponse errorResponse)
        {
            Code = errorResponse.Code;
            Description = errorResponse.Description;
            Message = errorResponse.Message;
        }

        /// <summary>
        /// The response code
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set;  }

        /// <summary>
        /// The description (useless to us)
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// If login was successful
        /// </summary>
        public bool Success { get; set; }
    }
}
