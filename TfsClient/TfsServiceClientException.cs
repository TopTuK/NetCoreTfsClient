using System;
using System.Collections.Generic;
using System.Text;

namespace TfsClient
{
    [Serializable]
    public class TfsServiceClientException : Exception
    {
        public TfsServiceClientException()
            : base()
        {

        }

        public TfsServiceClientException(string message)
            : base(message)
        {

        }

        public TfsServiceClientException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
