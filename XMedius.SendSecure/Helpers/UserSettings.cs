using Newtonsoft.Json;
using System;

namespace XMedius.SendSecure.Helpers
{
    public class UserSettings
    {
        [JsonProperty(PropertyName = "created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime? UpdatedAt { get; set; }
        [JsonProperty(PropertyName = "mask_note")]
        public bool? MaskNote { get; set; }
        [JsonProperty(PropertyName = "open_first_transaction")]
        public bool? OpenFirstTransaction { get; set; }
        [JsonProperty(PropertyName = "mark_as_read")]
        public bool? MarkAsRead { get; set; }
        [JsonProperty(PropertyName = "mark_as_read_delay")]
        public int? MarkAsReadDelay { get; set; }
        [JsonProperty(PropertyName = "remember_key")]
        public bool? RememberKey { get; set; }
        [JsonProperty(PropertyName = "default_filter")]
        public string DefaultFilter { get; set; }
        [JsonProperty(PropertyName = "recipient_language")]
        public string RecipientLanguage { get; set; }
        [JsonProperty(PropertyName = "secure_link")]
        public PersonnalSecureLink SecureLink { get; set; }

        public static UserSettings FromJson(string json)
        {
            return JsonConvert.DeserializeObject<UserSettings>(json);
        }
    }    
}
