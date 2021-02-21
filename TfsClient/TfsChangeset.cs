using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsClient
{
    internal static class TfsChangesetFactory
    {
        private class TfsChangeset : ITfsChangeset
        {
            private ITfsServiceClient _tfsServiceClient;
            private List<int> _workitemsIds;
            public int ChangesetId { get; }

            public TfsChangeset(ITfsServiceClient tfsServiceClient, JToken jsonObj)
            {
                _tfsServiceClient = tfsServiceClient;
                ChangesetId = jsonObj["changesetId"].ToObject<int>();

                if (jsonObj["workItems"] != null)
                {
                    _workitemsIds = jsonObj["worktems"]
                        .Select(jsonItem => jsonItem["id"].ToObject<int>())
                        .ToList();
                }
            }

            public IEnumerable<ITfsWorkitem> GetAssociatedWorkItem()
            {
                return ((_workitemsIds != null) && (_workitemsIds.Count > 0))
                    ? _tfsServiceClient.GetWorkitems(_workitemsIds)
                    : null;
            }
        }

        public static ITfsChangeset FromResponse(ITfsServiceClient tfsServiceClient, string response)
        {
            var jsonObj = JObject.Parse(response);

            return new TfsChangeset(tfsServiceClient, jsonObj);
        }
    }
}
