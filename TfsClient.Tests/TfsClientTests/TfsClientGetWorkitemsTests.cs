using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TfsClient.Tests.TfsClientTests
{
    public class TfsClientGetWorkitemsTests
    {
        private const int BATCH_SIZE = 2;

        private const int WORKITEM_ID = 100500;
        private readonly int[] WORKITEM_IDS = { 100, 101, 102, 103 };

        private readonly string _tfsServerUrl = @"https://tfs-tfs/tfs";
        private readonly string _tfsProjectName = @"DefaultCollection/TestProject";

        [Fact(Skip = "Not implemented")]
        public void TfsClientGetSingleWorkitemTest()
        {
            // Arrange
            var tfsService = TfsServiceClientFactory.CreateTfsServiceClient(null, _tfsServerUrl, _tfsProjectName);

            // Act
            var tfsItem = tfsService.GetSingleWorkitem(WORKITEM_ID);

            // Assert
        }

        [Fact(Skip = "Not implemented")]
        public void TfsClientGetMultipyItemsTest()
        {
            // Arrange
            var tfsService = TfsServiceClientFactory.CreateTfsServiceClient(null, _tfsServerUrl, _tfsProjectName);

            // Act
            var tfsItems = tfsService.GetWorkitems(WORKITEM_IDS);

            // Assert
        }

        [Fact(Skip = "Not implemented")]
        public void TfsClientGetManyItemsTest()
        {
            // Arrange
            var tfsService = TfsServiceClientFactory.CreateTfsServiceClient(null, _tfsServerUrl, _tfsProjectName);

            // Act
            var tfsItems = tfsService.GetWorkitems(WORKITEM_IDS, batchSize: BATCH_SIZE);

            // Assert
        }

        [Fact(Skip = "Not implemented")]
        public void TfsClientGetItemNotFound()
        {
            // Arrange
            var tfsService = TfsServiceClientFactory.CreateTfsServiceClient(null, _tfsServerUrl, _tfsProjectName);

            // Act
            var tfsItem = tfsService.GetSingleWorkitem(WORKITEM_ID);

            // Assert
        }
    }
}
