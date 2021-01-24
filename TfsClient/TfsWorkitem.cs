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

        private class TfsWorkitemRelation : ITfsWorkitemRelation
        {
            public WorkitemRelationType RelationType { get; }
            public string RelationTypeName { get; }
            public string Url { get; }

            public TfsWorkitemRelation(JToken jsonRelation)
            {
                Url = jsonRelation["url"].Value<string>();
                RelationTypeName = jsonRelation["rel"].Value<string>();

                switch(RelationTypeName)
                {
                    case "System.LinkTypes.Hierarchy-Reverse":
                        RelationType = WorkitemRelationType.Parent;
                        break;
                    case "System.LinkTypes.Hierarchy-Forward":
                        RelationType = WorkitemRelationType.Child;
                        break;
                    case "Microsoft.VSTS.Common.Affects-Forward":
                        RelationType = WorkitemRelationType.Affects;
                        break;
                    case "Microsoft.VSTS.Common.Affects-Reverse":
                        RelationType = WorkitemRelationType.AffectedBy;
                        break;
                    case "System.LinkTypes.Related":
                        RelationType = WorkitemRelationType.Related;
                        break;
                    default:
                        RelationType = WorkitemRelationType.Unknown;
                        break;
                }
            }
        }

        private class TfsWorkitem : ITfsWorkitem
        {
            private ITfsServiceClient _tfsServiceClient;
            private Dictionary<string, JToken> _fields = new Dictionary<string, JToken>();
            private List<ITfsWorkitemRelation> _relations = new List<ITfsWorkitemRelation>();

            public WorkItemType ItemType { get; }
            public string ItemTypeName { get; }
            public string Url { get; }
            public int Id { get; }
            public IReadOnlyCollection<ITfsWorkitemRelation> Relations => _relations;

            public string this[string fieldName] 
            {
                get
                {
                    if (_fields.TryGetValue(fieldName, out JToken jField))
                    {
                        return jField.Type == JTokenType.String
                            ? jField.ToObject<string>()
                            : jField.ToString();
                    }

                    return null;
                } 
                set
                {
                    var jField = new JObject(value);
                    if(_fields.ContainsKey(fieldName))
                    {
                        _fields[fieldName] = jField;
                    }
                    else
                    {
                        _fields.Add(fieldName, jField);
                    }
                }
            }

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
                        else
                        {
                            if (fieldKey != "System.Id")
                            {
                                _fields.Add(fieldKey, field.Value);
                            }
                        }
                    }
                }

                var jsonRelations = jsonItem["relations"]?.ToObject<JArray>();
                if((jsonRelations != null) && (jsonRelations.HasValues))
                {
                    foreach(var jsonRelation in jsonRelations)
                    {
                        _relations.Add(new TfsWorkitemRelation(jsonRelation));
                    }
                }
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
