using System;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    public interface ITfsServiceClientWorkitemFacade
    {
        // Get Workitems
        ITfsWorkitem GetSingleWorkitem(int id,
            IEnumerable<string> fields = null, string expand = "All");
        IEnumerable<ITfsWorkitem> GetWorkitems(IEnumerable<int> ids,
            IEnumerable<string> fields = null, string expand = "All", int batchSize = 50);

        // Create Workitems
        ITfsWorkitem CreateWorkitem(string itemType, IReadOnlyDictionary<string, string> itemFields = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        ITfsWorkitem CreateWorkitem(WorkItemType itemType, IReadOnlyDictionary<string, string> itemFields = null);

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

    public interface ITfsServiceClient
    {
        string ServerUrl { get; }
        string Collection { get; }
        string Project { get; }

        void Authentificate(string userName, string userPassword);
        void Authentificate(string personalAccessToken);

        ITfsWorkitem GetSingleWorkitem(int id, 
            IEnumerable<string> fields = null, string expand = "All");
        IEnumerable<ITfsWorkitem> GetWorkitems(IEnumerable<int> ids,
            IEnumerable<string> fields = null, string expand = "All", int batchSize = 50);

        ITfsWorkitem CreateWorkitem(string itemType, IReadOnlyDictionary<string, string> itemFields = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false);
        ITfsWorkitem CreateWorkitem(WorkItemType itemType, IReadOnlyDictionary<string, string> itemFields = null);

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

        ITfsWiqlResult RunSavedQuery(string queryId);
        ITfsWiqlResult RunWiql(string query, int maxTop = -1);
    }
}
