using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TfsClient.HttpService;
using TfsClient.Utils;

namespace TfsClient
{
    /// <summary>
    /// Tfs Service Client Factory. Returns <see cref="ITfsServiceClient"/> interface implementation
    /// </summary>
    public static class TfsServiceClientFactory
    {
        /// <summary>
        /// Get <see cref="ITfsServiceClient"/> interface and set authentification with NTLM
        /// </summary>
        /// <param name="serverUrl">Server url</param>
        /// <param name="projectName">Project name with collection (ex.: DefaultCollection/MyProject)</param>
        /// <param name="userName">User name (with domain prefix)</param>
        /// <param name="userPassword">User password</param>
        /// <returns><see cref="ITfsServiceClient"/> implementation</returns>
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

        /// <summary>
        /// Get <see cref="ITfsServiceClient"/> interface and set authentification with personal access token
        /// </summary>
        /// <param name="serverUrl">Server url</param>
        /// <param name="projectName">Project name with collection (ex.: DefaultCollection/MyProject)</param>
        /// <param name="personalAccessToken">Personal access token</param>
        /// <returns><see cref="ITfsServiceClient"/> implementation</returns>
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

        /// <summary>
        /// Create <see cref="ITfsServiceClient"/> implementation with custom <see cref="IHttpService"/> realization
        /// </summary>
        /// <param name="httpService">Custom realization of <see cref="IHttpService"/></param>
        /// <param name="serverUrl">Server url</param>
        /// <param name="projectName">Project name with collection (ex.: DefaultCollection/MyProject)</param>
        /// <returns></returns>
        public static ITfsServiceClient CreateTfsServiceClient(IHttpService httpService, 
            string serverUrl, string projectName)
        {
            return new TfsServiceClient(httpService, serverUrl, projectName);
        }

        /// <summary>
        /// Extension method for get <see cref="ITfsServiceClientWorkitemFacade"/> facade
        /// </summary>
        /// <returns><see cref="ITfsServiceClientWorkitemFacade"/></returns>
        public static ITfsServiceClientWorkitemFacade GetTfsWorkitemServiceFacade(this ITfsServiceClient tfsServiceClient) =>
            new TfsServiceClientWorkitemFacade(tfsServiceClient);

        /// <summary>
        /// Extension method for get <see cref="IAsyncTfsServiceClientWorkitemFacade"/> facade
        /// </summary>
        /// <returns><see cref="IAsyncTfsServiceClientWorkitemFacade"/></returns>
        public static IAsyncTfsServiceClientWorkitemFacade GetTfsWorkitemServiceAsyncFacade(this ITfsServiceClient tfsServiceClient) =>
            new AsyncTfsServiceClientWorkitemFacade(tfsServiceClient);
    }

    internal class TfsServiceClient : ITfsServiceClient
    {
        private const string API_VERSION = "6.0";
        private const string WORKITEM_URL = @"wit/workitems";
        private const string WIQL_URL = @"wit/wiql";
        private const string QUERY_URL = @"wit/queries";
        private const string CHANGESET_URL = @"tfvc/changesets";

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

        private IEnumerable<ITfsWorkitem> GetTfsItemsFromResponse(IHttpResponse response)
        {
            if ((response != null) && (response.IsSuccess))
            {
                return TfsWorkitemFactory.FromJsonItems(this, response.Content);
            }

            return null;
        }

        private IEnumerable<ITfsWorkitem> GetTfsItems(string requestUrl,
            IReadOnlyDictionary<string, string> requestParams = null,
            bool underProject = false)
        {
            var url = underProject
                ? (_tfsUrlPrj + requestUrl)
                : (_tfsUrl + requestUrl);

            var response = _httpService.Get(url, requestParams);

            return GetTfsItemsFromResponse(response);
        }

        private async Task<IEnumerable<ITfsWorkitem>> GetTfsItemsAsync(string requestUrl,
            IReadOnlyDictionary<string, string> requestParams = null,
            bool underProject = false)
        {
            var url = underProject
                ? (_tfsUrlPrj + requestUrl)
                : (_tfsUrl + requestUrl);

            var response = await _httpService.GetAsync(url, requestParams);

            return GetTfsItemsFromResponse(response);
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

        public async Task<ITfsWorkitem> GetSingleWorkitemAsync(int id, IEnumerable<string> fields = null, string expand = "All")
        {
            int[] ids = new int[]
            {
                id
            };
            var items = await GetWorkitemsAsync(ids, fields, expand);

            return items?.FirstOrDefault();
        }

        private Dictionary<string, string> GetWorkitemsPrepareArgs(IEnumerable<string> fields, string expand)
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

            return defaultRequestParams;
        }

