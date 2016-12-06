using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.Helpers
{
    public class ExtensionFilter
    {
        [JsonProperty(PropertyName = "mode", Required = Required.Always)]
        public string Mode { get; set; }
        [JsonProperty(PropertyName = "list", Required = Required.Always)]
        public List<string> List { get; set; }
    }
}
