using System;
using System.Collections.Generic;
using System.Text;

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
            bool suppressNotifications = false, bool validateOnly = false)
        {
            throw new NotImplementedException();
        }

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
    }
}
