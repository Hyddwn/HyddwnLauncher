using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Documents;
using Newtonsoft.Json;

namespace HyddwnLauncher.Network.Rest
{
    internal class RestResponse
    {
        protected readonly HttpResponseMessage HttpResponseMessage;

        public RestResponse(HttpResponseMessage httpResponseMessage)
        {
            this.HttpResponseMessage = httpResponseMessage;
        }

        public HttpStatusCode StatusCode => this.HttpResponseMessage.StatusCode;

        public bool HasError => !this.HttpResponseMessage.IsSuccessStatusCode;
        public bool IsSuccessful => this.HttpResponseMessage.IsSuccessStatusCode;

        public bool HasNoOrEmptyContent => this.HttpResponseMessage.StatusCode == HttpStatusCode.NoContent ||
                                           (this.HttpResponseMessage.Content.Headers.ContentLength ?? 0) == 0;

        public bool HasErrorWithData => this.HasError
            ? !this.HasNoOrEmptyContent
            : this.HasError;

        public bool IsSuccessfulWithData => this.IsSuccessful
            ? !this.HasNoOrEmptyContent
            : this.IsSuccessful;

        public string GetHeader(string name, string @default = null)
        {
            return this.HttpResponseMessage.Headers.GetValues(name).FirstOrDefault() ?? @default;
        }

        public List<KeyValuePair<string, string>> GetCookies()
        {
            return (from httpResponseHeader in this.HttpResponseMessage.Headers
                    .Where(header => header.Key == "Set-Cookie")
                from cookie in httpResponseHeader.Value
                let cookieDataSplit = cookie.Split(';')[0].Split('=')
                select cookieDataSplit.Length == 1
                    ? new KeyValuePair<string, string>(cookie, string.Empty)
                    : new KeyValuePair<string, string>(cookieDataSplit[0], cookieDataSplit[1])).ToList();
        }

        public async Task<string> GetContentAsync()
        {
            return await this.HttpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }

    internal class RestResponse<TData> : RestResponse
    {
        public RestResponse(HttpResponseMessage httpResponseMessage)
            : base(httpResponseMessage)
        {
        }

        public async Task<TData> GetDataObjectAsync()
        {
            var content = await this.HttpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TData>(content);
        }

        public static implicit operator TData(RestResponse<TData> response)
        {
            try
            {
                // ReSharper disable once AsyncConverter.AsyncWait
                return response.GetDataObjectAsync().Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
    }

    internal class RestResponse<TData, TError> : RestResponse<TData>
    {
        public RestResponse(HttpResponseMessage httpResponseMessage)
            : base(httpResponseMessage)
        {
        }

        public async Task<TError> GetDataErrorObjectAsync()
        {
            if (!this.HasNoOrEmptyContent) return default;

            var content = await this.HttpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TError>(content);
        }

        //public static implicit operator TData(RestResponse<TData, TError> response)
        //{
        //    try
        //    {
        //        // ReSharper disable once AsyncConverter.AsyncWait
        //        return response.GetDataObject().Result;
        //    }
        //    catch (AggregateException ex)
        //    {
        //        throw ex.InnerException;
        //    }
        //}

        public static implicit operator TError(RestResponse<TData, TError> response)
        {
            try
            {
                // ReSharper disable once AsyncConverter.AsyncWait
                return response.GetDataErrorObjectAsync().Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}