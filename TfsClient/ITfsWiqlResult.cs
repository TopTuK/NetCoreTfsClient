using System;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    public interface ITfsWiqlResult
    {
        bool IsEmpty { get; }
        int Count { get; }
        IEnumerable<int> ItemIds { get; }
        IEnumerable<ITfsWorkitem> GetWorkitems();
    }
}
