using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TfsClient
{
    /// <summary>
    /// The key interface for interacting with TFS
    /// Note: to interact with TFS you must set authentication once
    /// </summary>
    public interface ITfsServiceClient
    {
        /// <summary>
        /// Get TFS Server URL
        /// </summary>
        /// <example>For example: "https://my-tfs/tfs/"</example>
        string ServerUrl { get; }

        /// <summary>
        /// Get TFS Collection
        /// </summary>
        /// <example>
        /// For example: if you set project name as DefaultCollection/MyProject this property return DefaultCollection
        /// </example>
        string Collection { get; }

        /// <summary>
        /// Get Project Name
        /// </summary>
        /// <example>
        /// For example: if you set project name as DefaultCollection/MyProject this property return MyProject
        /// </example>
        string Project { get; }

        /// <summary>
        /// Set authetification with NTLM
        /// </summary>
        /// <param name="userName">User name (with domain prefix)</param>
        /// <param name="userPassword">User password</param>
        void Authentificate(string userName, string userPassword);

        /// <summary>
        /// Set authentification with personal access token
        /// https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page
        /// </summary>
        /// <param name="personalAccessToken">Personal access token</param>
        void Authentificate(string personalAccessToken);

        /// <summary>
        /// Get single workitem by ID
        /// https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/work%20items/get%20work%20item?view=azure-devops-rest-6.0
        /// </summary>
        /// <param name="id">ID of workitem</param>
        /// <param name="fields">List of requested fields</param>
        /// <param name="expand">The expand parameters for work item attributes. Possible options are { None, Relations, Fields, Links, All }.</param>
        /// <returns>Return single wokitem with properties</returns>
        /// <exception cref="TfsServiceClientException"></exception>
        ITfsWorkitem GetSingleWorkitem(int id, 
            IEnumerable<string> fields = null, string expand = "All");

        /// <summary>
        /// Async get single workitem
        /// </summary>
        /// <param name="id">ID of workitem</param>
        /// <param name="fields">List of requested fields</param>
        /// <param name="expand">The expand parameters for work item attributes. Possible options are { None, Relations, Fields, Links, All }</param>
        /// <returns>Return single wokitem with properties</returns>
        /// <exception cref="TfsServiceClientException"></exception>
        Task<ITfsWorkitem> GetSingleWorkitemAsync(int id, IEnumerable<string> fields = null, string expand = "All");

        /// <summary>
        /// Get workitems with properties
        /// </summary>
        /// <param name="ids">List of id of workitems</param>
        /// <param name="fields">List of requested fields for each workitem</param>
        /// <param name="expand">The expand parameters for work item attributes. Possible options are { None, Relations, Fields, Links, All }</param>
        /// <param name="batchSize">Number of requested items per iteration</param>
        /// <returns>Enumerable list of workitems</returns>
        /// <exception cref="TfsServiceClientException"></exception>
        IEnumerable<ITfsWorkitem> GetWorkitems(IEnumerable<int> ids,
            IEnumerable<string> fields = null, string expand = "All", int batchSize = 50);

        /// <summary>
        /// Async get workitems with properties
        /// </summary>
        /// <param name="ids">List of id of workitems</param>
        /// <param name="fields">List of requested fields for each workitem</param>
        /// <param name="expand">The expand parameters for work item attributes. Possible options are { None, Relations, Fields, Links, All }</param>
        /// <param name="batchSize">Number of requested items per iteration</param>
        /// <returns>Enumerable list of workitems</returns>
        /// <exception cref="TfsServiceClientException"></exception>
        Task<IEnumerable<ITfsWorkitem>> GetWorkitemsAsync(IEnumerable<int> ids,
            IEnumerable<string> fields = null, string expand = "All", int batchSize = 50);

        ITfsWorkitem CreateWorkitem(string itemType, 
            IReadOnlyDictionary<string, string> itemFields = null, IEnumerable<ITfsWorkitemRelation> itemRelations = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        Task<ITfsWorkitem> CreateWorkitemAsync(string itemType, 
            IReadOnlyDictionary<string, string> itemFields = null, IEnumerable<ITfsWorkitemRelation> itemRelations = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        ITfsWorkitem CreateWorkitem(WorkItemType itemType, 
            IReadOnlyDictionary<string, string> itemFields = null,
            IEnumerable<ITfsWorkitemRelation> itemRelations = null);
        Task<ITfsWorkitem> CreateWorkitemAsync(
            WorkItemType itemType, 
            IReadOnlyDictionary<string, string> itemFields = null,
            IEnumerable<ITfsWorkitemRelation> itemRelations = null);

        ITfsWorkitem CopyWorkitem(int sourceItemId, IReadOnlyDictionary<string, string> destinationItemFields = null);
        Task<ITfsWorkitem> CopyWorkitemAsync(int sourceItemId,
            IReadOnlyDictionary<string, string> destinationItemFields = null);
        ITfsWorkitem CopyWorkitem(ITfsWorkitem sourceItem, IReadOnlyDictionary<string, string> destinationItemFields = null);
        Task<ITfsWorkitem> CopyWorkitemAsync(ITfsWorkitem sourceItem,
            IReadOnlyDictionary<string, string> destinationItemFields = null);

        ITfsWorkitem UpdateWorkitemFields(int workitemId, IReadOnlyDictionary<string, string> itemFields,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        Task<ITfsWorkitem> UpdateWorkitemFieldsAsync(int workitemId, IReadOnlyDictionary<string, string> itemFields,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);

        ITfsWorkitem AddRelationLink(
            int sourceWorkitemId, int destinationWorkitemId,
            string relationType, IReadOnlyDictionary<string, string> relationAttributes = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        Task<ITfsWorkitem> AddRelationLinkAsync(
            int sourceWorkitemId, int destinationWorkitemId,
            string relationType, IReadOnlyDictionary<string, string> relationAttributes = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        ITfsWorkitem AddRelationLink(
            int sourceWorkitemId, int destinationWorkitemId,
            WorkitemRelationType relationType, IReadOnlyDictionary<string, string> relationAttributes = null);
        Task<ITfsWorkitem> AddRelationLinkAsync(
            int sourceWorkitemId, int destinationWorkitemId,
            WorkitemRelationType relationType, IReadOnlyDictionary<string, string> relationAttributes = null);

        ITfsWorkitem RemoveRelationLink(
            int workitemId, int relationId,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        Task<ITfsWorkitem> RemoveRelationLinkAsync(
            int workitemId, int relationId,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);

        ITfsWiqlResult RunSavedQuery(string queryId);
        Task<ITfsWiqlResult> RunSavedQueryAsync(string queryId);

        ITfsWiqlResult RunWiql(string query, int maxTop = -1);
        Task<ITfsWiqlResult> RunWiqlAsync(string query, int maxTop = -1);
    }

    public interface ITfsServiceClientWorkitemFacade
    {
        // Get Workitems
        ITfsWorkitem GetSingleWorkitem(int id,
            IEnumerable<string> fields = null, string expand = "All");
        IEnumerable<ITfsWorkitem> GetWorkitems(IEnumerable<int> ids,
            IEnumerable<string> fields = null, string expand = "All", int batchSize = 50);

        // Create Workitems
        ITfsWorkitem CreateWorkitem(string itemType, 
            IReadOnlyDictionary<string, string> itemFields = null, IEnumerable<ITfsWorkitemRelation> itemRelations = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        ITfsWorkitem CreateWorkitem(WorkItemType itemType, 
            IReadOnlyDictionary<string, string> itemFields = null, IEnumerable<ITfsWorkitemRelation> itemRelations = null);

        ITfsWorkitem CopyWorkitem(int sourceItemId, IReadOnlyDictionary<string, string> destinationItemFields = null);
        ITfsWorkitem CopyWorkitem(ITfsWorkitem sourceItem, IReadOnlyDictionary<string, string> destinationItemFields = null);

        ITfsWorkitem UpdateWorkitemFields(int workitemId, IReadOnlyDictionary<string, string> itemFields,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);

        ITfsWorkitem AddRelationLink(
            int sourceWorkitemId, int destinationWorkitemId,
            string relationType, IReadOnlyDictionary<string, string> relationAttributes = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        ITfsWorkitem AddRelationLink(
            int sourceWorkitemId, int destinationWorkitemId,
            WorkitemRelationType relationType, IReadOnlyDictionary<string, string> relationAttributes = null);

        ITfsWorkitem RemoveRelationLink(
            int workitemId, int relationId,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
    }

    public interface IAsyncTfsServiceClientWorkitemFacade
    {
        // Get Workitems
        Task<ITfsWorkitem> GetSingleWorkitemAsync(int id,
            IEnumerable<string> fields = null, string expand = "All");
        Task<IEnumerable<ITfsWorkitem>> GetWorkitemsAsync(IEnumerable<int> ids,
            IEnumerable<string> fields = null, string expand = "All", int batchSize = 50);

        // Create Workitems
        Task<ITfsWorkitem> CreateWorkitemAsync(string itemType,
            IReadOnlyDictionary<string, string> itemFields = null, IEnumerable<ITfsWorkitemRelation> itemRelations = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        Task<ITfsWorkitem> CreateWorkitemAsync(WorkItemType itemType, 
            IReadOnlyDictionary<string, string> itemFields = null, IEnumerable<ITfsWorkitemRelation> itemRelations = null);

        Task<ITfsWorkitem> CopyWorkitemAsync(int sourceItemId, IReadOnlyDictionary<string, string> destinationItemFields = null);
        Task<ITfsWorkitem> CopyWorkitemAsync(ITfsWorkitem sourceItem, IReadOnlyDictionary<string, string> destinationItemFields = null);

        Task<ITfsWorkitem> UpdateWorkitemFieldsAsync(int workitemId, IReadOnlyDictionary<string, string> itemFields,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);

        Task<ITfsWorkitem> AddRelationLinkAsync(
            int sourceWorkitemId, int destinationWorkitemId,
            string relationType, IReadOnlyDictionary<string, string> relationAttributes = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        Task<ITfsWorkitem> AddRelationLinkAsync(
            int sourceWorkitemId, int destinationWorkitemId,
            WorkitemRelationType relationType, IReadOnlyDictionary<string, string> relationAttributes = null);

        Task<ITfsWorkitem> RemoveRelationLinkAsync(
            int workitemId, int relationId,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
    }
}
