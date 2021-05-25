using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsClient
{
    internal static class TfsWorkitemFactory
    {
        private static readonly IReadOnlyDictionary<string, WorkItemType> WorkItemTypeMap = new Dictionary<string, WorkItemType>()
        {
            { "Requirement", WorkItemType.Requirement },
            { "Change Request", WorkItemType.ChangeRequest },
            { "Task", WorkItemType.Task },
            { "Bug", WorkItemType.Bug }
        };

        public static readonly IReadOnlyDictionary<WorkItemType, string> WI_TYPE_MAP = new Dictionary<WorkItemType, string>()
        {
            { WorkItemType.Requirement, "Requirement" },
            { WorkItemType.ChangeRequest, "Change Request" },
            { WorkItemType.Task, "Task" },
            { WorkItemType.Bug, "Bug" }
        };

        public static readonly IReadOnlyDictionary<WorkitemRelationType, string> RELATION_TYPE_MAP = new Dictionary<WorkitemRelationType, string>()
        {
            { WorkitemRelationType.AffectedBy, "Microsoft.VSTS.Common.Affects-Reverse" },
            { WorkitemRelationType.Affects, "Microsoft.VSTS.Common.Affects-Forward" },
            { WorkitemRelationType.Child, "System.LinkTypes.Hierarchy-Forward" },
            { WorkitemRelationType.Parent, "System.LinkTypes.Hierarchy-Reverse" },
            { WorkitemRelationType.Related, "System.LinkTypes.Related" },
        };

        private static readonly List<string> IGNORE_FIELDS = new List<string>()
        {
            "System.Id"
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
            private Dictionary<string, string> _updatedFields = new Dictionary<string, string>();

            public WorkItemType ItemType { get; }
            public string ItemTypeName { get; }
            public string Url { get; }
            public int Id { get; }
            public IReadOnlyList<ITfsWorkitemRelation> Relations => _relations;

            public IReadOnlyCollection<string> FieldNames => _fields.Keys;

            public string Title
            {
                get
                {
                    if (_updatedFields.ContainsKey("System.Title"))
                    {
                        return _updatedFields["System.Title"];
                    }

                    if (_fields.TryGetValue("System.Title", out var fldValue))
                    {
                        return fldValue.ToString();
                    }

                    return null;
                }
            }

            public string State
            {
                get
                {
                    if (_updatedFields.ContainsKey("System.State"))
                    {
                        return _updatedFields["System.State"];
                    }

                    if (_fields.TryGetValue("System.State", out var fldValue))
                    {
                        return fldValue.ToString();
                    }

                    return null;
                }
            }

            public string Reason
            {
                get
                {
                    if (_updatedFields.ContainsKey("System.Reason"))
                    {
                        return _updatedFields["System.Reason"];
                    }

                    if (_fields.TryGetValue("System.Reason", out var fldValue))
                    {
                        return fldValue.ToString();
                    }

                    return null;
                }
            }

            public string AssignedTo
            {
                get
                {
                    if (_updatedFields.ContainsKey("System.AssignedTo"))
                    {
                        return _updatedFields["System.AssignedTo"];
                    }

                    if (_fields.TryGetValue("System.AssignedTo", out var fldValue))
                    {
                        if (fldValue.Type == JTokenType.String) return fldValue.ToObject<string>();

                        return fldValue.Type == JTokenType.Object
                            ? fldValue["displayName"]?.ToObject<string>()
                            : fldValue.ToString();
                    }

                    return null;
                }
            }

            public string this[string fieldName] 
            {
                get
                {
                    if(_updatedFields.TryGetValue(fieldName, out string fieldValue))
                    {
                        return fieldValue;
                    }

                    if (_fields.TryGetValue(fieldName, out JToken jField))
                    {
                        if (jField.Type == JTokenType.String) return jField.ToObject<string>();

                        return jField.Type == JTokenType.Object
                            ? jField["displayName"]?.ToObject<string>()
                            : jField.ToString();
                    }

                    return null;
                } 
                set
                {
                    if(_updatedFields.ContainsKey(fieldName))
                    {
                        _updatedFields[fieldName] = value;
                    }
                    else
                    {
                        _updatedFields.Add(fieldName, value);
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
                            if (!IGNORE_FIELDS.Contains(fieldKey))
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
                if(_updatedFields.Count == 0)
                {
                    return UpdateFieldsResult.UPDATE_EMPTY;
                }

                try
                {
                    var item = _tfsServiceClient.UpdateWorkitemFields(Id, _updatedFields, expand: "fields") as TfsWorkitem;
                    if (item == null) return UpdateFieldsResult.UPDATE_FAIL;

                    _updatedFields.Clear();
                    _fields = item._fields;

                    return UpdateFieldsResult.UPDATE_SUCCESS;
                }
                catch
                {
                    return UpdateFieldsResult.UPDATE_FAIL;
                }
            }

            public async Task<UpdateFieldsResult> UpdateFieldsAsync()
            {
                if (_updatedFields.Count == 0)
                {
                    return UpdateFieldsResult.UPDATE_EMPTY;
                }

                try
                {
                    var item = await _tfsServiceClient.UpdateWorkitemFieldsAsync(Id, _updatedFields, expand: "fields") as TfsWorkitem;
                    if (item == null) return UpdateFieldsResult.UPDATE_FAIL;

                    _updatedFields.Clear();
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
            }

            public async Task<UpdateRelationsResult> AddRelationLinkAsync(
                int destinationWorkitemId, WorkitemRelationType relationType,
                IReadOnlyDictionary<string, string> relationAttributes = null)
            {
                try
                {
                    var item = await _tfsServiceClient.AddRelationLinkAsync(Id, destinationWorkitemId,
                        relationType, relationAttributes) as TfsWorkitem;
                    if (item == null) return UpdateRelationsResult.UPDATE_FAIL;

                    _relations = item._relations;

                    return UpdateRelationsResult.UPDATE_SUCCESS;
                }
                catch
                {
                    return UpdateRelationsResult.UPDATE_FAIL;
                }
            }

            public UpdateRelationsResult RemoveRelationLinks(int destinationWorkitemId)
            {
                var relIdx = _relations.FindIndex(relation => relation.WorkitemId == destinationWorkitemId);

                if (relIdx < 0) return UpdateRelationsResult.UPDATE_FAIL;

                try
                {
                    var item = _tfsServiceClient.RemoveRelationLink(Id, relIdx, expand: "relations") as TfsWorkitem;
                    if (item == null) return UpdateRelationsResult.UPDATE_FAIL;

                    _relations = item._relations;

                    return UpdateRelationsResult.UPDATE_SUCCESS;
                }
                catch
                {
                    return UpdateRelationsResult.UPDATE_FAIL;
                }
            }

            public async Task<UpdateRelationsResult> RemoveRelationLinksAsync(int destinationWorkitemId)
            {
                var relIdx = _relations.FindIndex(relation => relation.WorkitemId == destinationWorkitemId);

                if (relIdx < 0) return UpdateRelationsResult.UPDATE_FAIL;

                try
                {
                    var item = await _tfsServiceClient.RemoveRelationLinkAsync(Id, relIdx, expand: "relations") as TfsWorkitem;
                    if (item == null) return UpdateRelationsResult.UPDATE_FAIL;

                    _relations = item._relations;

                    return UpdateRelationsResult.UPDATE_SUCCESS;
                }
                catch
                {
                    return UpdateRelationsResult.UPDATE_FAIL;
                }
            }

            public IEnumerable<ITfsWorkitemRelation> GetWorkitemRelations(WorkitemRelationType relationType)
            {
                return _relations
                    .Where(rel => rel.RelationType == relationType)
                    .Select(rel => rel)
                    .ToList();
            }

            public IEnumerable<ITfsWorkitem> GetRelatedWorkitems(WorkitemRelationType relationType)
            {
                if(relationType == WorkitemRelationType.Unknown)
                {
                    throw new ArgumentException("Relation type can't be unknown", "relationType");
                }

                var relationTypeName = RELATION_TYPE_MAP[relationType];
                return GetRelatedWorkitems(relationTypeName);
            }

            public async Task<IEnumerable<ITfsWorkitem>> GetRelatedWorkitemsAsync(WorkitemRelationType relationType)
            {
                if (relationType == WorkitemRelationType.Unknown)
                {
                    throw new ArgumentException("Relation type can't be unknown", "relationType");
                }

                var relationTypeName = RELATION_TYPE_MAP[relationType];
                return await GetRelatedWorkitemsAsync(relationTypeName);
            }

            public IEnumerable<ITfsWorkitem> GetRelatedWorkitems(string relationTypeName)
            {
                var relatedIds = _relations
                    .Where(rel => rel.RelationTypeName == relationTypeName)
                    .Select(rel => rel.WorkitemId)
                    .ToList();
                if (relatedIds.Count == 0) return Enumerable.Empty<ITfsWorkitem>();

                return _tfsServiceClient.GetWorkitems(relatedIds);
            }

            public async Task<IEnumerable<ITfsWorkitem>> GetRelatedWorkitemsAsync(string relationTypeName)
            {
                var relatedIds = _relations
                    .Where(rel => rel.RelationTypeName == relationTypeName)
                    .Select(rel => rel.WorkitemId)
                    .ToList();
                if (relatedIds.Count == 0) return Enumerable.Empty<ITfsWorkitem>();

                return await _tfsServiceClient.GetWorkitemsAsync(relatedIds);
            }
        }

        public static ITfsWorkitem FromJsonItem(ITfsServiceClient tfsService, string jsonItemStr)
        {
            ITfsWorkitem item;
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
