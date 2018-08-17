using Newtonsoft.Json;
using System.Collections.Generic;

namespace XMedius.SendSecure.Helpers
{
    public class Metadata
    {
        [JsonProperty(PropertyName = "attachment_count")]
        public int? AttachmentCount { get; set; }
        [JsonProperty(PropertyName = "emails")]
        public List<string> Emails { get; set; }
    }
}
