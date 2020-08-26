using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Threading;
using HyddwnLauncher.Core;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Extensibility.Model;
using HyddwnLauncher.Http;
using HyddwnLauncher.Network.Rest;
using HyddwnLauncher.Util;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace HyddwnLauncher.Network
{
    public class NexonApi : INexonApi
    {
        private static readonly SHA512Managed Sha512 = new SHA512Managed();
        private static readonly SHA256Managed Sha256 = new SHA256Managed();
        private static readonly Random Random = new Random();
        private static readonly string BodyClientId = "7853644408";
        private static readonly string BodyScope = "us.launcher.all";

        public static readonly NexonApi Instance = new NexonApi();

        //Tokens
        private string _accessToken;
        private string _idToken;
        private string _recaptchaToken;
        private string _lastLoginUsername;

        //Token Expiry Timer
        private int _accessTokenExpiration;
        private DispatcherTimer _accessTokenExpiryTimer;
        private bool _accessTokenIsExpired;
        private int _idTokenExpiration;
        private DispatcherTimer _idTokenExpiryTimer;
        private bool _idTokenIsExpired;

        public string LastLoginUsername => _lastLoginUsername;

        private string _lastAuthenticationProfileGuid;

        private RestClient _restClient;

        public async Task<dynamic> GetMabinogiMetadata()
        {
            if (_accessToken == null || _accessTokenIsExpired)
                throw new Exception("Invalid or expired access token!");
            _restClient = new RestClient(new Uri("https://api.nexon.io"), _accessToken);
            var restResponse = await _restClient.Create("/products/10200").ExecuteGet<string>();
            if (restResponse.StatusCode == HttpStatusCode.BadRequest) return default(dynamic);
            var body = await restResponse.GetContent();

            return JsonConvert.DeserializeObject<dynamic>(body);
        }

        public async Task<bool> GetMaintenanceStatus()
        {
            _restClient = new RestClient(new Uri("https://api.nexon.io"), null);
            var restResponse = await _restClient.Create("/maintenance")
                .AddQueryString("product_id", "10200")
                .AddQueryString("lang", "en")
                .ExecuteGet<string>();

            var body = await restResponse.GetContent();

            return !string.IsNullOrWhiteSpace(body);
        }

        public async Task<LauncherConfigResponseV2> GetLaunchConfig()
        {
            if (_accessToken == null || _accessTokenIsExpired)
                throw new Exception("Invalid or expired access token!");

            _restClient = new RestClient(new Uri("https://api.nexon.io"), _accessToken);
            var restResponse = await _restClient.Create("/game-info/v2/games/10200").ExecuteGet<string>();
            if (restResponse.StatusCode == HttpStatusCode.BadRequest) return null;
            var body = await restResponse.GetContent();

            try
            {
                var obj = JsonConvert.DeserializeObject<LauncherConfigResponseV2>(body);
                return obj;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to acquire launch config data.");
                return null;
            }
        }

        public async Task<GetManifestResponse> GetManifestUrl()
        {
            if (_accessToken == null || _accessTokenIsExpired)
                throw new Exception("Invalid or expired access token!");

            _restClient = new RestClient(new Uri("https://api.nexon.io"), _accessToken);
            var restResponse = await _restClient.Create("/game-info/v2/games/10200/branch/public").ExecuteGet<string>();
            if (restResponse.StatusCode == HttpStatusCode.BadRequest) return null;
            var body = await restResponse.GetContent();

            try
            {
                var obj = JsonConvert.DeserializeObject<GetManifestResponse>(body);
                return obj;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to acquire launch config data.");
                return null;
            }
        }

        public async Task<string> GetManifestHashString()
        {
            var details = await GetManifestUrl();
            var manifestUrl = details.ManifestUrl.Replace("https://download2.nexon.net", "");
            _restClient = new RestClient(new Uri("https://download2.nexon.net"), null);
            var request = _restClient.Create($"{manifestUrl}");
            var response = await request.ExecuteGet<string>();
            var manifestHashString = await response.GetContent();
            return manifestHashString;
        }

        public bool IsAccessTokenValid(string guid)
        {
            return _accessToken != null && !_accessTokenIsExpired &&
                   (_accessToken != NexonErrorCode.LoginFailed || _accessToken != NexonErrorCode.DevError) && guid == _lastAuthenticationProfileGuid;
        }

        public async Task<string> GetNxAuthHash(string username)
        {
            if (!IsAccessTokenValid(_lastAuthenticationProfileGuid))
                return _accessToken;

            _restClient = new RestClient(new Uri("https://api.nexon.io"), null);

            var request = _restClient.Create("/game-auth/v2/check-playable");

            var requestBody = new CheckPlayableV2Request
            {
                DeviceId = GetDeviceUuid(username),
                IdToken = _idToken,
                ProductId = "10200"
            };

            request.SetBody(requestBody);

            var response = await request.ExecutePost<CheckPlayableV2Response>();

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return NexonErrorCode.DevError;

            _restClient = new RestClient(new Uri("https://api.nexon.io"), _accessToken, false);

            request = _restClient.Create("/passport/v1/passport");

            var requestBody2 = new PassportV1Request
            {
                ProductId = "10200"
            };

            request.SetBody(requestBody2);

            var response2 = await request.ExecutePost<GetPassportV1Response>();

            if (response2.StatusCode == HttpStatusCode.BadRequest)
                return NexonErrorCode.DevError;

            var obj = await response2.GetDataObject();

            return obj.Passport;
        }

        public async Task<UserProfileResponse> GetUserProfile()
        {
            if (_accessToken == null || _accessTokenIsExpired)
                throw new Exception("Invalid or expired access token!");

            _restClient = new RestClient(new Uri("https://api.nexon.io"), _accessToken);

            var request = _restClient.Create("/users/me/profile");

            var response = await request.ExecuteGet<string>();

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            var data = await response.GetContent();

            try
            {
                var obj = JsonConvert.DeserializeObject<UserProfileResponse>(data);
                return obj;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to acquire user profile data.");
                return null;
            }
        }

        public async Task<int> GetLatestVersion()
        {
            var details = await GetManifestUrl();
            var manifestUrl = details.ManifestUrl.Replace("https://download2.nexon.net", "");
            var versionSearch = "([\\d]*R)";
            if (manifestUrl == null)
                return -1;

            string match = Regex.Match(manifestUrl, versionSearch).Value;
            int.TryParse(match.Replace('R', ' '), out var version);

            return version;
        }

        public async Task<GetAccessTokenResponse> GetAccessTokenWithIdTokenOrPassword(string username, string password, IClientProfile clientProfile, bool rememberMe, bool enableTagging)
        {
            var currentDate = DateTime.Now;

            if (string.IsNullOrWhiteSpace(clientProfile.LastIdToken) ||  currentDate.Subtract(clientProfile.LastRefreshTime) > TimeSpan.FromSeconds(clientProfile.TokenExpirationTimeFrame) || !rememberMe)
            {
                Log.Info("Using username and password");
                return await GetAccessToken(username, password, clientProfile, rememberMe, enableTagging);
            }

            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/account-webapi/login/launcher");

            var deviceId = GetDeviceUuid(enableTagging ? username : "");

            var initialRequestBody = new IdTokenRefreshRequest
            {
                IdToken = clientProfile.LastIdToken,
                ClientId = BodyClientId,
                DeviceId = deviceId
            };

            request.SetBody(initialRequestBody);

            RestResponse response = null;

            response = await request.ExecutePost();

            var data = "";

            if (response.StatusCode != HttpStatusCode.OK)
            {
                data = await response.GetContent();
                var error = JsonConvert.DeserializeObject<ErrorResponse>(data);
                Log.Info("Refresh failed, using username and password: \r\nError: {0}\r\nMessage: {1}", error.Code, error.Message);
                return await GetAccessToken(username, password, clientProfile, true, enableTagging);
            }

            // dispose of password yo
            password = null;
            initialRequestBody = new IdTokenRefreshRequest();
            // Compiler tricks to ensure it isn't optimized away
            var ps = password;

            data = await response.GetContent();
            var body = JsonConvert.DeserializeObject<AccountLoginResponse>(data);
            _accessToken = body.AccessToken;
            _accessTokenExpiration = body.AccessTokenExpiresIn;
            _idToken = body.IdToken;
            _idTokenExpiration = body.IdTokenExpiresIn;

            ((ClientProfile)clientProfile).LastIdToken = _idToken;
            ((ClientProfile)clientProfile).TokenExpirationTimeFrame = _idTokenExpiration;
            ((ClientProfile)clientProfile).LastRefreshTime = DateTime.Now;
            ((ClientProfile)clientProfile).AutoLogin = true;

            _lastAuthenticationProfileGuid = clientProfile.Guid;

            _accessTokenIsExpired = false;
            _idTokenIsExpired = false;
            StartAccessTokenExpiryTimer(_accessTokenExpiration);
            StartIdTokenExpiryTimer(_idTokenExpiration);

            _lastLoginUsername = username;

            return new GetAccessTokenResponse { Success = true };
        }

        public async Task<GetAccessTokenResponse> GetAccessToken(string username, string password, IClientProfile clientProfile, bool rememberMe, bool enableTagging)
        {
            if (_accessToken != null && !_accessTokenIsExpired && _lastAuthenticationProfileGuid == clientProfile.Guid)
                return new GetAccessTokenResponse {Success = true};

            _recaptchaToken = App.IsAdministrator() && LauncherContext.Instance.LauncherSettingsManager.LauncherSettings.EnableCaptchaBypass 
                ? await WebServer.Instance.Run() ?? CreateString(256) : CreateString(256);

            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/account-webapi/login/launcher");

            var deviceId = GetDeviceUuid(enableTagging ? username : "");

            var initialRequestBody = new AccountLoginRequest
            {
                AutoLogin = rememberMe,
                CaptchaToken = _recaptchaToken,
                CaptchaVersion = "v3",
                ClientId = BodyClientId,
                DeviceId = deviceId,
                Id = username,
                Password = password,
                Scope = BodyScope
            };

            request.SetBody(initialRequestBody);

            RestResponse response = null;

            try
            {
                if (App.IsAdministrator() && LauncherContext.Instance.LauncherSettingsManager.LauncherSettings.EnableCaptchaBypass)
                    WebServer.Instance.Stop();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failure during WebServer.Stop");
            }

            response = await request.ExecutePost();

            // dispose of password yo
            password = null;
            initialRequestBody = new AccountLoginRequest();
            // Compiler tricks to ensure it isn't optimized away
            var ps = password;

            var data = "";

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                data = await response.GetContent();
                var responseObject = JsonConvert.DeserializeObject<ErrorResponse>(data);
                var rsp = new GetAccessTokenResponse(responseObject);
                Log.Info("Login Error: {0} Message: {1}", rsp.Code, rsp.Message);
                rsp.Success = false;

                if (rsp.Code == NexonErrorCode.UserDoesNotExist)
                    rsp.Message = "Username does not exist!";
                if (rsp.Code == NexonErrorCode.InvalidParameter && rsp.Message.Contains("error.email"))
                    rsp.Message = "Malformed email!";

                return rsp;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var responseObject = new GetAccessTokenResponse();
                responseObject.Success = false;
                responseObject.Description = "Username does not exist!";
                responseObject.Code = "NOTFOUND";
                responseObject.Message = responseObject.Description;
                return responseObject;
            }

            data = await response.GetContent();
            var body = JsonConvert.DeserializeObject<AccountLoginResponse>(data);
            _accessToken = body.AccessToken;
            _accessTokenExpiration = body.AccessTokenExpiresIn;
            _idToken = body.IdToken;
            _idTokenExpiration = body.IdTokenExpiresIn;

            if (!rememberMe)
            {
                // Unset to be safe because I seem to be adding more bugs than fixes....
                ((ClientProfile) clientProfile).LastIdToken = "";
                ((ClientProfile) clientProfile).TokenExpirationTimeFrame = 0;
                ((ClientProfile) clientProfile).LastRefreshTime = DateTime.MinValue;
            }
            else
            {
                ((ClientProfile) clientProfile).LastIdToken = _idToken;
                ((ClientProfile) clientProfile).TokenExpirationTimeFrame = _idTokenExpiration;
                ((ClientProfile) clientProfile).LastRefreshTime = DateTime.Now;
            }

            ((ClientProfile)clientProfile).AutoLogin = rememberMe;

            _lastAuthenticationProfileGuid = clientProfile.Guid;

            _accessTokenIsExpired = false;
            _idTokenIsExpired = false;
            StartAccessTokenExpiryTimer(_accessTokenExpiration);
            StartIdTokenExpiryTimer(_idTokenExpiration);

            _lastLoginUsername = username;

            return new GetAccessTokenResponse {Success = true};
        }

        public void HashPassword(ref string password)
        {
            // ToLower is required here, otherwise the password is incorrect.
            password = BitConverter.ToString(Sha512.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "")
                .ToLower();
        }

        public async Task<bool> PutVerifyDevice(string email, string code, string deviceId, bool rememberDevice)
        {
            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/account-webapi/trusted_devices");

            var requestBody = new TrustDeviceRequest
            {
                Email = email,
                VerificationCode = code,
                DeviceId = deviceId,
                RememberMe = rememberDevice
            };

            request.SetBody(requestBody);

            var response = await request.ExecutePut<string>();

            if (response.StatusCode != HttpStatusCode.BadRequest)
                return true;

            var data = await response.GetContent();
            var responseObject = JsonConvert.DeserializeObject<ErrorResponse>(data);
            Log.Info("Failed code verification. Code: {0} | Description: {1}", responseObject.Code,
                responseObject.Description);


            return false;
        }

        private void StartAccessTokenExpiryTimer(int timeout = 7200)
        {
            _accessTokenExpiryTimer?.Stop();

            _accessTokenExpiryTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(timeout)};
            _accessTokenExpiryTimer.Tick += (sender, args) => _accessTokenIsExpired = true;
        }

        private void StartIdTokenExpiryTimer(int timeout = 1209600)
        {
            _idTokenExpiryTimer?.Stop();

            _idTokenExpiryTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(timeout) };
            _idTokenExpiryTimer.Tick += (sender, args) => _idTokenIsExpired = true;
        }

        /// <summary>
        ///     Gets Device ID
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceUuid(string tag = "")
        {
            var deviceId = "";

            try
            {
                var scope = new ManagementScope($@"\\{Environment.MachineName}\root\CIMV2", null);
                scope.Connect();
                var query = new ObjectQuery("SELECT UUID FROM Win32_ComputerSystemProduct");
                var searcher = new ManagementObjectSearcher(scope, query);

                foreach (var o in searcher.Get())
                {
                    var wmiObject = (ManagementObject) o;
                    deviceId += wmiObject["UUID"].ToString();
                    break;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex, Properties.Resources.FailedToAcquireWMIC);
            }

            try
            {
                using (var key = Environment.Is64BitOperatingSystem
                    ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                    : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    var regKey = key.OpenSubKey("SOFTWARE\\Microsoft\\Cryptography");

                    if (regKey != null)
                    {
                        deviceId += regKey.GetValue("MachineGuid").ToString();
                        regKey.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex, Properties.Resources.FailedToAcquireMachineGUID);
            }

            if (string.IsNullOrWhiteSpace(deviceId)) return null;
            if (!string.IsNullOrWhiteSpace(tag)) deviceId += tag;
            deviceId = BitConverter.ToString(Sha256.ComputeHash(Encoding.UTF8.GetBytes(deviceId))).Replace("-", "")
                .ToLower();

            return deviceId;
        }

        
        internal static string CreateString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789_-";
            var chars = new char[stringLength];

            for (var i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[Random.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
    }
}