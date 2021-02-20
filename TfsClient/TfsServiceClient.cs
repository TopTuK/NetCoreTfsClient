using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TfsClient.HttpService;
using TfsClient.Utils;

namespace TfsClient
{
    public enum TfsItemUpdateResult
    {
        FAIL_UPDATE = 0,
        NOTHING_UPDATE,
        SUCCESS_UPDATE
    }

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

        public static ITfsServiceClient CreateTfsServiceClient(string serverUrl, string projectName,
            string personalAccessToken)
        {
            if(personalAccessToken == null)
            {
                throw new ArgumentException("Token must be not null", "personalAccessToken");
            }

            var tfsServiceClient = new TfsServiceClient(HttpServiceFactory.CreateHttpService(), serverUrl, projectName);
            tfsServiceClient.Authentificate(personalAccessToken);

            return tfsServiceClient;
        }

        public static ITfsServiceClient CreateTfsServiceClient(IHttpService httpService, 
            string serverUrl, string projectName)
        {
            return new TfsServiceClient(httpService, serverUrl, projectName);
        }

        public static ITfsServiceClientWorkitemFacade GetTfsWorkitemService(this ITfsServiceClient tfsServiceClient) =>
            new TfsServiceClientWorkitemFacade(tfsServiceClient);
    }

    internal class TfsServiceClient : ITfsServiceClient
    {
        private const string API_VERSION = "6.0";
        private const string WORKITEM_URL = @"wit/workitems";
        private const string WIQL_URL = @"wit/wiql";
        private const string QUERY_URL = @"wit/queries";

        private readonly IHttpService _httpService;
        private readonly string _tfsUrl;
        private readonly string _tfsUrlPrj;

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

        public void Authentificate(string personalAccessToken)
        {
            _httpService.Authentificate(personalAccessToken);
        }

        private string GetWorkitemUrl(int workitemId) => $"{ServerUrl}{_tfsUrl}{WORKITEM_URL}/{workitemId}";
        private Dictionary<string, string> MakeQueryParams(string expand, bool bypassRules,
            bool suppressNotifications, bool validateOnly) => new Dictionary<string, string>()
            {
                { "api-version", API_VERSION },
                { "$expand", expand },
                { "bypassRules", $"{bypassRules}" },
                { "suppressNotifications", $"{suppressNotifications}" },
                { "validateOnly", $"{validateOnly}" }
            };

        private IEnumerable<ITfsWorkitem> GetTfsItems(string requestUrl,
            IReadOnlyDictionary<string, string> requestParams = null,
            bool underProject = false)
        {
            var url = underProject
                ? (_tfsUrlPrj + requestUrl)
                : (_tfsUrl + requestUrl);

            var response = _httpService.Get(url, requestParams);

            if((response != null) && (response.IsSuccess))
            {
                var items = TfsWorkitemFactory.FromJsonItems(this, response.Content);
                
                return items;
            }

            return null;
        }

        public ITfsWorkitem GetSingleWorkitem(int id, IEnumerable<string> fields = null, string expand = "All")
        {
            int[] ids = new int[]
            {
                id
            };
            var items = GetWorkitems(ids, fields, expand);

            return items?.FirstOrDefault();
        }

        public IEnumerable<ITfsWorkitem> GetWorkitems(IEnumerable<int> ids,
            IEnumerable<string> fields = null, string expand = "All", int batchSize = 50)
        {
            var defaultRequestParams = new Dictionary<string, string>
            {
                { "$expand", $"{expand}" },
                { "api-version", API_VERSION }
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
                    var tfsItems = GetTfsItems(WORKITEM_URL, requestParams);
                    resultItems.AddRange(tfsItems);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }

            return resultItems;
        }

        // https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/work%20items/create?view=azure-devops-rest-6.0
        public ITfsWorkitem CreateWorkitem(string itemType, IReadOnlyDictionary<string, string> itemFields = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false)
        {
            if (itemType == null)
            {
                throw new ArgumentNullException("itemType", "type can not be null");
            }

            var requestUrl = $"{_tfsUrlPrj}/{WORKITEM_URL}/${itemType}";

            var queryParams = MakeQueryParams(expand, bypassRules, suppressNotifications, validateOnly);

            // Media Types: "application/json-patch+json"
            var requestBody = itemFields
                .Select(fld => new {
                    op = "add",
                    path = $"/fields/{fld.Key}",
                    @from = (string) null,
                    value = fld.Value
                })
                .ToList();

            var customHeaders = new Dictionary<string, string>()
            {
                { "Content-Type", "application/json-patch+json" }
            };

            try
            {
                var response = _httpService.PostJson(requestUrl, requestBody,
                    customParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public ITfsWorkitem CreateWorkitem(WorkItemType itemType, IReadOnlyDictionary<string, string> itemFields = null)
        {
            if(itemType == WorkItemType.Unknown)
            {
                throw new ArgumentException("Type can't be unknown", "itemType");
            }

            return CreateWorkitem(TfsWorkitemFactory.WI_TYPE_MAP[itemType], itemFields);
        }

        public ITfsWorkitem CopyWorkitem(int sourceItemId, IReadOnlyDictionary<string, string> destinationItemFields = null)
        {
            var sourceItem = GetSingleWorkitem(sourceItemId);

            return (sourceItem != null)
                ? CopyWorkitem(sourceItem, destinationItemFields)
                : null;
        }

        public ITfsWorkitem CopyWorkitem(ITfsWorkitem sourceItem, 
            IReadOnlyDictionary<string, string> destinationItemFields = null)
        {
            if(sourceItem == null)
            {
                throw new ArgumentException("Source item can't be null", "sourceItem");
            }

            var ignoreFields = new List<string>
            {
                "System.TeamProject",
                "System.AreaPath",
                "System.AreaId",
                "System.AreaLevel1",
                "System.AreaLevel2",
                "System.AreaLevel3",
                "System.AreaLevel4",
                "System.Id",
                "System.NodeName",
                "System.Rev",
                "System.AutorizedDate",
                "System.RevisedDate",
                "System.IterationId",
                "System.IterationLevel1",
                "System.IterationLevel2",
                "System.IterationLevel3",
                "System.IterationLevel4",
                "System.CreatedBy",
                "System.ChangedDate",
                "System.ChangedBy",
                "System.AuthorizedAs",
                "System.AuthorizedDate",
                "System.Watermark"
            };

            var itemTypeName = sourceItem.ItemTypeName;
            var sourceItemFields = sourceItem.FieldNames;

            var fields = new Dictionary<string, string>(sourceItemFields.Count);
            foreach(var fldName in sourceItemFields)
            {
                if (ignoreFields.Contains(fldName)) continue;

                string fldValue = sourceItem[fldName];
                if((destinationItemFields != null) && (destinationItemFields.ContainsKey(fldName)))
                {
                    fldValue = destinationItemFields[fldName];
                }

                fields.Add(fldName, fldValue);
            }

            return CreateWorkitem(itemTypeName, itemFields: fields);

            throw new NotImplementedException();
        }

        // https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/work%20items/update?view=azure-devops-rest-6.0
        public ITfsWorkitem UpdateWorkitemFields(int workitemId, IReadOnlyDictionary<string, string> itemFields,
            string expand = "All", bool bypassRules = false, 
            bool suppressNotifications = false, bool validateOnly = false)
        {
            if (itemFields == null)
            {
                throw new ArgumentNullException("itemFields");
            }

            var requestBody = itemFields
                .Select(fld => new { 
                    op = "add",
                    path = $"/fields/{fld.Key}",
                    value = fld.Value
                })
                .ToList();

            var requestUrl = $"{_tfsUrlPrj}/{WORKITEM_URL}/{workitemId}";

            var queryParams = MakeQueryParams(expand, bypassRules, suppressNotifications, validateOnly);

            var customHeaders = new Dictionary<string, string>()
            {
                { "Content-Type", "application/json-patch+json" }
            };

            try
            {
                var response = _httpService.PatchJson(requestUrl, requestBody, 
                    customParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        // https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/work%20items/update?view=azure-devops-rest-6.0#add-a-link
        public ITfsWorkitem AddRelationLink(
            int sourceWorkitemId, int destinationWorkitemId, 
            string relationType, IReadOnlyDictionary<string, string> relationAttributes = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false)
        {
            if((relationType == null) || (relationType.Trim() == ""))
            {
                throw new ArgumentNullException("relationType", "parametr is null or empty");
            }

            var requestUrl = $"{_tfsUrlPrj}{WORKITEM_URL}/{sourceWorkitemId}";

            var queryParams = MakeQueryParams(expand, bypassRules, suppressNotifications, validateOnly);

            var requestBody = new[]
            { 
                new
                {
                    op = "add",
                    path = @"/relations/-",
                    value = new
                    {
                        rel = relationType,
                        url = GetWorkitemUrl(destinationWorkitemId),
                        attributes = relationAttributes
                    }
                }
            };

            var customHeaders = new Dictionary<string, string>()
            {
                { "Content-Type", "application/json-patch+json" }
            };

            try
            {
                var response = _httpService.PatchJson(requestUrl, requestBody, 
                    customParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ITfsWorkitem AddRelationLink(
            int sourceWorkitemId, int destinationWorkitemId,
            WorkitemRelationType relationType, IReadOnlyDictionary<string, string> relationAttributes = null)
        {
            if(!TfsWorkitemFactory.RELATION_TYPE_MAP.TryGetValue(relationType, out string relTypeName))
            {
                throw new ArgumentException("relationType must not be unknown", "relationType");
            }

            return AddRelationLink(sourceWorkitemId, destinationWorkitemId, relTypeName, relationAttributes);
        }

        public ITfsWorkitem RemoveRelationLink(
            int workitemId, int relationId,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false
            )
        {
            var requestUrl = $"{_tfsUrlPrj}{WORKITEM_URL}/{workitemId}";
            var queryParams = MakeQueryParams(expand, bypassRules, suppressNotifications, validateOnly);

            var requestBody = new[]
            {
                new
                {
                    op = "remove",
                    path = $"/relations/{relationId}"
                }
            };

            var customHeaders = new Dictionary<string, string>()
            {
                { "Content-Type", "application/json-patch+json" }
            };

            try
            {
                var response = _httpService.PatchJson(requestUrl, requestBody, 
                    customParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ITfsWiqlResult RunSavedQuery(string queryId)
        {
            if (queryId == null)
            {
                throw new ArgumentException("param can't be null", "queryId");
            }

            var queryParams = new Dictionary<string, string>()
            {
                { "api-version", API_VERSION },
                { "$expand", "clauses" }
            };

            var requestUrl = $"{_tfsUrlPrj}{QUERY_URL}/{queryId}";
            try
            {
                var response = _httpService.Get(requestUrl, queryParams);

                return response.IsSuccess
                    ? TfsWiqlFactory.FromQueryResponse(this, response.Content)
                    : null;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public ITfsWiqlResult RunWiql(string query, int maxTop = -1)
        {
            if(query == null)
            {
                throw new ArgumentException("param can't be null", "query");
            }

            var requestBody = new
            {
                query = query
            };

            var queryParams = new Dictionary<string, string>()
            {
                { "api-version", API_VERSION }
            };

            if(maxTop > 0)
            {
                queryParams.Add("$top", maxTop.ToString());
            }

            var requestUrl = $"{_tfsUrlPrj}{WIQL_URL}";

            try
            {
                var response = _httpService.PostJson(requestUrl, requestBody, customParams: queryParams);

                return response.IsSuccess
                    ? TfsWiqlFactory.FromContentResponse(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
