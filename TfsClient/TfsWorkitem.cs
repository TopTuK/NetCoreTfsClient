using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsClient
{
    internal static class TfsWorkitemFactory
    {
        private static readonly IReadOnlyDictionary<string, WorkItemType> WorkItemTypeMap = new Dictionary<string, WorkItemType>()
        {
            { "Requirement", WorkItemType.Requirement },
            { "Change request", WorkItemType.ChangeRequest },
            { "Task", WorkItemType.Task },
            { "Bug", WorkItemType.Bug }
        };

        public static readonly IReadOnlyDictionary<WorkitemRelationType, string> RELATION_TYPE_MAP = new Dictionary<WorkitemRelationType, string>()
        {
            { WorkitemRelationType.AffectedBy, "Microsoft.VSTS.Common.Affects-Reverse" },
            { WorkitemRelationType.Affects, "Microsoft.VSTS.Common.Affects-Forward" },
            { WorkitemRelationType.Child, "System.LinkTypes.Hierarchy-Forward" },
            { WorkitemRelationType.Parent, "System.LinkTypes.Hierarchy-Reverse" },
            { WorkitemRelationType.Related, "System.LinkTypes.Related" },
        };

        private class TfsWorkitemRelation : ITfsWorkitemRelation
        {
            private static readonly string WORKITEM_SUBSTR = "workItems/";
            private static readonly int WORKITEM_SUBSTR_LENGTH = WORKITEM_SUBSTR.Length;

            public WorkitemRelationType RelationType { get; }
            public string RelationTypeName { get; }
            public string Url { get; }
            public int WorkitemId { get; }

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

                WorkitemId = -1;

                var idStartIdx = Url.IndexOf(WORKITEM_SUBSTR);
                if(idStartIdx > 1)
                {
                    int id;
                    if(int.TryParse(Url.Substring(idStartIdx + WORKITEM_SUBSTR_LENGTH), out id))
                    {
                        WorkitemId = id;
                    }
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
            public IReadOnlyList<ITfsWorkitemRelation> Relations => _relations;

            public IReadOnlyCollection<string> FieldNames => _fields.Keys;

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

            public UpdateFieldsResult UpdateFields()
            {
                var fields = _fields
                    .Select(fld => fld)
                    .ToDictionary(fld => fld.Key, fld => this[fld.Key]);

                try
                {
                    var item = _tfsServiceClient.UpdateWorkitemFields(Id, fields, expand: "fields") as TfsWorkitem;
                    if (item == null) return UpdateFieldsResult.UPDATE_FAIL;

                    _fields = item._fields;

                    return UpdateFieldsResult.UPDATE_SUCCESS;
                }
                catch
                {
                    return UpdateFieldsResult.UPDATE_FAIL;
                }
            }

            public UpdateRelationsResult AddRelationLink(
                int destinationWorkitemId, WorkitemRelationType relationType,
                IReadOnlyDictionary<string, string> relationAttributes = null)
            {
                try
                {
                    var item = _tfsServiceClient.AddRelationLink(Id, destinationWorkitemId, 
                        relationType, relationAttributes) as TfsWorkitem;
                    if (item == null) return UpdateRelationsResult.UPDATE_FAIL;

                    _relations = item._relations;

                    return UpdateRelationsResult.UPDATE_SUCCESS;
                }
                catch
                {
                    return UpdateRelationsResult.UPDATE_FAIL;
                }
                throw new NotImplementedException();
            }
        }

        public static ITfsWorkitem FromJsonItem(ITfsServiceClient tfsService, string jsonItemStr)
        {
            ITfsWorkitem item = null;

            try
            {
                var jsonItem = JObject.Parse(jsonItemStr);
                item = new TfsWorkitem(tfsService, jsonItem);
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return item;
        }

        public static IEnumerable<ITfsWorkitem> FromJsonItems(ITfsServiceClient tfsService, string jsonItemsStr)
        {
            List<ITfsWorkitem> items = null;

            try
            {
                var jsonResult = JObject.Parse(jsonItemsStr);
                if (jsonResult.ContainsKey("value"))
                {
                    var jsonItems = jsonResult["value"].ToObject<JArray>();
                    items = new List<ITfsWorkitem>(jsonItems.Count);

                    foreach (var jsonItem in jsonItems)
                    {
                        items.Add(new TfsWorkitem(tfsService, jsonItem));
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return items;
        }
    }
}
