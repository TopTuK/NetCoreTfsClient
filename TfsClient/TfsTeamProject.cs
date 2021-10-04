using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsClient
{
    internal static class TfsTeamProjectFactory
    {
        private class TfsTeamProject : ITfsTeamProject
        {
            public string Id { get; }
            public string Name { get; }
            public string Description { get; }

            public TfsTeamProject(JToken jsonItem)
            {
                Id = jsonItem["id"]?.Value<string>();
                Name = jsonItem["name"]?.Value<string>();
                Description = jsonItem["description"]?.Value<string>();
            }
        }

        public static IEnumerable<ITfsTeamProject> FromJsonItems(string jsonItemsStr)
        {
            IEnumerable<ITfsTeamProject> items = null;
            try
            {
                var jsonItems = JObject.Parse(jsonItemsStr);
                if (jsonItems.ContainsKey("count") && jsonItems.ContainsKey("value"))
                {
                    var itemsCount = jsonItems["count"].Value<int>();
                    if (itemsCount > 0)
                    {
                        items = jsonItems["value"]
                            .Select(jTeamProject => new TfsTeamProject(jTeamProject))
                            .ToList();
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