        public IEnumerable<ITfsWorkitem> GetWorkitems(IEnumerable<int> ids,
            IEnumerable<string> fields = null, string expand = "All", int batchSize = 50)
        {
            var defaultRequestParams = GetWorkitemsPrepareArgs(fields, expand);

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
                    throw new TfsServiceClientException("TfsServiceClient: GetWorkitems exception", ex);
                }
            }

            return resultItems;
        }

        public async Task<IEnumerable<ITfsWorkitem>> GetWorkitemsAsync(IEnumerable<int> ids,
            IEnumerable<string> fields = null, string expand = "All", int batchSize = 50)
        {
            var defaultRequestParams = GetWorkitemsPrepareArgs(fields, expand);

            List<ITfsWorkitem> resultItems = new List<ITfsWorkitem>(ids.Count());
            foreach (var items in ids.Batch(batchSize))
            {
                var requestParams = new Dictionary<string, string>(defaultRequestParams)
                {
                    { "ids", string.Join(",", items) }
                };

                try
                {
                    var tfsItems = await GetTfsItemsAsync(WORKITEM_URL, requestParams);
                    resultItems.AddRange(tfsItems);
                }
                catch (Exception ex)
                {
                    throw new TfsServiceClientException("TfsServiceClient: GetWorkitemsAsync exception", ex);
                }
            }

            return resultItems;
        }

        private Dictionary<string, string> MakeQueryParams(string expand, bool bypassRules,
            bool suppressNotifications, bool validateOnly) => new Dictionary<string, string>()
            {
                { "api-version", API_VERSION },
                { "$expand", expand },
                { "bypassRules", $"{bypassRules}" },
                { "suppressNotifications", $"{suppressNotifications}" },
                { "validateOnly", $"{validateOnly}" }
            };

        private (string, Dictionary<string, string>, object, Dictionary<string, string>) CreateWorkitemPrepareArgs(
            string itemType, 
            IReadOnlyDictionary<string, string> itemFields, IEnumerable<ITfsWorkitemRelation> itemRelations,
            string expand, bool bypassRules,
            bool suppressNotifications, bool validateOnly)
        {
            var requestUrl = $"{_tfsUrlPrj}/{WORKITEM_URL}/${itemType}";

            var queryParams = MakeQueryParams(expand, bypassRules, suppressNotifications, validateOnly);

            var requestBody = itemFields
                .Select(fld => new {
                    op = "add",
                    path = $"/fields/{fld.Key}",
                    @from = (string)null,
                    value = fld.Value
                })
                .ToList<object>();

            if (itemRelations != null)
            {
                foreach(var relation in itemRelations)
                {
                    requestBody.Add(new
                    {
                        op = "add",
                        path = $"/relations/-",
                        @from = (string)null,
                        value = new
                        {
                            rel = relation.RelationType,
                            url = relation.Url,
                            attributes = (string)null
                        }
                    });
                }
            }

            // Media Types: "application/json-patch+json"
            var customHeaders = new Dictionary<string, string>()
            {
                { "Content-Type", "application/json-patch+json" }
            };

            return (requestUrl, queryParams, requestBody, customHeaders);
        }

        // https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/work%20items/create?view=azure-devops-rest-6.0
        public ITfsWorkitem CreateWorkitem(string itemType, 
            IReadOnlyDictionary<string, string> itemFields = null, IEnumerable<ITfsWorkitemRelation> itemRelations = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false)
        {
            if (itemType == null)
            {
                throw new ArgumentNullException("itemType", "type can not be null");
            }

            (var requestUrl, var queryParams, var requestBody, var customHeaders) = CreateWorkitemPrepareArgs(
                itemType, itemFields, itemRelations, expand, bypassRules, suppressNotifications, validateOnly
            );

            try
            {
                var response = _httpService.PostJson(requestUrl, requestBody,
                    queryParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch(Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: CreateWorkitem exception", ex);
            }
        }

        public async Task<ITfsWorkitem> CreateWorkitemAsync(string itemType, 
            IReadOnlyDictionary<string, string> itemFields = null, IEnumerable<ITfsWorkitemRelation> itemRelations = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false)
        {
            if (itemType == null)
            {
                throw new ArgumentNullException("itemType", "type can not be null");
            }

            (var requestUrl, var queryParams, var requestBody, var customHeaders) = CreateWorkitemPrepareArgs(
                itemType, itemFields, itemRelations, expand, bypassRules, suppressNotifications, validateOnly
            );

            try
            {
                var response = await _httpService.PostJsonAsync(requestUrl, requestBody,
                    queryParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: CreateWorkitemAsync exception", ex);
            }
        }

        public ITfsWorkitem CreateWorkitem(WorkItemType itemType, 
            IReadOnlyDictionary<string, string> itemFields = null, IEnumerable<ITfsWorkitemRelation> itemRelations = null)
        {
            if(itemType == WorkItemType.Unknown)
            {
                throw new ArgumentException("Type can't be unknown", "itemType");
            }

            return CreateWorkitem(TfsWorkitemFactory.WI_TYPE_MAP[itemType], itemFields, itemRelations);
        }

        public async Task<ITfsWorkitem> CreateWorkitemAsync(WorkItemType itemType,
            IReadOnlyDictionary<string, string> itemFields = null, IEnumerable<ITfsWorkitemRelation> itemRelations = null)
        {
            if (itemType == WorkItemType.Unknown)
            {
                throw new ArgumentException("Type can't be unknown", "itemType");
            }

            return await CreateWorkitemAsync(TfsWorkitemFactory.WI_TYPE_MAP[itemType], itemFields, itemRelations);
        }

        private IReadOnlyDictionary<string, string> CopyWorkitemPrepareArgs(ITfsWorkitem sourceItem, 
            IReadOnlyDictionary<string, string> destinationItemFields)
        {
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

            var sourceItemFields = sourceItem.FieldNames;

            var fields = new Dictionary<string, string>(sourceItemFields.Count);
            foreach (var fldName in sourceItemFields)
            {
                if (ignoreFields.Contains(fldName)) continue;

                string fldValue = sourceItem[fldName];
                if ((destinationItemFields != null) && (destinationItemFields.ContainsKey(fldName)))
                {
                    fldValue = destinationItemFields[fldName];
                }

                fields.Add(fldName, fldValue);
            }

            return fields;
        }

        public ITfsWorkitem CopyWorkitem(ITfsWorkitem sourceItem, 
            IReadOnlyDictionary<string, string> destinationItemFields = null)
        {
            if(sourceItem == null)
            {
                throw new ArgumentException("Source item can't be null", "sourceItem");
            }

            var itemTypeName = sourceItem.ItemTypeName;
            var fields = CopyWorkitemPrepareArgs(sourceItem, destinationItemFields);

            return CreateWorkitem(itemTypeName, itemFields: fields);
        }

        public async Task<ITfsWorkitem> CopyWorkitemAsync(ITfsWorkitem sourceItem,
            IReadOnlyDictionary<string, string> destinationItemFields = null)
        {
            if (sourceItem == null)
            {
                throw new ArgumentException("Source item can't be null", "sourceItem");
            }

            var itemTypeName = sourceItem.ItemTypeName;
            var fields = CopyWorkitemPrepareArgs(sourceItem, destinationItemFields);

            return await CreateWorkitemAsync(itemTypeName, itemFields: fields);
        }

        public ITfsWorkitem CopyWorkitem(int sourceItemId, IReadOnlyDictionary<string, string> destinationItemFields = null)
        {
            var sourceItem = GetSingleWorkitem(sourceItemId);

            return (sourceItem != null)
                ? CopyWorkitem(sourceItem, destinationItemFields)
                : null;
        }

        public async Task<ITfsWorkitem> CopyWorkitemAsync(int sourceItemId,
            IReadOnlyDictionary<string, string> destinationItemFields = null)
        {
            var sourceItem = await GetSingleWorkitemAsync(sourceItemId);

            return (sourceItem != null)
                ? await CopyWorkitemAsync(sourceItem, destinationItemFields)
                : null;
        }

        private (string, object, IReadOnlyDictionary<string, string>, IReadOnlyDictionary<string, string>)
            UpdateWorkitemFieldsPrepareAgrs(int workitemId, IReadOnlyDictionary<string, string> itemFields,
                string expand, bool bypassRules,
                bool suppressNotifications, bool validateOnly)
        {
            var requestUrl = $"{_tfsUrlPrj}/{WORKITEM_URL}/{workitemId}";

            var requestBody = itemFields
                .Select(fld => new {
                    op = "add",
                    path = $"/fields/{fld.Key}",
                    value = fld.Value
                })
                .ToList();

            var queryParams = MakeQueryParams(expand, bypassRules, suppressNotifications, validateOnly);

            var customHeaders = new Dictionary<string, string>()
            {
                { "Content-Type", "application/json-patch+json" }
            };

            return (requestUrl, requestBody, queryParams, customHeaders);
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

            (var requestUrl, var requestBody, var queryParams, var customHeaders) = UpdateWorkitemFieldsPrepareAgrs(
                workitemId, itemFields, expand, bypassRules, suppressNotifications, validateOnly);

            try
            {
                var response = _httpService.PatchJson(requestUrl, requestBody,
                    queryParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch(Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: UpdateWOrkitemsFields exception", ex);
            }
        }

        public async Task<ITfsWorkitem> UpdateWorkitemFieldsAsync(int workitemId, IReadOnlyDictionary<string, string> itemFields,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false)
        {
            if (itemFields == null)
            {
                throw new ArgumentNullException("itemFields");
            }

            (var requestUrl, var requestBody, var queryParams, var customHeaders) = UpdateWorkitemFieldsPrepareAgrs(
                workitemId, itemFields, expand, bypassRules, suppressNotifications, validateOnly);

            try
            {
                var response = await _httpService.PatchJsonAsync(requestUrl, requestBody,
                    queryParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: UpdateWOrkitemsFieldsAsync exception", ex); ;
            }
        }

        private (string, IReadOnlyDictionary<string, string>, object, IReadOnlyDictionary<string, string>)
            AddRelationLinkPrepareArgs(int sourceWorkitemId, int destinationWorkitemId,
            string relationType, IReadOnlyDictionary<string, string> relationAttributes,
            string expand, bool bypassRules,
            bool suppressNotifications, bool validateOnly)
        {
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
                        url = $"{ServerUrl}{_tfsUrl}{WORKITEM_URL}/{destinationWorkitemId}",
                        attributes = relationAttributes
                    }
                }
            };

            var customHeaders = new Dictionary<string, string>()
            {
                { "Content-Type", "application/json-patch+json" }
            };

            return (requestUrl, queryParams, requestBody, customHeaders);
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

            (var requestUrl, var queryParams, var requestBody, var customHeaders) = AddRelationLinkPrepareArgs(
                sourceWorkitemId, destinationWorkitemId, relationType, relationAttributes,
                expand, bypassRules, suppressNotifications, validateOnly);

            try
            {
                var response = _httpService.PatchJson(requestUrl, requestBody,
                    queryParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: AddRelationLink exception", ex);
            }
        }

        public async Task<ITfsWorkitem> AddRelationLinkAsync(
            int sourceWorkitemId, int destinationWorkitemId,
            string relationType, IReadOnlyDictionary<string, string> relationAttributes = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false)
        {
            if ((relationType == null) || (relationType.Trim() == ""))
            {
                throw new ArgumentNullException("relationType", "parametr is null or empty");
            }

            (var requestUrl, var queryParams, var requestBody, var customHeaders) = AddRelationLinkPrepareArgs(
                sourceWorkitemId, destinationWorkitemId, relationType, relationAttributes,
                expand, bypassRules, suppressNotifications, validateOnly);

            try
            {
                var response = await _httpService.PatchJsonAsync(requestUrl, requestBody,
                    queryParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: AddRelationLinkAsync exception", ex); ;
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

        public async Task<ITfsWorkitem> AddRelationLinkAsync(
            int sourceWorkitemId, int destinationWorkitemId,
            WorkitemRelationType relationType, IReadOnlyDictionary<string, string> relationAttributes = null)
        {
            if (!TfsWorkitemFactory.RELATION_TYPE_MAP.TryGetValue(relationType, out string relTypeName))
            {
                throw new ArgumentException("relationType must not be unknown", "relationType");
            }

            return await AddRelationLinkAsync(sourceWorkitemId, destinationWorkitemId, relTypeName, relationAttributes);
        }

        private (string, IReadOnlyDictionary<string, string>, object, IReadOnlyDictionary<string, string>)
            RemoveRelationLinkPrepareArgs(int workitemId, int relationId,
            string expand, bool bypassRules,
            bool suppressNotifications, bool validateOnly)
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

            return (requestUrl, queryParams, requestBody, customHeaders);
        }

        public ITfsWorkitem RemoveRelationLink(
            int workitemId, int relationId,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false)
        {
            (var requestUrl, var queryParams, var requestBody, var customHeaders) = RemoveRelationLinkPrepareArgs(
                workitemId, relationId, expand, bypassRules, suppressNotifications, validateOnly);

            try
            {
                var response = _httpService.PatchJson(requestUrl, requestBody,
                    queryParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: RemoveRelationLink exception", ex);
            }
        }

        public async Task<ITfsWorkitem> RemoveRelationLinkAsync(
            int workitemId, int relationId,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false)
        {
            (var requestUrl, var queryParams, var requestBody, var customHeaders) = RemoveRelationLinkPrepareArgs(
                workitemId, relationId, expand, bypassRules, suppressNotifications, validateOnly);

            try
            {
                var response = await _httpService.PatchJsonAsync(requestUrl, requestBody,
                    queryParams: queryParams, customHeaders: customHeaders);

                return response.IsSuccess
                    ? TfsWorkitemFactory.FromJsonItem(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: RemoveRelationLinkAsync exception", ex); ;
            }
        }

        private (string, IReadOnlyDictionary<string, string>) RunSavedQueryPrepareArgs(string queryId)
        {
            var requestUrl = $"{_tfsUrlPrj}{QUERY_URL}/{queryId}";

            var queryParams = new Dictionary<string, string>()
            {
                { "api-version", API_VERSION },
                { "$expand", "clauses" }
            };

            return (requestUrl, queryParams);
        }

        public ITfsWiqlResult RunSavedQuery(string queryId)
        {
            if (queryId == null)
            {
                throw new ArgumentException("param can't be null", "queryId");
            }

            (var requestUrl, var queryParams) = RunSavedQueryPrepareArgs(queryId);

            try
            {
                var response = _httpService.Get(requestUrl, queryParams);

                return response.IsSuccess
                    ? TfsWiqlFactory.FromQueryResponse(this, response.Content)
                    : null;
            }
            catch(Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: RunSavedQuery exception", ex);
            }
        }

        public async Task<ITfsWiqlResult> RunSavedQueryAsync(string queryId)
        {
            if (queryId == null)
            {
                throw new ArgumentException("param can't be null", "queryId");
            }

            (var requestUrl, var queryParams) = RunSavedQueryPrepareArgs(queryId);

            try
            {
                var response = await _httpService.GetAsync(requestUrl, queryParams);

                return response.IsSuccess
                    ? TfsWiqlFactory.FromQueryResponse(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: RunSavedQueryAsync exception", ex); ;
            }
        }

        private (string, object, IReadOnlyDictionary<string, string>) RunWiqlPrepareArgs(string query, int maxTop)
        {
            var requestUrl = $"{_tfsUrlPrj}{WIQL_URL}";

            var requestBody = new
            {
                query = query
            };

            var queryParams = new Dictionary<string, string>()
            {
                { "api-version", API_VERSION }
            };

            if (maxTop > 0)
            {
                queryParams.Add("$top", maxTop.ToString());
            }

            return (requestUrl, requestBody, queryParams);
        }

        public ITfsWiqlResult RunWiql(string query, int maxTop = -1)
        {
            if(query == null)
            {
                throw new ArgumentException("param can't be null", "query");
            }

            (var requestUrl, var requestBody, var queryParams) = RunWiqlPrepareArgs(query, maxTop);

            try
            {
                var response = _httpService.PostJson(requestUrl, requestBody, 
                    queryParams: queryParams);

                return response.IsSuccess
                    ? TfsWiqlFactory.FromContentResponse(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: RunWiql exception", ex);
            }
        }

        public async Task<ITfsWiqlResult> RunWiqlAsync(string query, int maxTop = -1)
        {
            if (query == null)
            {
                throw new ArgumentException("param can't be null", "query");
            }

            (var requestUrl, var requestBody, var queryParams) = RunWiqlPrepareArgs(query, maxTop);

            try
            {
                var response = await _httpService.PostJsonAsync(requestUrl, requestBody, 
                    queryParams: queryParams);

                return response.IsSuccess
                    ? TfsWiqlFactory.FromContentResponse(this, response.Content)
                    : null;
            }
            catch (Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: RunWiqlAsync exception", ex);
            }
        }

        public ITfsChangeset GetChangeset(int changesetId)
        {
            var queryParams = new Dictionary<string, string>()
            {
                { "api-version", API_VERSION },
                { "includeWorkItems", "true" }
            };

            var requestUrl = $"{_tfsUrlPrj}{WIQL_URL}/{changesetId}";

            try
            {
                var response = _httpService.Get(requestUrl, queryParams: queryParams);

                return response.IsSuccess
                    ? TfsChangesetFactory.FromResponse(this, response.Content)
                    : null;
            }
            catch(Exception ex)
            {
                throw new TfsServiceClientException("TfsServiceClient: GetChangeset exception", ex);
            }
        }
    }
}
