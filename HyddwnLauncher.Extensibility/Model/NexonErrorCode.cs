using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Extensibility.Model
{
    public static class NexonErrorCode
    {
        public const string LoginFailed = "LOGINFAILED";
        public const string DevError = "DEVERROR_HEHEHE";
        
       
        public const string InvalidParameter = "INVALID_PARAMETER";
        public const string AutoLoginUnauthorized = "10001";
        public const string InvalidLoginSession = "20001";
        public const string UserDoesNotExist = "20002";
        public const string DevieNameIsInvalid = "20203";
        public const string VerificationCodeInvalid = "20013";
        public const string TrustedDeviceRequired = "20027";
        public const string InvalidPassword = "20055";
        public const string AuthenticatorNotVerified = "20111";
        public const string InvalidAuthenticatorCode = "20188";
    }
}
