using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Newtonsoft.Json;

namespace HyddwnLauncher.Network.Rest
{
    internal class RestRequest
    {
        private readonly string _endpoint;
        private readonly RestClient _restClient;

        private string _accessToken;

        private object _bodyObj;

        private List<KeyValuePair<string, string>> _queryString;
        private List<KeyValuePair<string, string>> _urlSegment;
        private List<KeyValuePair<string, string>> _cookies;
        private List<KeyValuePair<string, string>> _headers;
        private List<KeyValuePair<string, string>> _formData;

        public RestRequest(RestClient restClient, string endpoint, string accessToken)
        {
            _restClient = restClient;
            _endpoint = endpoint;
            _accessToken = accessToken;
        }

        private void AppendCookie(StringBuilder stringBuilder, string name, string value)
        {
            if (stringBuilder.Length > 0)
                stringBuilder.Append("; ");

            stringBuilder.Append(WebUtility.UrlEncode(name));
            stringBuilder.Append("=");
            stringBuilder.Append(WebUtility.UrlEncode(value));
        }

        private void AppendCookie(StringBuilder stringBuilder, KeyValuePair<string, string> value)
        {
            AppendCookie(stringBuilder, value.Key, value.Value);
        }

        private void AppendQueryString(StringBuilder stringBuilder, string key, string value)
        {
            if (stringBuilder.Length > 0)
                stringBuilder.Append("&");

            stringBuilder.Append(key);
            stringBuilder.Append("=");
            stringBuilder.Append(WebUtility.UrlEncode(value));
        }

        private void AppendQueryString(StringBuilder stringBuilder, KeyValuePair<string, string> value)
        {
            AppendQueryString(stringBuilder, value.Key, value.Value);
        }

