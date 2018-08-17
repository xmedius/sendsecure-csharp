using Newtonsoft.Json;

namespace XMedius.SendSecure.Helpers
{
    public class PersonnalSecureLink
    {
        [JsonProperty(PropertyName = "security_profile_id")]
        public int? SecurityProfileId { get; set; }
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
        [JsonProperty(PropertyName = "enabled")]
        public bool? Enabled { get; set; }
    }
}
