using System;
using System.Collections.Generic;
using System.Text;
using TfsClient.HttpService;

namespace TfsClient
{
    public static class TfsServiceClientFactory
    {
        public static ITfsServiceClient CreateTfsServiceClient(string serverUrl, string projectName,
            string userName = null, string userPassword = null)
        {
            var tfsServiceClient = new TfsServiceClient(HttpServiceFactory.CreateHttpService(), serverUrl, projectName);
            
            if ((userName != null) && (userPassword != null))
            {
                tfsServiceClient.Authentificate(userName, userPassword);
            }

            return tfsServiceClient;
        }

        public static ITfsServiceClient CreateTfsServiceClient(IHttpService httpService, 
            string serverUrl, string projectName)
        {
            return new TfsServiceClient(httpService, serverUrl, projectName);
        }
    }

    internal class TfsServiceClient : ITfsServiceClient
    {
        private readonly IHttpService _httpService;
        private string _tfsUrl;
        private string _tfsUrlPrj;

        public string ServerUrl { get; }
        public string Collection { get; }
        public string Project { get; }

        public TfsServiceClient(IHttpService httpService, string serverUrl, string projectName)
        {
            // ServerUrl should ends with '/'
            ServerUrl = serverUrl.EndsWith("/")
                ? serverUrl
                : $"{serverUrl}/";

            // Closure function to get Collection and Project
            (string, string) GetCollectionAndProject()
            {
                var splitPrj = projectName.Split('/');

                string collection = splitPrj[0];
                string project = null;

                if ((splitPrj.Length > 1) && (splitPrj[1].Trim() != ""))
                {
                    project = splitPrj[1];
                }

                return (collection, project);
            }

            // Assign Collection and Project
            (Collection, Project) = GetCollectionAndProject();

            // Remove part after / in project-name, like DefaultCollection/MyProject => DefaultCollection
            // API responce only in Project, without subproject
            _tfsUrl = $"{ServerUrl}{Collection}/_apis/";

            // API response only for Collection/Project
            _tfsUrlPrj = Project == null
                ? $"{ServerUrl}{Collection}/{Project}/_apis/"
                : _tfsUrl;
        }

        public void Authentificate(string userName, string userPassword)
        {
            _httpService.Authentificate(userName, userPassword);
        }

        public ITfsWorkitem GetSingleWorkitem(int id, IEnumerable<string> fields = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITfsWorkitem> GetWorkitems(IEnumerable<int> ids,
            IEnumerable<string> fields = null, int batchSize = 50)
        {
            throw new NotImplementedException();
        }
    }
}
