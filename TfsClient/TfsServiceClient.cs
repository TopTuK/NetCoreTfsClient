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
        private const string GET_WORKITEM_URL = @"wit/workitems";

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

            _httpService.BaseUrl = new Uri(ServerUrl);

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
            _tfsUrl = $"{Collection}/_apis/";

            // API response only for Collection/Project
            _tfsUrlPrj = Project != null
                ? $"{Collection}/{Project}/_apis/"
                : _tfsUrl;
        }

        public void Authentificate(string userName, string userPassword)
        {
            _httpService.Authentificate(userName, userPassword);
        }

        private IEnumerable<ITfsWorkitem> GetTfsItems(string requstUrl,
            IReadOnlyDictionary<String, string> requestParams = null,
            bool underProject = false)
        {
            var url = underProject
                ? (_tfsUrlPrj + requstUrl)
                : (_tfsUrl + requstUrl);

            var response = _httpService.Get(url, requestParams);

            IEnumerable<ITfsWorkitem> items = null;
            if((response != null) && (response.IsSuccess))
            {
                items = TfsWorkitemFactory.FromJsonItems(response.Content);
            }

            return items;
        }

        public ITfsWorkitem GetSingleWorkitem(int id, IEnumerable<string> fields = null)
        {
            var requestParams = new Dictionary<string, string>
            {
                { "expand", "all" },
                { "api-version", "1.0" },
                { "ids", $"{id}" }
            };

            if(fields != null)
            {
                var flds = string.Join(",", fields);
                requestParams.Add("fields", flds);
            }

            IEnumerable<ITfsWorkitem> items;
            try
            {
                items = GetTfsItems(GET_WORKITEM_URL, requestParams);
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
            var defaultRequestParams = new Dictionary<string, string>
            {
                { "expand", "all" },
                { "api-version", "1.0" }
            };

            if (fields != null)
            {
                var flds = string.Join(",", fields);
                defaultRequestParams.Add("fields", flds);
            }

            List<ITfsWorkitem> resultItems = new List<ITfsWorkitem>(ids.Count());
            foreach (var items in ids.Batch(batchSize))
            {
                var requestParams = new Dictionary<string, string>(defaultRequestParams)
                {
                    { "ids", string.Join(",", items) }
                };

                try
                {
                    var tfsItems = GetTfsItems(GET_WORKITEM_URL, requestParams);
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
