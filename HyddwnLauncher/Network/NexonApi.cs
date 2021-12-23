using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.RightsManagement;
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
        private string _gaccessToken;
        private string _hashedUserId;
        private string _idToken;
        private string _recaptchaToken;
        private string _lastLoginUsername;

        // Session id
        private string _areanSessionId;
        private long _arenaSessionTimeStamp;
        private string _sessionId;
        private int _apiCallTraceSequence;

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

        public void InitializeApiSession()
        {
            _apiCallTraceSequence = 0;
            _sessionId = Guid.NewGuid().ToString().ToLower().Replace("-", "");
        }

        public async Task<dynamic> GetMabinogiMetadata()
        {
            if (_accessToken == null || _accessTokenIsExpired)
                throw new Exception("Invalid or expired access token!");

            _restClient = new RestClient(new Uri("https://www.nexon.com"), _accessToken);
            var request = _restClient.Create("/api/game-build/v1/configuration/games/10200");
            request.AddCookie("AToken", _accessToken);
            request.AddCookie("g_AToken", _gaccessToken);
            request.AddCookie("NxLSession", _idToken);
            request.AddCookie("NexonUserID", _hashedUserId);
            var restResponse = await request.ExecuteGet<string>();
            if (restResponse.StatusCode == HttpStatusCode.BadRequest) return default(dynamic);
            var body = await restResponse.GetContentAsync();

            return JsonConvert.DeserializeObject<dynamic>(body);
        }

        public async Task<bool> GetMaintenanceStatus()
        {
            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);
            var request = _restClient.Create("/api/maintenance/v1/products/10200");
            request.AddQueryString("lang", "en");
            request.AddCookie("AToken", _accessToken);
            request.AddCookie("g_AToken", _gaccessToken);
            request.AddCookie("NxLSession", _idToken);
            request.AddCookie("NexonUserID", _hashedUserId);
            var response = await request.ExecuteGet<string>();

            return response.StatusCode != HttpStatusCode.NotFound;
        }

        public async Task<GameBuildConfigurationV1Response> GetLaunchConfig()
        {
            if (_accessToken == null || _accessTokenIsExpired)
                throw new Exception("Invalid or expired access token!");

            _restClient = new RestClient(new Uri("https://www.nexon.com"), _accessToken);
            var request = _restClient.Create("/api/game-build/v1/configuration/games/10200");
            request.AddCookie("AToken", _accessToken);
            request.AddCookie("g_AToken", _gaccessToken);
            request.AddCookie("NxLSession", _idToken);
            request.AddCookie("NexonUserID", _hashedUserId);
            var restResponse = await request.ExecuteGet<string>();
            if (restResponse.StatusCode == HttpStatusCode.BadRequest) return default(GameBuildConfigurationV1Response);
            var body = await restResponse.GetContentAsync();

            try
            {
                var obj = JsonConvert.DeserializeObject<GameBuildConfigurationV1Response>(body);
                return obj;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to acquire launch config data.");
                return default(GameBuildConfigurationV1Response);
            }
        }

        public async Task<GetManifestResponse> GetManifestUrl()
        {
            if (_accessToken == null || _accessTokenIsExpired)
                throw new Exception("Invalid or expired access token!");

            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);
            var request = _restClient.Create("/api/game-build/v1/branch/games/10200/public");
            request.AddCookie("AToken", _accessToken);
            request.AddCookie("g_AToken", _gaccessToken);
            request.AddCookie("NxLSession", _idToken);
            request.AddCookie("NexonUserID", _hashedUserId);

            var restResponse = await request.ExecuteGet<string>();
            if (restResponse.StatusCode == HttpStatusCode.BadRequest) return null;
            var body = await restResponse.GetContentAsync();

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
            var manifestUrl = details.ManifestUrl.Replace("http://download2.nexon.net", "");
            _restClient = new RestClient(new Uri("http://download2.nexon.net"), null);
            var request = _restClient.Create($"{manifestUrl}");
            var response = await request.ExecuteGet<string>();
            var manifestHashString = await response.GetContentAsync();
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

            var success = await CheckPlayable();
            if (!success)
                return NexonErrorCode.DevError;

            _restClient = new RestClient(new Uri("https://www.nexon.com"), _accessToken);

            var request = _restClient.Create("/api/passport/v2/passport");

            request.AddCookie("AToken", _accessToken);
            request.AddCookie("g_AToken", _gaccessToken);
            request.AddCookie("NxLSession", _idToken);
            request.AddCookie("NexonUserID", _hashedUserId);

            var requestBody2 = new PassportV2Request
            {
                ProductId = "10200"
            };

            request.SetBody(requestBody2);

            var response2 = await request.ExecutePost<GetPassportV1Response>();

            if (response2.StatusCode == HttpStatusCode.BadRequest)
                return NexonErrorCode.DevError;

            var obj = await response2.GetDataObjectAsync();

            return obj.Passport;
        }

        public async Task<UserProfileResponse> GetUserProfile()
        {
            if (_accessToken == null || _accessTokenIsExpired)
                throw new Exception("Invalid or expired access token!");

            var sessionMetadata = GetSessionMetadata();
            _restClient = new RestClient(new Uri("https://api.nexon.io"), _accessToken, sessionId: sessionMetadata.sessionId, apiTraceRequestSequence: sessionMetadata.apiCallTraceSequence);

            var request = _restClient.Create("/users/me/profile");

            var response = await request.ExecuteGet<string>();

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            var data = await response.GetContentAsync();

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
            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/api/game-auth2/v1/access");

            var requestBody = new GameAuth2AccessV1Request
            {
                ProductId = "10200"
            };

            request.SetBody(requestBody);

            request.AddCookie("AToken", _accessToken);
            request.AddCookie("g_AToken", _gaccessToken);
            request.AddCookie("NxLSession", _idToken);
            request.AddCookie("NexonUserID", _hashedUserId);

            var response = await request.ExecutePost<GameAuth2AccessV1Response>();

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return 0;

            var details = await GetManifestUrl();
            var manifestUrl = details.ManifestUrl.Replace("https://download2.nexon.net", "");
            var versionSearch = "([\\d]*R)";
            if (manifestUrl == null)
                return -1;

            string match = Regex.Match(manifestUrl, versionSearch).Value;
            int.TryParse(match.Replace('R', ' '), out var version);

            return version;
        }

        public async Task<bool> CheckPlayable()
        {
            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/api/game-auth2/v1/playable");

            var requestBody = new GameAuth2CheckPlayableV1Request
            {
                ProductId = "10200"
            };

            request.SetBody(requestBody);

            request.AddCookie("AToken", _accessToken);
            request.AddCookie("g_AToken", _gaccessToken);
            request.AddCookie("NxLSession", _idToken);
            request.AddCookie("NexonUserID", _hashedUserId);

            var response = await request.ExecutePost<GameAuth2CheckPlayableV1Response>();

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return false;

            return true;
        }

        public async Task<GetAccessTokenResponse> GetAccessTokenWithIdTokenOrPassword(string username, string password, IClientProfile clientProfile, bool rememberMe, bool enableTagging)
        {
            var currentDate = DateTime.Now;

            if (string.IsNullOrWhiteSpace(clientProfile.LastIdToken) ||  currentDate.Subtract(clientProfile.LastRefreshTime) > TimeSpan.FromSeconds(clientProfile.TokenExpirationTimeFrame) || !rememberMe)
            {
                Log.Info("Using username and password");
                return await GetAccessToken(username, password, clientProfile, rememberMe, enableTagging, true);
            }

            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            // https://www.nexon.com/api/account/v1/login/launcher/autologin
            var request = _restClient.Create("/api/account/v1/no-auth/login/launcher/autologin");

            var deviceId = GetDeviceUuid(enableTagging ? username : "");

            var initialRequestBody = new AccountAutoLoginNoAuthV1Request
            {
                DeviceId = deviceId
            };

            request.SetBody(initialRequestBody)
                .AddCookie("NxLSession", clientProfile.LastIdToken);

            var response = await request.ExecutePost<AccountLoginNoAuthV1Response, string>();

            var data = "";

            if (response.StatusCode != HttpStatusCode.OK)
            {
                data = await response.GetContentAsync();
                var error = JsonConvert.DeserializeObject<ErrorResponse>(data);
                Log.Info("Refresh failed, using username and password: \r\nError: {0}\r\nMessage: {1}", error.Code, error.Message);
                return await GetAccessToken(username, password, clientProfile, true, enableTagging, true);
            }

            // dispose of password yo
            password = null;
            initialRequestBody = new AccountAutoLoginNoAuthV1Request();
            // Compiler tricks to ensure it isn't optimized away
            var ps = password;

            data = await response.GetContentAsync();
            var cookies = response.GetCookies();
            var body = JsonConvert.DeserializeObject<AccountLoginNoAuthV1Response>(data);
            _accessToken = cookies.FirstOrDefault(x => x.Key == "AToken").Value;
            _gaccessToken = cookies.FirstOrDefault(x => x.Key == "g_AToken").Value;
            _idToken = cookies.FirstOrDefault(x => x.Key == "NxLSession").Value;
            _idTokenExpiration = body.LoginSessionExpiresIn;
            _hashedUserId = body.HashedUserNumber;

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

        public async Task<GetAccessTokenResponse> GetAccessToken(string username, string password, IClientProfile clientProfile, bool rememberMe, bool enableTagging, bool isFirstLogin = false)
        {
            if (_accessToken != null && !_accessTokenIsExpired && _lastAuthenticationProfileGuid == clientProfile.Guid)
                return new GetAccessTokenResponse {Success = true};

            _recaptchaToken = App.IsAdministrator() && LauncherContext.Instance.LauncherSettingsManager.LauncherSettings.EnableCaptchaBypass 
                ? await WebServer.Instance.Run() ?? CreateString(256) : CreateString(256);

            var deviceId = GetDeviceUuid(enableTagging ? username : "");

            if (isFirstLogin)
                await InitializeArenaSession(username, deviceId);

            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/api/regional-auth/v1.0/no-auth/launcher/email/login");

            var initialRequestBody = new RegionalAccountLoginNoAuthV1Request
            {
                AutoLogin = rememberMe,
                CaptchaToken = _recaptchaToken,
                CaptchaVersion = "v3",
                ClientId = BodyClientId,
                DeviceId = deviceId,
                Id = username,
                Password = password,
                Scope = BodyScope,
                LocalTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                TimeOffset = Math.Abs((int)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes)
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
            initialRequestBody = new RegionalAccountLoginNoAuthV1Request();
            // Compiler tricks to ensure it isn't optimized away
            var ps = password;

            var data = "";

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                data = await response.GetContentAsync();
                var responseObject = JsonConvert.DeserializeObject<ErrorResponse>(data);
                var code = response.GetHeader("x-arena-web-errorcode", "0");

                var accessTokenResponse = new GetAccessTokenResponse(responseObject, code);

                accessTokenResponse.Success = false;

                return accessTokenResponse;
            }

            data = await response.GetContentAsync();
            var cookies = response.GetCookies();
            var body = JsonConvert.DeserializeObject<AccountLoginNoAuthV1Response>(data);
            _accessToken = cookies.FirstOrDefault(x => x.Key == "AToken").Value;
            _gaccessToken = cookies.FirstOrDefault(x => x.Key == "g_AToken").Value;
            _idToken = cookies.FirstOrDefault(x => x.Key == "NxLSession").Value;
            _idTokenExpiration = body.LoginSessionExpiresIn;
            _hashedUserId = body.HashedUserNumber;

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

        public async Task<bool> PutAuthTrustDevice(string name, string deviceId)
        {
            // https://www.nexon.com/api/account/v1/trusted-device
            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/api/account/v1/trusted-device");

            var requestBody = new TrustDeviceV1Request
            {
                Name = name,
                DeviceId = deviceId
            };

            request.SetBody(requestBody);

            request.AddCookie("AToken", _accessToken);
            request.AddCookie("g_AToken", _gaccessToken);
            request.AddCookie("NxLSession", _idToken);
            request.AddCookie("NexonUserID", _hashedUserId);

            var response = await request.ExecutePut<string>();

            if (response.StatusCode != HttpStatusCode.BadRequest)
                return true;

            var data = await response.GetContentAsync();
            var responseObject = JsonConvert.DeserializeObject<ErrorResponse>(data);
            Log.Info("Failed to save device. Code: {0} | Description: {1}", response.GetHeader("x-arena-web-errorcode", "0"),
                responseObject.Description);

            return false;
        }

        public async Task<bool> PostRequestEmailCode(string email)
        {
            // https://www.nexon.com/api/account/v1/no-auth/mfa/email/request-code
            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/api/account/v1/no-auth/mfa/email/request-code");

            var requestBody = new AccountEmailCodeNoAuthV1Request
            {
                Email = email
            };

            request.SetBody(requestBody);

            request.AddCookie("AToken", _accessToken);
            request.AddCookie("g_AToken", _gaccessToken);
            request.AddCookie("NxLSession", _idToken);
            request.AddCookie("NexonUserID", _hashedUserId);

            var response = await request.ExecutePost<string>();

            if (response.StatusCode != HttpStatusCode.BadRequest)
                return true;

            var data = await response.GetContentAsync();
            var responseObject = JsonConvert.DeserializeObject<ErrorResponse>(data);
            Log.Info("Failed to send code email. Code: {0} | Description: {1}", response.GetHeader("x-arena-web-errorcode", "0"),
                responseObject.Description);

            return false;
        }

        public async Task<bool> PutVerifyDevice(string email, string code, string deviceId, bool rememberDevice, string name, AuthyType authType)
        {
            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/api/account/v1/no-auth/trusted-device/verify");

            var requestBody = new TrustDeviceNoAuthV1Request
            {
                AuthType = (int)authType,
                Email = email,
                VerificationCode = code,
                DeviceId = deviceId,
                RememberMe = rememberDevice,
                Name = name
            };

            request.SetBody(requestBody);

            var response = await request.ExecutePut<string>();

            if (response.StatusCode != HttpStatusCode.BadRequest)
            {
                //if (rememberDevice)
                //    await PutAuthTrustDevice(name, deviceId);

                return true;
            }

            var data = await response.GetContentAsync();
            var responseObject = JsonConvert.DeserializeObject<ErrorResponse>(data);
            Log.Info("Failed code verification. Code: {0} | Description: {1}", response.GetHeader("x-arena-web-errorcode", "0"),
                responseObject.Description);


            return false;
        }

        // TODO: Actually refresh
        private void StartAccessTokenExpiryTimer(int timeout = 7140)
        {
            _accessTokenExpiryTimer?.Stop();

            _accessTokenExpiryTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(timeout)};
            _accessTokenExpiryTimer.Tick += (sender, args) => _accessTokenIsExpired = true;
        }

        // TODO: Actually refresh
        private void StartIdTokenExpiryTimer(int timeout = 1209540)
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

        public (string sessionId, int apiCallTraceSequence) GetSessionMetadata()
        {
            // Just in case...
            if (_sessionId == null)
                InitializeApiSession();

            return (_sessionId, ++_apiCallTraceSequence);
        }

        internal async Task InitializeArenaSession(string username, string deviceId)
        {
            _restClient = new RestClient(new Uri("https://www.nexon.com"), null);

            var request = _restClient.Create("/api/regional-auth/v1.0/no-auth/login/validate");

            var validateRequest = new RegionalLoginValidateV1Request
            {
                Id = username,
                DeviceId = deviceId
            };

            request.SetBody(validateRequest);

            var response = await request.ExecutePost<string>();
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