using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace XMedius.SendSecure.Helpers
{
    public class ConsentMessageGroup
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty(PropertyName = "consent_messages")]
        public List<ConsentMessage> ConsentMessages { get; set; }

        public static ConsentMessageGroup FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ConsentMessageGroup>(json);
        }
    }
}
