using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.JsonObjects
{
    internal class GetTokenRequest
    {
        public string permalink;
        public string username;
        public string password;
        public string application_type;
        public string device_id;
        public string device_name;
        public string otp;
    }
}
