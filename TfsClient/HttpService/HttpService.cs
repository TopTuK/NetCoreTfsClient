using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsClient.HttpService
{
    public static class HttpServiceFactory
    {
        public static IHttpService CreateHttpService(string baseUrl = null)
        {
            return new HttpService(baseUrl);
        }
    }

    internal class HttpService : IHttpService
    {
        private class RestHttpResponse : IHttpResponse
        {
            private IRestResponse _restResponse;

            public RestHttpResponse(IRestResponse restResponse)
            {
                _restResponse = restResponse;
            }

            public int StatusCode => (int)_restResponse.StatusCode;
            public bool IsSuccess => _restResponse.IsSuccessful;
            public bool HasError => !IsSuccess;
            public Uri RequestUrl => _restResponse.ResponseUri;
            public string ContentType => _restResponse.ContentType;

            public IReadOnlyDictionary<string, string> Headers => _restResponse
                .Headers
                .ToDictionary(param => param.Name, param => param.Value.ToString());

            public bool IsEmptyCookies => Cookies.Count == 0;
            public IReadOnlyDictionary<string, string> Cookies => _restResponse
                .Cookies
                .ToDictionary(cookie => cookie.Name, cookie => cookie.Value);

            public string Content => _restResponse.Content;
        }

        private readonly RestClient _restClient;
        public Uri BaseUrl 
        {
            get => _restClient.BaseUrl;
            set => _restClient.BaseUrl = value;
        }

        public HttpService(string baseUrl = null)
        {
            _restClient = (baseUrl == null) ? new RestClient() : new RestClient(baseUrl);
            _restClient.CookieContainer = new System.Net.CookieContainer();
        }

        public void Authentificate(string userName, string userPassword)
        {
            _restClient.Authenticator = new NtlmAuthenticator(userName, userPassword);
        }

        private IRestRequest MakeRequest(string resource,
            IDictionary<string, string> customParams = null,
            IDictionary<string, string> customHeaders = null)
        {
            var request = new RestRequest(resource);

            if (customParams != null)
            {
                foreach (var param in customParams)
                {
                    request.AddParameter(param.Key, param.Value);
                }
            }

            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            return request;
        }

        public IHttpResponse Get(string resource, 
            IDictionary<string, string> customParams = null, 
            IDictionary<string, string> customHeaders = null)
        {
            var request = MakeRequest(resource, customParams, customHeaders);
            var response = _restClient.Get(request);

            return new RestHttpResponse(response);
        }

        public async Task<IHttpResponse> GetAsync(string resource, 
            IDictionary<string, string> customParams = null, 
            IDictionary<string, string> customHeaders = null)
        {
            var request = MakeRequest(resource, customParams, customHeaders);
            request.Method = Method.GET;
            var response = await _restClient.ExecuteAsync(request);

            return new RestHttpResponse(response);
        }
    }
}
