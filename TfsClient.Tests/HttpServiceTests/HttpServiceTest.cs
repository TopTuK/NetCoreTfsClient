using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsClient.HttpService;
using Xunit;

namespace TfsClient.Tests.HttpServiceTests
{
    // https://stackoverflow.com/questions/5725430/http-test-server-accepting-get-post-requests
    public class HttpServiceTest
    {
        private const string _baseUrl = @"https://httpbin.org/";
        private const string _getUrl = @"https://httpbin.org/get";
        private const string _postUrl = @"https://httpbin.org/post";
        private const string _patchUrl = @"https://httpbin.org/patch";

        private readonly IHttpService _httpService = HttpServiceFactory.CreateHttpService();

        [Fact(DisplayName = "GET Returns simple success response")]
        public void HttpServiceSimpleGetSuccess()
        {
            // Arrange

            // Act
            var response = _httpService.Get(_getUrl);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.HasError);
            Assert.NotNull(response.Content);
        }

        [Fact(DisplayName = "GET Returns json response with args")]
        public void HttpServiceSimpleGetArgsSuccess()
        {
            // Arrange
            var args = new Dictionary<string, string>()
            {
                { "id", "1" },
                { "my_param", "my_value" }
            };

            // Act
            var response = _httpService.Get(_getUrl, args);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.HasError);
            Assert.NotNull(response.Content);

            var jsonResponse = JObject.Parse(response.Content);
            Assert.True(jsonResponse.ContainsKey("args"));
            Assert.Equal(1, jsonResponse["args"]["id"].Value<int>());
            Assert.Equal("my_value", jsonResponse["args"]["my_param"].Value<string>());
        }

        [Fact(DisplayName = "GET async Returns success response")]
        public async Task HttpServiceSimpleGetAsyncSuccess()
        {
            // Arrange

            // Act
            var response = await _httpService.GetAsync(_getUrl);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.HasError);
            Assert.NotNull(response.Content);
        }

        [Fact(DisplayName = "POST Returns simple success response")]
        public void HttpServiceSimplePostSuccess()
        {
            // Arrange

            // Act
            var response = _httpService.Post(_postUrl);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.HasError);
            Assert.NotNull(response.Content);
        }

        [Fact(DisplayName = "POST async Returns success response")]
        public async Task HttpServiceSimplePostAsyncSuccess()
        {
            // Arrange

            // Act
            var response = await _httpService.PostAsync(_postUrl);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.HasError);
            Assert.NotNull(response.Content);
        }

        [Fact(DisplayName = "POST Returns json response with args")]
        public void HttpServiceSimplePostArgsSuccess()
        {
            // Arrange
            var args = new Dictionary<string, string>()
            {
                { "id", "1" },
                { "my_param", "my_value" }
            };

            // Act
            var response = _httpService.Post(_postUrl, args);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.HasError);
            Assert.NotNull(response.Content);

            var jsonResponse = JObject.Parse(response.Content);
            Assert.True(jsonResponse.ContainsKey("form"));
            Assert.Equal(1, jsonResponse["form"]["id"].Value<int>());
            Assert.Equal("my_value", jsonResponse["form"]["my_param"].Value<string>());
        }

        [Fact(DisplayName = "POST JSON Returns json response with args")]
        public void HttpServiceSimplePostJsonSuccess()
        {
            // Arrange
            var json = new List<object>()
            {
                new { A = "AA", B = "BB"},
                new { C = true, D = 1 }
            };

            // Act
            var response = _httpService.PostJson(_postUrl, json);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.HasError);
            Assert.NotNull(response.Content);

            var jsonResponse = JObject.Parse(response.Content);
            Assert.True(jsonResponse.ContainsKey("json"));
            Assert.Equal(2, jsonResponse["json"].Count());
        }

        [Fact(DisplayName = "POST JSON async Returns json response with args")]
        public async Task HttpServiceSimplePostJsonAsyncSuccess()
        {
            // Arrange
            var json = new List<object>()
            {
                new { A = "AA", B = "BB"},
                new { C = true, D = 1 }
            };

            // Act
            var response = await _httpService.PostJsonAsync(_postUrl, json);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.HasError);
            Assert.NotNull(response.Content);

            var jsonResponse = JObject.Parse(response.Content);
            Assert.True(jsonResponse.ContainsKey("json"));
            Assert.Equal(2, jsonResponse["json"].Count());
        }

        [Fact(DisplayName = "PATCH Returns simple success response")]
        public void HttpServiceSimplePatchSuccess()
        {
            // Arrange
            var queryParams = new Dictionary<string, string>()
            {
                { "key", "value" },
                { "param", "1" }
            };

            // Act
            var response = _httpService.Patch(_patchUrl, customParams: queryParams);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.HasError);
            Assert.NotNull(response.Content);
        }

        [Fact(DisplayName = "PATCH async Returns success response")]
        public async Task HttpServiceSimplePatchAsyncSuccess()
        {
            // Arrange

            // Act
            var response = await _httpService.PatchAsync(_patchUrl);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.HasError);
            Assert.NotNull(response.Content);
        }

        [Fact(DisplayName = "AUTH Returns successful auth")]
        public void HttpServiceAuthSuccess()
        {
            // Arrange
            var userName = "user";
            var userPwd = "pwd";
            var url = $"{_baseUrl}/basic-auth/{userName}/{userPwd}";

            // Act
            _httpService.Authentificate(userName, userPwd);
            var response = _httpService.Get(url);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.HasError);
            Assert.NotNull(response.Content);
        }

        [Fact(DisplayName = "Auth bad Returns bad authorization")]
        public void HttpServiceAuthBad()
        {
            // Arrange
            var userName = "user";
            var userPwd = "pwd";
            var badUserPwd = "badpwd";
            var url = $"{_baseUrl}/basic-auth/{userName}/{userPwd}";

            // Act
            _httpService.Authentificate(userName, badUserPwd);
            var response = _httpService.Get(url);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.IsSuccess);
            Assert.True(response.HasError);
            Assert.NotNull(response.Content);
        }

        [Fact(DisplayName = "COOKIE set get Returns success if set and get cookies")]
        public void HttpServiceCookieSetGet()
        {
            // Arrange
            const string cookieName = "myCookie";
            const string cookieValue = "myCookieValue";

            var setUrl = $"{_baseUrl}/cookies/set/{cookieName}/{cookieValue}";
            var getUrl = $"{_baseUrl}/cookies";

            // Act
            _httpService.Get(setUrl);
            var response = _httpService.Get(getUrl);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.False(response.IsEmptyCookies);
            Assert.True(response.Cookies.ContainsKey(cookieName));
            Assert.Equal(cookieValue, response.Cookies[cookieName]);
        }
    }
}
