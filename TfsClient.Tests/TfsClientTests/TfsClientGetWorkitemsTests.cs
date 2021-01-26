using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TfsClient.Tests.TfsClientTests.HttpServiceMock;
using Xunit;

namespace TfsClient.Tests.TfsClientTests
{
    public class TfsClientGetWorkitemsTests
    {
        private const int SINGLE_WORKITEM_ID = 100500;

        private readonly string _tfsServerUrl = @"https://tfs-tfs/tfs";
        private readonly string _tfsProjectName = @"DefaultCollection/TestProject";

        [Fact(DisplayName = "TfsClient GetSingleItem Test Success")]
        public void TfsClientGetSingleWorkitemSuccessTest()
        {
            // Arrange
            var httpServiceMock = HttpServiceMockFactory
                .CreateSingleItemHttpServiceMock(SINGLE_WORKITEM_ID);
            var tfsService = TfsServiceClientFactory
                .CreateTfsServiceClient(httpServiceMock.Object, _tfsServerUrl, _tfsProjectName);

            // Act
            var tfsItem = tfsService.GetSingleWorkitem(SINGLE_WORKITEM_ID);

            // Assert
            Assert.NotNull(tfsItem);
            Assert.Equal(SINGLE_WORKITEM_ID, tfsItem.Id);
            Assert.NotNull(tfsItem["System.Title"]);
            Assert.NotNull(tfsItem["System.AssignedTo"]);
        }

        [Fact(DisplayName = "TfsClient GetWorkitems Test Success")]
        public void TfsClientGetworkitemsSuccessTest()
        {
            // Arrange
            int[] ids = new int[] { 100500, 100501, 100502 };
            var httpServiceMock = HttpServiceMockFactory
                .CreateItemsHttpServiceMock(ids);
            var tfsService = TfsServiceClientFactory
                .CreateTfsServiceClient(httpServiceMock.Object, _tfsServerUrl, _tfsProjectName);

            // Act
            var items = tfsService.GetWorkitems(ids);

            // Assert
            Assert.NotNull(items);
            Assert.Equal(ids.Count(), items.Count());
            foreach(var item in items)
            {
                Assert.Contains(item.Id, ids);
                Assert.NotNull(item["System.Title"]);
                Assert.NotNull(item["System.AssignedTo"]);
            }
        }
    }
}
