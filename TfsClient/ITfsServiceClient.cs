﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    public interface ITfsServiceClient
    {
        string ServerUrl { get; }
        string Collection { get; }
        string Project { get; }

        void Authentificate(string userName, string userPassword);

        ITfsWorkitem GetSingleWorkitem(int id, 
            IEnumerable<string> fields = null, string expand = "All");
        IEnumerable<ITfsWorkitem> GetWorkitems(IEnumerable<int> ids,
            IEnumerable<string> fields = null, string expand = "All", int batchSize = 50);

        ITfsWorkitem UpdateWorkitemFields(int workitemId, IDictionary<string, string> itemFields,
            string expand = "All", bool bypassRules = false,
            bool supressNotifications = true, bool validateOnly = false);
    }
}
