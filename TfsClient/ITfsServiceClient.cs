using System;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    public enum WorkItemType
    {
        Requirement = 0,
        ChangeRequest,
        Bug
    };

    public interface ITfsWorkitem
    {
        WorkItemType ItemType { get; }
        int Id { get; }
        string Title { get; }
        string Description { get; }

        IReadOnlyDictionary<string, string> Fields { get; }
    }

    public interface ITfsServiceClient
    {
        string ServerUrl { get; }
        string Collection { get; }
        string Project { get; }

        void Authentificate(string userName, string userPassword);

        ITfsWorkitem GetSingleWorkitem(int id, 
            IEnumerable<string> fields = null);
        IEnumerable<ITfsWorkitem> GetWorkitems(IEnumerable<int> ids,
            IEnumerable<string> fields = null, int batchSize = 50);
    }
}
