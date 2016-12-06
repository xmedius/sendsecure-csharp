using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.JsonObjects
{
    class GetTokenResponseSuccess
    {
        public bool Result { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Token { get; set; }
    }
}
