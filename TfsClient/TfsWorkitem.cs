using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    internal static class TfsWorkitemFactory
    {
        private class TfsWorkitem : ITfsWorkitem
        {
            public WorkItemType ItemType { get; }
            public int Id => throw new NotImplementedException();
            public string Title => throw new NotImplementedException();
            public string Description => throw new NotImplementedException();
            public IReadOnlyDictionary<string, string> Fields => throw new NotImplementedException();
            public string Url => throw new NotImplementedException();

            public TfsWorkitem(JToken jsonItem)
            {

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
