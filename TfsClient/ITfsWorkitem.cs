using System;
using System.Collections.Generic;
using System.Text;

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

    public interface ITfsWorkitemRelation
    {
        WorkitemRelationType RelationType { get; }
        string RelationTypeName { get; }
        string Url { get; }
    }

    public interface ITfsWorkitem
    {
        WorkItemType ItemType { get; }
        string ItemTypeName { get; }
        string Url { get; }
        int Id { get; }

        string this[string fieldName] { get; set; }

        IReadOnlyCollection<ITfsWorkitemRelation> Relations { get; }
    }
}
