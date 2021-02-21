using System;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    public interface ITfsChangeset
    {
        int ChangesetId { get; }
        IEnumerable<ITfsWorkitem> GetAssociatedWorkItem();
    }
}
