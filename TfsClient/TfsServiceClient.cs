using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TfsClient.HttpService;
using TfsClient.Utils;

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
            _httpService = httpService;

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

        private IEnumerable<ITfsWorkitem> GetTfsItems(string requstUrl, bool underProject = false)
        {
            var url = underProject
                ? (_tfsUrlPrj + requstUrl)
                : (_tfsUrl + requstUrl);

            var response = _httpService.Get(url);
            if((response != null) && (response.IsSuccess))
            {
                var json = response.Content;
            }

            return null;
        }

        public ITfsWorkitem GetSingleWorkitem(int id, IEnumerable<string> fields = null)
        {
            var flds = fields != null ? $"&fields={string.Join(",", fields)}" : "";
            var requestUrl = $"wit/workitems?ids={id}{flds}&expand=all&api-version=1.0";

            IEnumerable<ITfsWorkitem> items;

            try
            {
                items = GetTfsItems(requestUrl);
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return items?.FirstOrDefault();
        }

        public IEnumerable<ITfsWorkitem> GetWorkitems(IEnumerable<int> ids,
            IEnumerable<string> fields = null, int batchSize = 50)
        {
            List<ITfsWorkitem> resultItems = new List<ITfsWorkitem>(ids.Count());

            var flds = fields != null ? $"&fields={string.Join(",", fields)}" : "";

            foreach (var items in ids.Batch(batchSize))
            {                
                var requestUrl = $"wit/workitems?ids={string.Join(",", items)}{flds}&expand=all&api-version=1.0";

                try
                {
                    var tfsItems = GetTfsItems(requestUrl);
                    resultItems.AddRange(tfsItems);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }

            return resultItems;
        }
    }
}
