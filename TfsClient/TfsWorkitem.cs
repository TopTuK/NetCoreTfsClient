using System;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    internal static class TfsWorkitemFactory
    {
        private class TfsWorkitem : ITfsWorkitem
        {
            public WorkItemType ItemType => throw new NotImplementedException();
            public int Id => throw new NotImplementedException();
            public string Title => throw new NotImplementedException();
            public string Description => throw new NotImplementedException();
            public IReadOnlyDictionary<string, string> Fields => throw new NotImplementedException();
        }

        public static ITfsWorkitem FromJson(string json)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<ITfsWorkitem> FromJsonItems(string jsonItems)
        {
            throw new NotImplementedException();
        }

        public static ITfsWorkitem CreateNewItem()
        {
            throw new NotImplementedException();
        }
    }
}
