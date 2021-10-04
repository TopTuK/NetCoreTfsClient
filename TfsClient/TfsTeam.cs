using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsClient
{
    internal static class TfsTeamFactory
    {
        private class TfsTeam : ITfsTeam
        {
            public string Id { get; }
            public string Name { get; }
            public string Description { get; }

            public TfsTeam(JToken jsonTeam)
            {
                Id = jsonTeam["id"]?.Value<string>();
                Name = jsonTeam["name"]?.Value<string>();
                Description = jsonTeam["description"]?.Value<string>();
            }
        }

        public static IEnumerable<ITfsTeam> FromJsonItems(string jsonItemsStr)
        {
            IEnumerable<ITfsTeam> teams = null;
            try
            {
                var jsonItems = JObject.Parse(jsonItemsStr);
                if (jsonItems.ContainsKey("count") && jsonItems.ContainsKey("value"))
                {
                    var itemsCount = jsonItems["count"].Value<int>();
                    if (itemsCount > 0)
                    {
                        teams = jsonItems["value"]
                            .Select(jTeam => new TfsTeam(jTeam))
                            .ToList();
                    }
                }
                
                return teams;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private class TfsTeamMember : ITfsTeamMember
        {
            public bool IsTeamAdmin { get; }
            public string Id { get; }
            public string DisplayName { get; }
            public string Url { get; }
            public ITfsTeam Team { get; }

            public TfsTeamMember(JToken jsonMember, ITfsTeam team)
            {
                Team = team;

                IsTeamAdmin = jsonMember["isTeamAdmin"]?.Value<bool>() ?? false;

                if (jsonMember["identity"] != null)
                {
                    var identity = jsonMember["identity"];
                    Id = identity["id"]?.Value<string>();
                    DisplayName = identity["displayName"].Value<string>();
                    Url = identity["url"].Value<string>();
                }
                else
                {
                    DisplayName = "Unknown member";
                }
            }
        }

        public static IEnumerable<ITfsTeamMember> FromJsonTeamMembers(ITfsTeam team, string jsonTeamMembersStr)
        {
            IEnumerable<ITfsTeamMember> teamMembers = null;

            try
            {
                var jsonMembers = JObject.Parse(jsonTeamMembersStr);
                if (jsonMembers.ContainsKey("count") && jsonMembers.ContainsKey("value"))
                {
                    var itemsCount = jsonMembers["count"].Value<int>();
                    if (itemsCount > 0)
                    {
                        teamMembers = jsonMembers["value"]
                            .Select(jTeamMember => new TfsTeamMember(jTeamMember, team))
                            .ToList();
                    }
                }

                return teamMembers;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
