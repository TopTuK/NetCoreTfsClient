using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TfsClient
{
    public enum WorkItemType
    {
        Unknown = 0,
        Requirement,
        ChangeRequest,
        Task,
        Bug
    };

    // https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/work%20item%20relation%20types/list?view=azure-devops-rest-6.0
    public enum WorkitemRelationType
    {
        Unknown = 0,
        Parent, // System.LinkTypes.Hierarchy-Reverse
        Child, // System.LinkTypes.Hierarchy-Forward
        Affects, // Microsoft.VSTS.Common.Affects-Forward
        AffectedBy, // Microsoft.VSTS.Common.Affects-Reverse
        Related // System.LinkTypes.Related
    }

    public enum UpdateFieldsResult: byte
    {
        UPDATE_FAIL = 0,
        UPDATE_EMPTY,
        UPDATE_SUCCESS
    }

    public enum UpdateRelationsResult: byte
    {
        UPDATE_FAIL = 0,
        UPDATE_SUCCESS
    }

    public interface ITfsWorkitemRelation
    {
        WorkitemRelationType RelationType { get; }
        string RelationTypeName { get; }
        string Url { get; }
        int WorkitemId { get; }
    }

    public interface ITfsWorkitem
    {
        WorkItemType ItemType { get; }
        string ItemTypeName { get; }
        string Url { get; }

        int Id { get; }
        string Title { get; }
        string State { get; }
        string Reason { get; }
        string AssignedTo { get; }

        IReadOnlyCollection<string> FieldNames { get; }
        string this[string fieldName] { get; set; }
        UpdateFieldsResult UpdateFields();
        Task<UpdateFieldsResult> UpdateFieldsAsync();

        IReadOnlyList<ITfsWorkitemRelation> Relations { get; }
        UpdateRelationsResult AddRelationLink(int destinationWorkitemId, WorkitemRelationType relationType,
            IReadOnlyDictionary<string, string> relationAttributes = null);
        Task<UpdateRelationsResult> AddRelationLinkAsync(
                int destinationWorkitemId, WorkitemRelationType relationType,
                IReadOnlyDictionary<string, string> relationAttributes = null);
        UpdateRelationsResult RemoveRelationLinks(int destinationWorkitemId);
        Task<UpdateRelationsResult> RemoveRelationLinksAsync(int destinationWorkitemId);
        IEnumerable<ITfsWorkitemRelation> GetWorkitemRelations(WorkitemRelationType relationType);

        IEnumerable<ITfsWorkitem> GetRelatedWorkitems(WorkitemRelationType relationType);
        Task<IEnumerable<ITfsWorkitem>> GetRelatedWorkitemsAsync(WorkitemRelationType relationType);
        IEnumerable<ITfsWorkitem> GetRelatedWorkitems(string relationTypeName);
        Task<IEnumerable<ITfsWorkitem>> GetRelatedWorkitemsAsync(string relationTypeName);
    }
}
