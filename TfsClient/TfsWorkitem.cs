using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsClient
{
    internal static class TfsWorkitemFactory
    {
        private static readonly Dictionary<string, WorkItemType> WorkItemTypeMap = new Dictionary<string, WorkItemType>()
        {
            { "Requirement", WorkItemType.Requirement },
            { "Change request", WorkItemType.ChangeRequest },
            { "Task", WorkItemType.Task },
            { "Bug", WorkItemType.Bug }
        };

        private class TfsWorkitem : ITfsWorkitem
        {
            private ITfsServiceClient _tfsServiceClient;
            public WorkItemType ItemType { get; }
            public string ItemTypeName { get; }
            public string Url { get; }
            public int Id { get; }
            public IDictionary<string, string> Fields { get; } = new Dictionary<string, string>();

            public TfsWorkitem(ITfsServiceClient tfsServiceClient, JToken jsonItem)
            {
                ItemType = WorkItemType.Unknown;

                _tfsServiceClient = tfsServiceClient;

                Url = jsonItem["url"].Value<string>();
                Id = jsonItem["id"].Value<int>();

                var jsonFields = jsonItem["fields"]?.ToObject<JObject>();
                if((jsonFields != null) && (jsonFields.HasValues))
                {
                    foreach(var field in jsonFields)
                    {
                        var fieldKey = field.Key;
                        if (fieldKey == "System.WorkItemType")
                        {
                            ItemTypeName = field.Value.Value<string>();

                            if(WorkItemTypeMap.ContainsKey(ItemTypeName))
                            {
                                ItemType = WorkItemTypeMap[ItemTypeName];
                            }
                        }
                        
                        if (fieldKey != "System.Id") Fields.Add(field.Key, field.Value?.ToString());
                    }
                }
            }

            public TfsItemUpdateResult Save()
            {
                throw new NotImplementedException();
            }
        }

        public static IEnumerable<ITfsWorkitem> FromJson(ITfsServiceClient tfsService, string jsonItemsStr)
        {
            List<ITfsWorkitem> items = null;

            var jsonResult = JObject.Parse(jsonItemsStr);
            if(jsonResult.ContainsKey("value"))
            {
                var jsonItems = jsonResult["value"].ToObject<JArray>();
                items = new List<ITfsWorkitem>(jsonItems.Count);

                foreach(var jsonItem in jsonItems)
                {
                    items.Add(new TfsWorkitem(tfsService, jsonItem));
                }
            }

            return items;
        }
    }
}
