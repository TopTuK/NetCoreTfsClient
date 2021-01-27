using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using TfsClient.HttpService;

namespace TfsClient.Tests.TfsClientTests.HttpServiceMock
{
    public static class HttpServiceMockFactory
    {
        private static readonly string _itemContentTemplate = @"
        {
            ""id"": {_ID_},
            ""rev"": 1,
            ""fields"": {
                ""System.Id"": {_ID_},
                ""System.WorkItemType"": ""{_TYPE_}"",
                ""System.Title"": ""{_TITLE_}"",
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
            ""url"": ""https://tfs-tfs/tfs/_apis/wit/workItems/{_ID_}""
        }";

        private static readonly string _itemsResponseTemplate = @"
        {
            ""count"": {_COUNT_},
            ""value"": [{_ITEMS_}]
        }";

        private static IHttpResponse CreateSingleItemHttpResponse(int workitemId)
        {
            var itemContent = new StringBuilder(_itemContentTemplate);
            itemContent.Replace("{_ID_}", workitemId.ToString());
            itemContent.Replace("{_TYPE_}", "Task");
            itemContent.Replace("{_TITLE_}", "Test Task Item");

            var singleItemResponse = new StringBuilder(_itemsResponseTemplate);
            singleItemResponse.Replace("{_COUNT_}", "1");
            singleItemResponse.Replace("{_ITEMS_}", itemContent.ToString());

            return Mock.Of<IHttpResponse>(resp =>
                resp.HasError == false &&
                resp.Headers == null &&
                resp.IsEmptyCookies == true &&
                resp.IsSuccess == true &&
                resp.RequestUrl == null &&
                resp.StatusCode == 200 &&
                resp.Cookies == null &&
                resp.ContentType == "" &&
                resp.Content == singleItemResponse.ToString()
            );
        }

        private static IHttpResponse CreateItemsHttpResponse(IEnumerable<int> workitemIds)
        {
            List<string> itemsContent = new List<string>();

            foreach(var workitemId in workitemIds)
            {
                var itemContent = new StringBuilder(_itemContentTemplate);
                itemContent.Replace("{_ID_}", workitemId.ToString());
                itemContent.Replace("{_TYPE_}", "Task");
                itemContent.Replace("{_TITLE_}", $"Test Task Item - {workitemId}");

                itemsContent.Add(itemContent.ToString());
            }

            var itemsResponse = new StringBuilder(_itemsResponseTemplate);
            itemsResponse.Replace("{_COUNT_}", $"{itemsContent.Count}");
            itemsResponse.Replace("{_ITEMS_}", string.Join(',', itemsContent));

            return Mock.Of<IHttpResponse>(resp =>
                resp.HasError == false &&
                resp.Headers == null &&
                resp.IsEmptyCookies == true &&
                resp.IsSuccess == true &&
                resp.RequestUrl == null &&
                resp.StatusCode == 200 &&
                resp.Cookies == null &&
                resp.ContentType == "" &&
                resp.Content == itemsResponse.ToString()
            );
        }

        public static Mock<IHttpService> CreateSingleItemHttpServiceMock(int workitemId)
        {
            var response = CreateSingleItemHttpResponse(workitemId);

            var httpServiceMock = new Mock<IHttpService>();
            httpServiceMock
                .Setup(mock => mock.Get(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>()
                 ))
                .Returns(response);

            return httpServiceMock;
        }

        public static Mock<IHttpService> CreateItemsHttpServiceMock(IEnumerable<int> workitemIds)
        {
            var response = CreateItemsHttpResponse(workitemIds);

            var httpServiceMock = new Mock<IHttpService>();
            httpServiceMock
                .Setup(mock => mock.Get(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>()
                 ))
                .Returns(response);

            return httpServiceMock;
        }

        public static Mock<IHttpService> CreateUpdateItemFieldsServiceMock(
            int workItemId,
            IReadOnlyDictionary<string, string> itemFields)
        {
            StringBuilder itemResponse = new StringBuilder(_itemContentTemplate);
            itemResponse.Replace("{_ID_}", workItemId.ToString());
            itemResponse.Replace("{_TYPE_}", "Task");

            if (itemFields.ContainsKey("System.Title"))
            {
                itemResponse.Replace("{_TITLE_}", itemFields["System.Title"]);
            }
            else throw new ArgumentException("Dictonary must contain System.Title key", "itemFields");

            var httpServiceMock = new Mock<IHttpService>();
            httpServiceMock.Setup(mock => mock.PatchJson(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>()
                ))
                .Returns(Mock.Of<IHttpResponse>(resp =>
                    resp.HasError == false &&
                    resp.Headers == null &&
                    resp.IsEmptyCookies == true &&
                    resp.IsSuccess == true &&
                    resp.RequestUrl == null &&
                    resp.StatusCode == 200 &&
                    resp.Cookies == null &&
                    resp.ContentType == "" &&
                    resp.Content == itemResponse.ToString()
                )
            );

            return httpServiceMock;
        }
    }
}
