using System;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HyddwnLauncher.Network.Rest;
using Newtonsoft.Json;

namespace HyddwnLauncher.Util
{
    internal class ClientAuth
    {
        private static readonly SHA512Managed Sha512 = new SHA512Managed();
        private static readonly string BodyClientId = "7853644408";
        private static readonly string BodyScope = "us.launcher.all";

        public static readonly string LoginFailed = "LOGINFAILED";
        public static readonly string DevError = "DEVERROR_HEHEHE";

        private RestClient _restClient;

        internal async Task<string> GetNxAuthHash(string username, string password)
        {
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

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return LoginFailed;

            var data = await response.GetContent();
            var body = JsonConvert.DeserializeObject<dynamic>(data);
            string token = body["access_token"];

            _restClient = new RestClient(new Uri("https://api.nexon.io"), token);

            request = _restClient.Create("/users/me/passport");

            response = await request.ExecuteGet<string>();

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return DevError;

            //TODO: Error checking yo
            data = await response.GetContent();

            body = JsonConvert.DeserializeObject<dynamic>(data);

            return body["passport"];
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

        public void HashPassword(ref string password)
        {
            // ToLower is required here, otherwise the password is incorrect.
            password = BitConverter.ToString(Sha512.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "")
                .ToLower();
        }

        private struct AccountLoginJson
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "password")]
            public string Password { get; set; }

            [JsonProperty(PropertyName = "auto_login")]
            public bool AutoLogin { get; set; }

            [JsonProperty(PropertyName = "client_id")]
            public string ClientId { get; set; }

            [JsonProperty(PropertyName = "scope")]
            public string Scope { get; set; }

            [JsonProperty(PropertyName = "device_id")]
            public string DeviceId { get; set; }
        }
    }
}