using System;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    public interface ITfsTeam
    {
        /// <summary>
        /// Team (Identity) Guid. A Team Foundation ID. 
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Team name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Team description
        /// </summary>
        string Description { get; }
    }

    public interface ITfsTeamMember
    {
        /// <summary>
        /// Relevant Tfs Team
        /// </summary>
        ITfsTeam Team { get; }

        /// <summary>
        /// Flag if member is team admin
        /// </summary>
        bool IsTeamAdmin { get; }

        /// <summary>
        /// Member ID.
        /// This ID is used for mentions
        /// </summary>
        string Id { get; }

        /// <summary>
        /// This is the non-unique display name of the graph subject.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// This url is the full route to the source resource of this graph subject.
        /// </summary>
        string Url { get; }
    }
}
