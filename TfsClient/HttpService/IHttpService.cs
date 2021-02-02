using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TfsClient.HttpService
{
    public interface IHttpResponse
    {
        int StatusCode { get; }
        bool IsSuccess { get; }
        bool HasError { get; }

        Uri RequestUrl { get; }
        IReadOnlyDictionary<string, string> Headers { get; }
        string ContentType { get; }
        bool IsEmptyCookies { get; }
        IReadOnlyDictionary<string,string> Cookies { get; }
        string Content { get; } 
    }

    public interface IHttpService
    {
        Uri BaseUrl { get; set; }

        void Authentificate(string userName, string userPassword);

        IHttpResponse Get(string resource,
            IReadOnlyDictionary<string, string> customParams = null,
            IReadOnlyDictionary<string, string> customHeaders = null);
        Task<IHttpResponse> GetAsync(string resource,
            IReadOnlyDictionary<string, string> customParams = null,
            IReadOnlyDictionary<string, string> customHeaders = null);

        IHttpResponse Post(string resource,
            IReadOnlyDictionary<string, string> customParams = null,
            IReadOnlyDictionary<string, string> customHeaders = null);
        Task<IHttpResponse> PostAsync(string resource,
            IReadOnlyDictionary<string, string> customParams = null,
            IReadOnlyDictionary<string, string> customHeaders = null);
        IHttpResponse PostJson(string resource,
            object requestBody,
            IReadOnlyDictionary<string, string> customParams = null,
            IReadOnlyDictionary<string, string> customHeaders = null);
        Task<IHttpResponse> PostJsonAsync(string resource,
            object requestBody,
            IReadOnlyDictionary<string, string> customParams = null,
            IReadOnlyDictionary<string, string> customHeaders = null);

        IHttpResponse Patch(string resource,
            IReadOnlyDictionary<string, string> customParams = null,
            IReadOnlyDictionary<string, string> customHeaders = null);
        Task<IHttpResponse> PatchAsync(string resource,
            IReadOnlyDictionary<string, string> customParams = null,
            IReadOnlyDictionary<string, string> customHeaders = null);
        IHttpResponse PatchJson(string resource,
            object requestBody,
            IReadOnlyDictionary<string, string> customParams = null,
            IReadOnlyDictionary<string, string> customHeaders = null);
        Task<IHttpResponse> PatchJsonAsync(string resource,
            object requestBody,
            IReadOnlyDictionary<string, string> customParams = null,
            IReadOnlyDictionary<string, string> customHeaders = null);
    }
}
