using System;
using System.Collections.Generic;
using System.Text;
using TfsClient.Tests.TfsClientTests.HttpServiceMock;
using Xunit;

namespace TfsClient.Tests.TfsClientTests
{
    public class TfsUpdateWorkitemFieldsTests
    {
        private const int SINGLE_WORKITEM_ID = 100500;

        private readonly string _tfsServerUrl = @"https://tfs-tfs/tfs";
        private readonly string _tfsProjectName = @"DefaultCollection/TestProject";

        private IReadOnlyDictionary<string, string> _itemFields = new Dictionary<string, string>()
        {
            { "System.Title", "Update test workitem" }
        };

        [Fact(DisplayName = "Simple Update item Fields Success Test")]
        public void TfsClientSimpleUpdateFieldsSuccess()
        {
            // Arrange
            var httpServiceMock = HttpServiceMockFactory
                .CreateUpdateItemFieldsServiceMock(SINGLE_WORKITEM_ID, _itemFields);
            var tfsService = TfsServiceClientFactory
                .CreateTfsServiceClient(httpServiceMock.Object, _tfsServerUrl, _tfsProjectName);

            // Act
            var item = tfsService.UpdateWorkitemFields(SINGLE_WORKITEM_ID, _itemFields);

            // Assert
            Assert.NotNull(item);
            Assert.Equal(SINGLE_WORKITEM_ID, item.Id);
            Assert.Equal(_itemFields["System.Title"], item["System.Title"]);
        }

        [Fact(DisplayName = "Simple Update Workitem Field Success Test")]
        public void TfsClientSimpleUpdateItemFieldsSuccess()
        {
            // Arrange
            var httpServiceMock = HttpServiceMockFactory
                .CreateUpdateItemFieldsServiceMock(SINGLE_WORKITEM_ID, _itemFields);
            var tfsService = TfsServiceClientFactory
                .CreateTfsServiceClient(httpServiceMock.Object, _tfsServerUrl, _tfsProjectName);

            // Act
            var item = tfsService.UpdateWorkitemFields(SINGLE_WORKITEM_ID, _itemFields);
            item.UpdateFields();

            // Assert
            Assert.NotNull(item);
            Assert.Equal(SINGLE_WORKITEM_ID, item.Id);
            Assert.Equal(_itemFields["System.Title"], item["System.Title"]);
        }
    }
}
