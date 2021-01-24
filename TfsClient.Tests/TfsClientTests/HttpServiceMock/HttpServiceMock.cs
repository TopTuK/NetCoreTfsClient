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
                ""id"": {0},
                ""rev"": 1,
                ""fields"": {
                    ""System.Id"": {0},
                    ""System.WorkItemType"": ""Requirement"",
                    ""System.Title"": ""Tfs test workitem - Requirement"",
                    ""System.Description"": ""Description of test workitem"",
                    ""System.AssignedTo"": {
			            ""displayName"": ""TopTuK"",
			            ""url"": ""https://tfs-url/tfs/blah"",
			            ""_links"": {
				            ""avatar"": {
					            ""href"": ""https://tfs-url/tfs/blah""
				            }
			            },
			            ""id"": ""GUID"",
			            ""uniqueName"": ""Test\\TopTuK"",
			            ""imageUrl"": ""https://tfs-url/tfs/imageUrl"",
			            ""descriptor"": ""win.sdgsdfgsdfg""
		            },
                },
                ""url"": ""https://tfs-tfs/tfs/_apis/wit/workItems/{0}""
            }]
        }";

        private static IHttpResponse CreateSingleItemHttpResponse(int workitemId)
        {
            var singleItemResponse = _singleItemResponseContent.Replace("{0}", workitemId.ToString());

            return Mock.Of<IHttpResponse>(resp =>
                resp.HasError == false &&
                resp.Headers == null &&
                resp.IsEmptyCookies == true &&
                resp.IsSuccess == true &&
                resp.RequestUrl == null &&
                resp.StatusCode == 200 &&
                resp.Cookies == null &&
                resp.ContentType == "" &&
                resp.Content == singleItemResponse
            );
        }

        public static Mock<IHttpService> CreateSingleItemHttpServiceMock(int workitemId)
        {
            var httpServiceMock = new Mock<IHttpService>();
            var response = CreateSingleItemHttpResponse(workitemId);

            httpServiceMock
                .Setup(mock => mock.Get(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>()
                 ))
                .Returns(response);

            return httpServiceMock;
        }
    }
}
