using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.Exceptions
{
    class UnexpectedServerResponseException : SendSecureException
    {
        public UnexpectedServerResponseException()
            : base(1, "An error occurred within a SendSecure call.")
        {
        }
    }
}
