using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TfsClient.Tests.TfsClientTests
{
    public class TfsClientTests
    {
        private string _serverUrl = @"https://tfs-tfs/tfs";
        private string _singleProject = "DefaultProject";
        private string _subProject = @"DefaultProject/TestProject";

        [Fact(DisplayName = "TfsClient Check only single DefaultCollection project")]
        public void CreateTfsServiceClientSingleProject()
        {
            // Arrange
            var expectServerUrl = $"{_serverUrl}/";

            // Act
            var tfsService = TfsServiceClientFactory.CreateTfsServiceClient(_serverUrl, _singleProject);

            // Assert
            Assert.NotNull(tfsService);
            Assert.Equal(expectServerUrl, tfsService.ServerUrl);
            Assert.Equal(_singleProject, tfsService.Collection);
            Assert.Null(tfsService.Project);
        }

        [Fact(DisplayName = "TfsClient Check client with subproject DefaultCollection/TestProject")]
        public void CreateTfsServiceClientWithSubProject()
        {
            // Arrange
            var expectServerUrl = $"{_serverUrl}/";
            var projectName = "TestProject";

            // Act
            var tfsService = TfsServiceClientFactory.CreateTfsServiceClient(_serverUrl, _subProject);

            // Assert
            Assert.NotNull(tfsService);
            Assert.Equal(expectServerUrl, tfsService.ServerUrl);
            Assert.Equal(_singleProject, tfsService.Collection);
            Assert.Equal(projectName, tfsService.Project);
        }
    }
}
