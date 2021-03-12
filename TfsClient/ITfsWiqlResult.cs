using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TfsClient
{
    public interface ITfsWiqlResult
    {
        bool IsEmpty { get; }
        int Count { get; }
        IEnumerable<int> ItemIds { get; }
        IEnumerable<ITfsWorkitem> GetWorkitems();
        Task<IEnumerable<ITfsWorkitem>> GetWorkitemsAsync();
    }
}
