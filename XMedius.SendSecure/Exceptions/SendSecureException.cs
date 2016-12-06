using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.Exceptions
{
    public class SendSecureException : Exception
    {
        public SendSecureException()
            : base("An error occurred within a SendSecure call.")
        {
        }

        public SendSecureException(string message)
            : base(message)
        {
        }

        public SendSecureException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public SendSecureException(int code, string response, string message)
            : base(message)
        {
            Code = code;
            Response = response;
        }

        public SendSecureException(int code, string message)
            : base(message)
        {
            Code = code;
            Response = null;
        }

        public int Code
        {
            get; private set;
        }

        public string Response
        {
            get; private set;
        }
    }
}
