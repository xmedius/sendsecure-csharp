using System;
using Newtonsoft.Json;

namespace XMedius.SendSecure.Helpers
{
    public class ConsentMessage
    {
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
