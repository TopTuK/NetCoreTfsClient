using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    internal static class TfsWiqlFactory
    {
        private class TfsWiqlResult : ITfsWiqlResult
        {
        }

        public static ITfsWiqlResult FromContentResponse(ITfsServiceClient tfsServiceClient, string contentREsponse)
        {
            throw new NotImplementedException();
        }
    }
}
