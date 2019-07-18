using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private static readonly string BodyClientId = "7853644408";
        private static readonly string BodyScope = "us.launcher.all";

        public static readonly string LoginFailed = "LOGINFAILED";
        public static readonly string DevError = "DEVERROR_HEHEHE";
        public static readonly string TrustedDeviceRequired = "TRUST_DEVICE_REQUIRED";

        public static readonly NexonApi Instance = new NexonApi();

        //Tokens
        private string _accessToken;
        private string _idToken;
        private string _recaptcaToken;

        //Token Expiry Timer
        private int _accessTokenExpiration;
        private DispatcherTimer _accessTokenExpiryTimer;
        private bool _accessTokenIsExpired;
        private int _idTokenExpiration;
        private DispatcherTimer _idTokenExpiryTimer;
        private bool _idTokenIsExpired;



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

            return restResponse.StatusCode != HttpStatusCode.OK;
        }

        public async Task<LauncherConfigResponse> GetLaunchConfig()
        {
            if (_accessToken == null || _accessTokenIsExpired)
                throw new Exception("Invalid or expired access token!");

            _restClient = new RestClient(new Uri("https://api.nexon.io"), _accessToken);
            var restResponse = await _restClient.Create("/game-info/v1/games/10200").ExecuteGet<string>();
            if (restResponse.StatusCode == HttpStatusCode.BadRequest) return null;
            var body = await restResponse.GetContent();

            try
            {
                var obj = JsonConvert.DeserializeObject<LauncherConfigResponse>(body);
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
            var restResponse = await _restClient.Create("/game-info/v1/games/10200/branch/public").ExecuteGet<string>();
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
                   (_accessToken != LoginFailed || _accessToken != DevError) && guid == _lastAuthenticationProfileGuid;
        }

        public async Task<string> GetNxAuthHash()
        {
            if (!IsAccessTokenValid(_lastAuthenticationProfileGuid))
                return _accessToken;

            _restClient = new RestClient(new Uri("https://api.nexon.io"), null);

            var request = _restClient.Create("/game-auth/v2/check-playable");

            var requestBody = new CheckPlayableV2Request
            {
                DeviceId = GetDeviceUuid(),
                IdToken = _idToken,
                ProductId = "10200"
            };

            request.SetBody(requestBody);

            var response = await request.ExecutePost<CheckPlayableV2Response>();

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return DevError;

            _restClient = new RestClient(new Uri("https://api.nexon.io"), _accessToken, false);

            request = _restClient.Create("/passport/v1/passport");

            var requestBody2 = new PassportV1Request
            {
                ProductId = "10200"
            };

            request.SetBody(requestBody2);

            var response2 = await request.ExecutePost<GetPassportV1Response>();

            if (response2.StatusCode == HttpStatusCode.BadRequest)
                return DevError;

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
            if (_accessToken == null || _accessTokenIsExpired)
                throw new Exception("Invalid or expired access token!");

            _restClient = new RestClient(new Uri("https://api.nexon.io"), _accessToken);

            var request = _restClient.Create("/products/10200");

            var response = await request.ExecuteGet<string>();

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return -1;

            //TODO: Detect responses for real.
            var data = await response.GetContent();

            if (string.IsNullOrWhiteSpace(data))
                return -1;

            var body = JsonConvert.DeserializeObject<dynamic>(data);

            var manifestUrl = body["product_details"]["manifestUrl"].Value;

            var versionSearch = "([\\d]*R)";

            if (manifestUrl == null)
                return -1;

            string match = Regex.Match(manifestUrl, versionSearch).Value;

            int.TryParse(match.Replace('R', ' '), out var version);

            return version;
        }

        public async Task<GetAccessTokenResponse> GetAccessTokenWithIdTokenOrPassword(string username, string password, IClientProfile clientProfile)
        {
            var currentDate = DateTime.Now;

            if (string.IsNullOrWhiteSpace(clientProfile.LastIdToken) ||  currentDate.Subtract(clientProfile.LastRefreshTime) > TimeSpan.FromSeconds(clientProfile.TokenExpirationTimeFrame))
            {
                Log.Info("Using username and password");
                return await GetAccessToken(username, password, clientProfile);
            }

            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/account-webapi/login/launcher");

            var deviceId = GetDeviceUuid();

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
                return await GetAccessToken(username, password, clientProfile);
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

            _lastAuthenticationProfileGuid = clientProfile.Guid;

            _accessTokenIsExpired = false;
            _idTokenIsExpired = false;
            StartAccessTokenExpiryTimer(_accessTokenExpiration);

            return new GetAccessTokenResponse { Success = true };
        }

        public async Task<GetAccessTokenResponse> GetAccessToken(string username, string password, IClientProfile clientProfile)
        {
            if (_accessToken != null && !_accessTokenIsExpired && _lastAuthenticationProfileGuid == clientProfile.Guid)
                return new GetAccessTokenResponse {Success = true};

            WebServer.Instance.Completed += s => _recaptcaToken = s;
            WebServer.Instance.Run();

            Process.Start("http://nexon.com");

            while (_recaptcaToken == null)
                await Task.Delay(100);

            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/account-webapi/login/launcher");

            var deviceId = GetDeviceUuid();

            var initialRequestBody = new AccountLoginRequest
            {
                AutoLogin = true,
                CaptchaToken = _recaptcaToken,
                CaptchaVersion = "v3",
                ClientId = BodyClientId,
                DeviceId = deviceId,
                Id = username,
                Password = password,
                Scope = BodyScope
            };

            request.SetBody(initialRequestBody);

            RestResponse response = null;

            WebServer.Instance.Stop();

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

            ((ClientProfile)clientProfile).LastIdToken = _idToken;
            ((ClientProfile)clientProfile).TokenExpirationTimeFrame = _accessTokenExpiration;
            ((ClientProfile)clientProfile).LastRefreshTime = DateTime.Now;

            _lastAuthenticationProfileGuid = clientProfile.Guid;

            _accessTokenIsExpired = false;
            _idTokenIsExpired = false;
            StartAccessTokenExpiryTimer(_accessTokenExpiration);

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

            return response.StatusCode != HttpStatusCode.BadRequest;
        }

        private void StartAccessTokenExpiryTimer(int timeout = 7200)
        {
            _accessTokenExpiryTimer?.Stop();

            _accessTokenExpiryTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(timeout)};
            _accessTokenExpiryTimer.Tick += (sender, args) => _accessTokenIsExpired = true;
        }

        /// <summary>
        ///     Gets Device ID
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceUuid()
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
                Log.Exception(ex, "Failed to acquire data from WMIC");
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
                Log.Exception(ex, "Failed to acquire data from Machine GUID");
            }

            if (string.IsNullOrWhiteSpace(deviceId)) return null;
            deviceId = BitConverter.ToString(Sha256.ComputeHash(Encoding.UTF8.GetBytes(deviceId))).Replace("-", "")
                .ToLower();

            return deviceId;
        }

        private static readonly Random Rnd = new Random();
        internal static string CreateString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789_-";
            var chars = new char[stringLength];

            for (var i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[Rnd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
    }
}