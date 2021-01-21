using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using TfsClient.HttpService;

namespace TfsClient.Tests.TfsClientTests.HttpServiceMock
{
    public static class HttpServiceMockFactory
    {
        private static readonly string _singleItemResponseContent = @"
        {
            ""count"": 1,
            ""value"": [
            {
                ""id"": 100500,
                ""rev"": 1,
                ""fields"": {
                    ""System.Id"": 100500,
                    ""System.WorkItemType"": ""Requirement"",
                    ""System.Title"": ""Tfs test workitem - Requirement"",
                    ""System.Description"": ""Description of test workitem""
                },
                ""url"": ""https://tfs-tfs/tfs/_apis/wit/workItems/100500""
            }]
        }";

        private static readonly string _multipyItemsResponseContent = @"
        {
            ""count"": 2,
            ""value"": [
            {
                ""id"": 100501,
                ""rev"": 1,
                ""fields"": {
                    ""System.Id"": 100501,
                    ""System.WorkItemType"": ""Requirement"",
                    ""System.Title"": ""Tfs test workitem - Requirement"",
                    ""System.Description"": ""Description of test workitem""
                },
                ""url"": ""https://tfs-tfs/tfs/_apis/wit/workItems/100500""
            },
            {
                ""id"": 100502,
                ""rev"": 1,
                ""fields"": {
                    ""System.Id"": 100502,
                    ""System.WorkItemType"": ""Requirement"",
                    ""System.Title"": ""Tfs test workitem - Requirement"",
                    ""System.Description"": ""Description of test workitem""
                },
                ""url"": ""https://tfs-tfs/tfs/_apis/wit/workItems/100500""
            }
            ]
        }";

        private static readonly string _manyItemsResponseContent = @"";

        private static readonly string _itemNotFoundResponseContent = @"";

        private static readonly IHttpResponse _singleItemResponse = Mock.Of<IHttpResponse>(resp =>
            resp.HasError == false &&
            resp.Headers == null &&
            resp.IsEmptyCookies == true &&
            resp.IsSuccess == true &&
            resp.RequestUrl == null &&
            resp.StatusCode == 200 &&
            resp.Cookies == null &&
            resp.ContentType == "" &&
            resp.Content == _singleItemResponseContent
        );

        private static readonly IHttpResponse _multipyItemsResponse = Mock.Of<IHttpResponse>(resp =>
            resp.HasError == false &&
            resp.Headers == null &&
            resp.IsEmptyCookies == true &&
            resp.IsSuccess == true &&
            resp.RequestUrl == null &&
            resp.StatusCode == 200 &&
            resp.Cookies == null &&
            resp.ContentType == "" &&
            resp.Content == _multipyItemsResponseContent
        );

        private static readonly IHttpResponse _manyItemsResponse = Mock.Of<IHttpResponse>(resp =>
            resp.HasError == false &&
            resp.Headers == null &&
            resp.IsEmptyCookies == true &&
            resp.IsSuccess == true &&
            resp.RequestUrl == null &&
            resp.StatusCode == 200 &&
            resp.Cookies == null &&
            resp.ContentType == "" &&
            resp.Content == _manyItemsResponseContent
        );

        private static readonly IHttpResponse _itemNotFoundResponse = Mock.Of<IHttpResponse>(resp =>
            resp.HasError == false &&
            resp.Headers == null &&
            resp.IsEmptyCookies == true &&
            resp.IsSuccess == true &&
            resp.RequestUrl == null &&
            resp.StatusCode == 200 &&
            resp.Cookies == null &&
            resp.ContentType == "" &&
            resp.Content == _itemNotFoundResponseContent
        );

        public static Mock<IHttpService> CreateSingleItemHttpServiceMock()
        {
            var httpServiceMock = new Mock<IHttpService>();

            httpServiceMock
                .Setup(mock => mock.Get(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>()
                 ))
                .Returns(_singleItemResponse);

            return httpServiceMock;
        }

        public static Mock<IHttpService> CreateMultipyItemHttpServiceMock()
        {
            var httpServiceMock = new Mock<IHttpService>();

            httpServiceMock
                .Setup(mock => mock.Get(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>()
                 ))
                .Returns(_multipyItemsResponse);

            return httpServiceMock;
        }
    }
}
