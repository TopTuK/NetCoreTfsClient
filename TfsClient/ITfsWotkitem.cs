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

    public interface ITfsWorkitem
    {
        WorkItemType ItemType { get; }
        string ItemTypeName { get; }
        string Url { get; }
        int Id { get; }

        IDictionary<string, string> Fields { get; }

        TfsItemUpdateResult Save();
    }
}