        /// <exception cref="ArgumentOutOfRangeException">Enumerater value not defined.</exception>
        public RestRequest AddParameter(string key, string value, ParameterType parameterType)
        {
            switch (parameterType)
            {
                case ParameterType.QueryString:
                    return AddQueryString(key, value);
                case ParameterType.UrlSegment:
                    return AddUrlSegment(key, value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
            }
        }

        public RestRequest AddParameter(KeyValuePair<string, string> keyValuePair,
            ParameterType parameterType = ParameterType.QueryString)
        {
            AddParameter(keyValuePair.Key, keyValuePair.Value, parameterType);

            return this;
        }

        public RestRequest AddUrlSegment(string key, string value)
        {
            if (_urlSegment == null)
                _urlSegment = new List<KeyValuePair<string, string>>();

            _urlSegment.Add(new KeyValuePair<string, string>(key, value));

            return this;
        }

        public RestRequest AddQueryString(string key, string value)
        {
            if (_queryString == null)
                _queryString = new List<KeyValuePair<string, string>>();

            _queryString.Add(new KeyValuePair<string, string>(key, value));

            return this;
        }

        public RestRequest AddCookie(string name, string value)
        {
            if (_cookies == null)
                _cookies = new List<KeyValuePair<string, string>>();

            _cookies.Add(new KeyValuePair<string, string>(name, value));

            return this;
        }

        public RestRequest AddHeader(string name, string value)
        {
            if (_headers == null)
                _headers = new List<KeyValuePair<string, string>>();

            _headers.Add(new KeyValuePair<string, string>(name, value));

            return this;
        }

        public RestRequest AddForm(string name, string value)
        {
            if (_formData == null)
                _formData = new List<KeyValuePair<string, string>>();

            _formData.Add(new KeyValuePair<string, string>(name, value));

            return this;
        }

        public RestRequest SetBody(object obj)
        {
            _bodyObj = obj;

            return this;
        }

        private HttpRequestMessage PrepRequest(HttpMethod httpMethod)
        {
            try
            {
                var queryStringBuilder = new StringBuilder();

                if (_queryString != null)
                    foreach (var keyValuePair in _queryString)
                        AppendQueryString(queryStringBuilder, keyValuePair);

                foreach (var keyValuePair in _restClient.DefaultQueryString)
                    AppendQueryString(queryStringBuilder, keyValuePair);

                var endpoint = _endpoint;
                if (_urlSegment != null)
                    endpoint = _urlSegment.Aggregate(endpoint,
                        (current, keyValuePair) => current.Replace("{" + keyValuePair.Key + "}", keyValuePair.Value));

                var uriBuilder = new UriBuilder(new Uri(_restClient.BaseUrl, endpoint));
                uriBuilder.Query = queryStringBuilder.ToString();

                var httpRequestMessage = new HttpRequestMessage(httpMethod, uriBuilder.Uri);

                httpRequestMessage.Headers.UserAgent.ParseAdd("NexonLauncher.nxl-release-18.14.10-220-fc7480c-coreapp-3.3.0");
                if (!string.IsNullOrWhiteSpace(_accessToken))
                {
                    httpRequestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse(
                        $"Bearer {(_restClient.RequiresBase64Encode ? Convert.ToBase64String(Encoding.UTF8.GetBytes(_accessToken)) : _accessToken)}");
                }

                if (!string.IsNullOrWhiteSpace(_accessToken))
                {
                    httpRequestMessage.Headers.Add("X-Amzn-Trace-Id", $"NxL={_restClient.SessionId}.{_restClient.ApiTraceRequestSequence}");
                }

                if (_cookies != null)
                {
                    var cookieStringBuilder = new StringBuilder();

                    foreach (var keyValuePair in _cookies)
                        AppendCookie(cookieStringBuilder, keyValuePair);

                    httpRequestMessage.Headers.Add("Cookie", cookieStringBuilder.ToString());
                }

                if (httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put)
                {
                    if (_bodyObj != null)
                    {
                        var json = JsonConvert.SerializeObject(_bodyObj);

                        httpRequestMessage.Content = new StringContent(json);
                        httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        httpRequestMessage.Headers.Add("Accept", "*/*");
                    }
                    else if (_formData != null)
                    {
                        httpRequestMessage.Content = new FormUrlEncodedContent(_formData);
                        httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                        httpRequestMessage.Headers.Add("Accept", "*/*");
                    }
                }

                return httpRequestMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public void CheckResponse(HttpResponseMessage httpResponseMessage)
        {
            // if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            //     throw new UnauthorizedAccessException(
            //         Properties.Resources.NexonAPIUnauthorized);
        }

        public async Task<RestResponse<T>> ExecuteGetAsync<T>()
        {
            var httpResponseMessage = await SendInternalAsync(HttpMethod.Get).ConfigureAwait(false);

            // CheckResponse(httpResponseMessage);

            return new RestResponse<T>(httpResponseMessage);
        }

        public async Task<RestResponse<TData, TError>> ExecuteGetAsync<TData, TError>()
        {
            var httpResponseMessage = await SendInternalAsync(HttpMethod.Get).ConfigureAwait(false);

            // CheckResponse(httpResponseMessage);

            return new RestResponse<TData, TError>(httpResponseMessage);
        }

        public async Task<RestResponse> ExecutePostAsync()
        {
            var httpResponseMessage = await SendInternalAsync(HttpMethod.Post).ConfigureAwait(false);

            // CheckResponse(httpResponseMessage);

            return new RestResponse(httpResponseMessage);
        }

        public async Task<RestResponse<T>> ExecutePostAsync<T>()
        {
            var httpResponseMessage = await SendInternalAsync(HttpMethod.Post).ConfigureAwait(false);

            // CheckResponse(httpResponseMessage);

            return new RestResponse<T>(httpResponseMessage);
        }

        public async Task<RestResponse<TData, TError>> ExecutePostAsync<TData, TError>()
        {
            var httpResponseMessage = await SendInternalAsync(HttpMethod.Post).ConfigureAwait(false);

            // CheckResponse(httpResponseMessage);

            return new RestResponse<TData, TError>(httpResponseMessage);
        }

        public async Task<RestResponse> ExecuteDeleteAsync()
        {
            var httpResponseMessage = await SendInternalAsync(HttpMethod.Delete).ConfigureAwait(false);

            // CheckResponse(httpResponseMessage);

            return new RestResponse(httpResponseMessage);
        }

        public async Task<RestResponse<T>> ExecuteDeleteAsync<T>()
        {
            var httpResponseMessage = await SendInternalAsync(HttpMethod.Delete).ConfigureAwait(false);

            // CheckResponse(httpResponseMessage);

            return new RestResponse<T>(httpResponseMessage);
        }

        public async Task<RestResponse<TData, TError>> ExecuteDeleteAsync<TData, TError>()
        {
            var httpResponseMessage = await SendInternalAsync(HttpMethod.Delete).ConfigureAwait(false);

            // CheckResponse(httpResponseMessage);

            return new RestResponse<TData, TError>(httpResponseMessage);
        }

        public async Task<RestResponse> ExecutePutAsync()
        {
            var httpResponseMessage = await SendInternalAsync(HttpMethod.Put).ConfigureAwait(false);

            // CheckResponse(httpResponseMessage);

            return new RestResponse(httpResponseMessage);
        }

        public async Task<RestResponse<T>> ExecutePutAsync<T>()
        {
            var httpResponseMessage = await SendInternalAsync(HttpMethod.Put).ConfigureAwait(false);

            // CheckResponse(httpResponseMessage);

            return new RestResponse<T>(httpResponseMessage);
        }

        public async Task<RestResponse<TData, TError>> ExecutePutAsync<TData, TError>()
        {
            var httpResponseMessage = await SendInternalAsync(HttpMethod.Put).ConfigureAwait(false);

            // CheckResponse(httpResponseMessage);

            return new RestResponse<TData, TError>(httpResponseMessage);
        }

        private async Task<HttpResponseMessage> SendInternalAsync(HttpMethod method)
        {
            // Account for the following settings:
            // - MaxRetryCount                          Max times to retry
            // DEPRECATED RetryWaitTimeInSeconds        Time to wait between retries
            // DEPRECATED ThrowErrorOnExeedingMaxCalls  Throw an exception if we hit a ratelimit

            var timesToTry = _restClient.MaxRetryCount + 1;

            Debug.Assert(timesToTry >= 1);

            HttpResponseMessage httpResponseMessage;

            do
            {
                var httpRequest = PrepRequest(method);
                httpResponseMessage = await new HttpClient(new HttpClientHandler { UseCookies = false }).SendAsync(httpRequest).ConfigureAwait(false);

                if (httpResponseMessage.StatusCode == (HttpStatusCode)429)
                {
                    // The previous result was a ratelimit, read the Retry-After header and wait the allotted time
                    if (httpResponseMessage.Headers.RetryAfter?.Delta != null)
                    {
                        var retryAfter = httpResponseMessage.Headers.RetryAfter?.Delta.Value;

                        if (retryAfter.Value.TotalSeconds > 0)
                            await Task.Delay(retryAfter.Value).ConfigureAwait(false);
                        else
                            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                    }

                    continue;
                }

                return httpResponseMessage;
            } while (timesToTry-- > 0);

            return httpResponseMessage;
        }
    }
}