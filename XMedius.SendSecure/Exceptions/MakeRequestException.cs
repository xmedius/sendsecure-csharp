using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.Exceptions
{
    internal class MakeRequestException : Exception
    {
        public MakeRequestException()
            : base("MakeRequestException")
        {
        }

        public MakeRequestException(string message)
        : base(message)
        {
        }

        public MakeRequestException(string message, Exception inner)
        : base(message, inner)
        {
        }

        public MakeRequestException(System.Net.HttpStatusCode statusCode, string reasonPhrase = null, string responseString = null)
            : base(reasonPhrase)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            ResponseString = responseString;
        }

        public System.Net.HttpStatusCode StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }
        public string ResponseString { get; private set; }

    }
}
