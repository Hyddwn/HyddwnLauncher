using System;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Network.Rest;
using Newtonsoft.Json;

namespace HyddwnLauncher.Network
{
    public class NexonApi : INexonApi
    {
        private static readonly SHA512Managed Sha512 = new SHA512Managed();
        private static readonly string BodyClientId = "7853644408";
        private static readonly string BodyScope = "us.launcher.all";

        public static readonly string LoginFailed = "LOGINFAILED";
        public static readonly string DevError = "DEVERROR_HEHEHE";

        public static readonly NexonApi Instance = new NexonApi();

        //Tokens
        private string _accessToken;

        //Token Expiry Timer
        private int _accessTokenExpiration;
        private DispatcherTimer _accessTokenExpiryTimer;
        private bool _accessTokenIsExpired;

        private string _lastAuthenticationProfileGuid;

        private RestClient _restClient;

        public bool IsAccessTokenValid(string guid)
        {
            return _accessToken != null && !_accessTokenIsExpired &&
                   (_accessToken != LoginFailed || _accessToken != DevError) && guid == _lastAuthenticationProfileGuid;
        }

        public async Task<string> GetNxAuthHash()
        {
            if (!IsAccessTokenValid(_lastAuthenticationProfileGuid))
                return _accessToken;

            _restClient = new RestClient(new Uri("https://api.nexon.io"), _accessToken);

            var request = _restClient.Create("/users/me/passport");

            var response = await request.ExecuteGet<string>();

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return DevError;

            //TODO: Error checking yo
            var data = await response.GetContent();

            var body = JsonConvert.DeserializeObject<dynamic>(data);

            return body["passport"];
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

            //TODO: Error checking yo
            var data = await response.GetContent();

            var body = JsonConvert.DeserializeObject<dynamic>(data);

            var manifestUrl = body["product_details"]["manifestUrl"].Value;

            var versionSearch = "([\\d]*R)";

            string match = Regex.Match(manifestUrl, versionSearch).Value;

            int version;

            int.TryParse(match.Replace('R', ' '), out version);

            return version;
        }

        public async Task<bool> GetAccessToken(string username, string password, string profileGuid)
        {
           if (_accessToken != null && !_accessTokenIsExpired && _lastAuthenticationProfileGuid == profileGuid)
                return true;

            _restClient = new RestClient(new Uri("https://accounts.nexon.net"), null);

            var request = _restClient.Create("/account/login/launcher");

            var initialRequestBody = new AccountLoginJson
            {
                Id = username,
                Password = password,
                AutoLogin = false,
                ClientId = BodyClientId,
                Scope = BodyScope,
                DeviceId = GetDeviceUuid()
            };

            request.SetBody(initialRequestBody);

            RestResponse response = await request.ExecutePost<string>();

            // dispose of password yo
            password = null;
            initialRequestBody = new AccountLoginJson();
            // Compiler tricks to ensure it isn't optimized away
            var ps = password;

            var data = "";

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                data = await response.GetContent();
                return false;
            }


            data = await response.GetContent();
            var body = JsonConvert.DeserializeObject<dynamic>(data);
            _accessToken = body["access_token"];
            _accessTokenExpiration = body["access_token_expires_in"];

            _lastAuthenticationProfileGuid = profileGuid;

            _accessTokenIsExpired = false;
            StartAccessTokenExpiryTimer(_accessTokenExpiration);

            return true;
        }

        public void HashPassword(ref string password)
        {
            // ToLower is required here, otherwise the password is incorrect.
            password = BitConverter.ToString(Sha512.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "")
                .ToLower();
        }

        private void StartAccessTokenExpiryTimer(int timeout = 7200)
        {
            _accessTokenExpiryTimer?.Stop();

            _accessTokenExpiryTimer = new DispatcherTimer();
            _accessTokenExpiryTimer.Interval = TimeSpan.FromSeconds(timeout);
            _accessTokenExpiryTimer.Tick += (sender, args) => _accessTokenIsExpired = true;
        }

        /// <summary>
        ///     Gets Device UUID
        /// </summary>
        /// <remarks>Confimed this gets the same UUID as 'wmic csproduct get uuid' </remarks>
        /// <returns></returns>
        //TODO: Make this a little better.
        private string GetDeviceUuid()
        {
            var Scope = new ManagementScope($@"\\{Environment.MachineName}\root\CIMV2", null);
            Scope.Connect();
            var Query = new ObjectQuery("SELECT UUID FROM Win32_ComputerSystemProduct");
            var Searcher = new ManagementObjectSearcher(Scope, Query);

            var uuid = "";

            foreach (ManagementObject wmiObject in Searcher.Get())
            {
                uuid = wmiObject["UUID"].ToString();
                break;
            }

            return uuid;
        }

        private struct AccountLoginJson
        {
            [JsonProperty(PropertyName = "id")] public string Id { get; set; }

            [JsonProperty(PropertyName = "password")]
            public string Password { get; set; }

            [JsonProperty(PropertyName = "auto_login")]
            public bool AutoLogin { get; set; }

            [JsonProperty(PropertyName = "client_id")]
            public string ClientId { get; set; }

            [JsonProperty(PropertyName = "scope")] public string Scope { get; set; }

            [JsonProperty(PropertyName = "device_id")]
            public string DeviceId { get; set; }
        }
    }
}