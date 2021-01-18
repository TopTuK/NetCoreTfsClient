using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    internal static class TfsWorkitemFactory
    {
        private static readonly IReadOnlyDictionary<string, WorkItemType> WorkItemTypeMap = new Dictionary<string, WorkItemType>()
        {

        };

        private class TfsWorkitem : ITfsWorkitem
        {
            public WorkItemType ItemType { get; }
            public int Id { get; }
            public IReadOnlyDictionary<string, string> Fields { get; }
            public string Url { get; }

            public TfsWorkitem(JToken jsonItem)
            {
                Id = jsonItem["id"].ToObject<int>();
                Url = jsonItem["url"].ToObject<string>();
            }
        }

        public static IEnumerable<ITfsWorkitem> FromJsonItems(string jsonItems)
        {
            List<ITfsWorkitem> items = null;

            var jsonObj = JObject.Parse(jsonItems);
            if(jsonObj.ContainsKey("value"))
            {
                int itemsCount = jsonObj["count"].ToObject<int>();

                items = new List<ITfsWorkitem>(itemsCount);
                foreach(var jsonItem in jsonObj["value"])
                {
                    items.Add(new TfsWorkitem(jsonItem));
                }
            }

            return items;
        }

        public static ITfsWorkitem CreateNewItem()
        {
            throw new NotImplementedException();
        }
    }
}
