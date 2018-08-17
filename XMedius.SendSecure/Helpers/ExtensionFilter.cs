using Newtonsoft.Json;
using System.Collections.Generic;

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
