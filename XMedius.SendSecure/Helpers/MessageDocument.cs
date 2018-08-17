using Newtonsoft.Json;

namespace XMedius.SendSecure.Helpers
{
    public class MessageDocument
    {
        [JsonProperty(PropertyName = "size")]
        public int? Size { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "sha")]
        public string Sha { get; set; }
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
