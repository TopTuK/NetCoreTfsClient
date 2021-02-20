using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsClient
{
    internal static class TfsWiqlFactory
    {
        private class TfsWiqlResult : ITfsWiqlResult
        {
            private ITfsServiceClient _tfsServiceClient;
            private int[] _itemIds = null;

            public bool IsEmpty => _itemIds == null;
            public IEnumerable<int> ItemIds => _itemIds;
            public int Count => _itemIds != null
                ? _itemIds.Count()
                : -1;

            public TfsWiqlResult(ITfsServiceClient tfsServiceClient, JToken jsonItems)
            {
                _tfsServiceClient = tfsServiceClient;

                var itemCount = jsonItems.Children().Count();
                if(itemCount > 0)
                {
                    _itemIds = new int[itemCount];

                    var idx = 0;
                    foreach(var jItem in jsonItems.Children())
                    {
                        _itemIds[idx++] = jItem["id"].ToObject<int>();
                    }
                }
            }

            public IEnumerable<ITfsWorkitem> GetWorkitems()
            {
                if (IsEmpty) return null;

                return _tfsServiceClient.GetWorkitems(_itemIds);
            }
        }

        public static ITfsWiqlResult FromContentResponse(ITfsServiceClient tfsServiceClient, string contentResponse)
        {
            try
            {
                var jsonObj = JObject.Parse(contentResponse);
                if (jsonObj["workItems"] == null) return null;

                return new TfsWiqlResult(tfsServiceClient, jsonObj["workItems"]);
            }
            catch
            {
                return null;
            }
        }

        public static ITfsWiqlResult FromQueryResponse(ITfsServiceClient tfsServiceClient, string contentResponse)
        {
            try
            {
                var jsonObj = JObject.Parse(contentResponse);
                if (jsonObj["wiql"] == null) return null;

                return tfsServiceClient.RunWiql(jsonObj["wiql"].ToObject<string>());
            }
            catch
            {
                return null;
            }
        }
    }
}
