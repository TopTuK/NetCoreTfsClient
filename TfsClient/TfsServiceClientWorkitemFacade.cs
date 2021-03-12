using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TfsClient
{
    internal class TfsServiceClientWorkitemFacade : ITfsServiceClientWorkitemFacade
    {
        private ITfsServiceClient _tfsService;

        public TfsServiceClientWorkitemFacade(ITfsServiceClient tfsService)
        {
            _tfsService = tfsService;
        }

        public ITfsWorkitem AddRelationLink(int sourceWorkitemId, int destinationWorkitemId,
            string relationType, IReadOnlyDictionary<string, string> relationAttributes = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false) =>
            _tfsService.AddRelationLink(sourceWorkitemId, destinationWorkitemId, relationType,
                relationAttributes, expand, bypassRules);

        public ITfsWorkitem AddRelationLink(int sourceWorkitemId, int destinationWorkitemId,
            WorkitemRelationType relationType, IReadOnlyDictionary<string, string> relationAttributes = null) =>
            _tfsService.AddRelationLink(sourceWorkitemId, destinationWorkitemId, relationType, relationAttributes);

        public ITfsWorkitem GetSingleWorkitem(int id, IEnumerable<string> fields = null, string expand = "All") =>
            _tfsService.GetSingleWorkitem(id, fields, expand);

        public IEnumerable<ITfsWorkitem> GetWorkitems(IEnumerable<int> ids, IEnumerable<string> fields = null,
            string expand = "All", int batchSize = 50) =>
            _tfsService.GetWorkitems(ids, fields, expand, batchSize);

        public ITfsWorkitem RemoveRelationLink(int workitemId, int relationId,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false) =>
            _tfsService.RemoveRelationLink(workitemId, relationId, expand, bypassRules, suppressNotifications, validateOnly);

        public ITfsWorkitem UpdateWorkitemFields(int workitemId, IReadOnlyDictionary<string, string> itemFields,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false) =>
            _tfsService.UpdateWorkitemFields(workitemId, itemFields, expand, bypassRules, suppressNotifications, validateOnly);

        public ITfsWorkitem CreateWorkitem(string itemType, IReadOnlyDictionary<string, string> itemFields = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false) =>
            _tfsService.CreateWorkitem(itemType, itemFields, expand, bypassRules, suppressNotifications, validateOnly);
        public ITfsWorkitem CreateWorkitem(WorkItemType itemType, IReadOnlyDictionary<string, string> itemFields = null) =>
            _tfsService.CreateWorkitem(itemType, itemFields);

        public ITfsWorkitem CopyWorkitem(int sourceItemId, IReadOnlyDictionary<string, string> destinationItemFields = null) =>
            _tfsService.CopyWorkitem(sourceItemId, destinationItemFields);
        public ITfsWorkitem CopyWorkitem(ITfsWorkitem sourceItem, IReadOnlyDictionary<string, string> destinationItemFields = null) =>
            _tfsService.CopyWorkitem(sourceItem, destinationItemFields);
    }

    internal class AsyncTfsServiceClientWorkitemFacade : IAsyncTfsServiceClientWorkitemFacade
    {
        private ITfsServiceClient _tfsService;

        public AsyncTfsServiceClientWorkitemFacade(ITfsServiceClient tfsService)
        {
            _tfsService = tfsService;
        }

        public Task<ITfsWorkitem> AddRelationLinkAsync(int sourceWorkitemId, int destinationWorkitemId,
            string relationType, IReadOnlyDictionary<string, string> relationAttributes = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false) =>
            _tfsService.AddRelationLinkAsync(sourceWorkitemId, destinationWorkitemId, relationType, relationAttributes,
                expand, bypassRules, suppressNotifications, validateOnly);

        public Task<ITfsWorkitem> AddRelationLinkAsync(int sourceWorkitemId, int destinationWorkitemId,
            WorkitemRelationType relationType, IReadOnlyDictionary<string, string> relationAttributes = null) =>
            _tfsService.AddRelationLinkAsync(sourceWorkitemId, destinationWorkitemId, relationType, relationAttributes);

        public Task<ITfsWorkitem> CopyWorkitemAsync(int sourceItemId,
            IReadOnlyDictionary<string, string> destinationItemFields = null) =>
            _tfsService.CopyWorkitemAsync(sourceItemId, destinationItemFields);

        public Task<ITfsWorkitem> CopyWorkitemAsync(ITfsWorkitem sourceItem,
            IReadOnlyDictionary<string, string> destinationItemFields = null) =>
            _tfsService.CopyWorkitemAsync(sourceItem, destinationItemFields);

        public Task<ITfsWorkitem> CreateWorkitemAsync(string itemType, IReadOnlyDictionary<string, string> itemFields = null,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false) =>
            _tfsService.CreateWorkitemAsync(itemType, itemFields, expand, bypassRules, suppressNotifications, validateOnly);

        public Task<ITfsWorkitem> CreateWorkitemAsync(WorkItemType itemType,
            IReadOnlyDictionary<string, string> itemFields = null) =>
            _tfsService.CreateWorkitemAsync(itemType, itemFields);

        public Task<ITfsWorkitem> GetSingleWorkitemAsync(int id, IEnumerable<string> fields = null, string expand = "All") =>
            _tfsService.GetSingleWorkitemAsync(id, fields, expand);

        public Task<IEnumerable<ITfsWorkitem>> GetWorkitemsAsync(IEnumerable<int> ids, IEnumerable<string> fields = null,
            string expand = "All", int batchSize = 50) =>
            _tfsService.GetWorkitemsAsync(ids, fields, expand, batchSize);

        public Task<ITfsWorkitem> RemoveRelationLinkAsync(int workitemId, int relationId,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false) =>
            _tfsService.RemoveRelationLinkAsync(workitemId, relationId, expand, bypassRules, suppressNotifications, validateOnly);

        public Task<ITfsWorkitem> UpdateWorkitemFieldsAsync(int workitemId, IReadOnlyDictionary<string, string> itemFields,
            string expand = "All", bool bypassRules = false,
            bool suppressNotifications = false, bool validateOnly = false) =>
            _tfsService.UpdateWorkitemFieldsAsync(workitemId, itemFields, expand, bypassRules, suppressNotifications, validateOnly);
    }
}
