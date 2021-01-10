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
            IDictionary<string, string> customParams = null,
            IDictionary<string, string> customHeaders = null);
        Task<IHttpResponse> GetAsync(string resource,
            IDictionary<string, string> customParams = null,
            IDictionary<string, string> customHeaders = null);
    }
}
