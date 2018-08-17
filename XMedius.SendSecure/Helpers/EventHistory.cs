using Newtonsoft.Json;
using System;

namespace XMedius.SendSecure.Helpers
{
    public class EventHistory
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "date")]
        public DateTime? Date { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "metadata")]
        public Metadata Metadata { get; set; }
    }
}
