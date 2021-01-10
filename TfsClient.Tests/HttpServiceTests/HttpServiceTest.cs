using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TfsClient.HttpService;
using Xunit;

namespace TfsClient.Tests.HttpServiceTests
{
    public class HttpServiceTest
    {
        private const string _baseUrl = @"https://httpbin.org/";
        private const string _getUrl = @"https://httpbin.org/get";

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

        [Fact(DisplayName = "GET async Returns success response")]
        public async Task HttpServiceSimpleGetAsyncSuccess()
        {
            // Arranget

            // Act
            var response = await _httpService.GetAsync(_getUrl);

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
