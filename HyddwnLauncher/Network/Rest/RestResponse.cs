using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyddwnLauncher.Network.Rest
{
    internal class RestResponse
    {
        protected readonly HttpResponseMessage HttpResponseMessage;

        public RestResponse(HttpResponseMessage httpResponseMessage)
        {
            HttpResponseMessage = httpResponseMessage;
        }

        public HttpStatusCode StatusCode => HttpResponseMessage.StatusCode;

        public string GetHeader(string name, string @default = null)
        {
            return HttpResponseMessage.Headers.GetValues(name).FirstOrDefault() ?? @default;
        }

        public async Task<string> GetContent()
        {
            return await HttpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }

    internal class RestResponse<T> : RestResponse
    {
        public RestResponse(HttpResponseMessage httpResponseMessage)
            : base(httpResponseMessage)
        {
        }

        public async Task<T> GetDataObject()
        {
            var content = await HttpResponseMessage.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);
        }

        public static implicit operator T(RestResponse<T> response)
        {
            try
            {
                return response.GetDataObject().Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}