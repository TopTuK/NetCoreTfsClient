using System;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    public interface ITfsTeamProject
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
    }
}
