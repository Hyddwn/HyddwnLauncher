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
        public GetAccessTokenResponse(ErrorResponse errorResponse, string code = "0")
        {
            Code = code;
            Description = errorResponse.Description;
            Message = errorResponse.Message;

            MapErrorMessage();
        }

        /// <summary>
        ///     The response code
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set;  }

        /// <summary>
        ///     The description (useless to us)
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        ///     The message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        ///     If login was successful
        /// </summary>
        public bool Success { get; set; }

        private void MapErrorMessage()
        {
            switch (Code)
            {
                case NexonErrorCode.UserDoesNotExist:
                    Message = "Username does not exist!";
                    break;
                case NexonErrorCode.InvalidPassword:
                    Message = "The password is incorrect.";
                    break;
                case NexonErrorCode.DevieNameIsInvalid:
                    Message = "Device name is empty, already in use, or invalid.";
                    break;
                case NexonErrorCode.BlockedUserPortalBan:
                    Message = "Login blocked: User banned. Please contact Nexon support to address this issue.";
                    break;
                case NexonErrorCode.BlockedUserSuspiciousIP:
                    Message = "Login blocked: Suspicious IP. Please log in via Nexon Launcher or https://nexon.net to address this issue.";
                    break;
                case NexonErrorCode.ProtectedUserNMode:
                    Message = "Protected User (S): Please log in via Nexon Launcher or https://nexon.net to address this issue.";
                    break;
                case NexonErrorCode.ProtectedUserSMode:
                    Message = "Protected User (N): Please log in via Nexon Launcher or https://nexon.net to address this issue.";
                    break;
                case NexonErrorCode.InvalidParameter:
                    if (Message.Contains("error.email")) 
                        Message = "Malformed email!";
                    break;
                default:
                    Message = "Undocumented error; Code: " + Code;
                    break;
            }
        }
    }
}
