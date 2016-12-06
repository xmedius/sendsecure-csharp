using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.JsonObjects
{
    class NewSafeboxResponseSuccess
    {
        [JsonProperty(Required = Required.Always)]
        public string guid { get; set; }
        public string public_encryption_key { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string upload_url { get; set; }
    }
}
