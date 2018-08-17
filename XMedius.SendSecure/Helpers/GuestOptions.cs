using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace XMedius.SendSecure.Helpers
{
    public class GuestOptions
    {
        [JsonProperty(PropertyName = "company_name")]
        public string CompanyName { get; set; }
        [JsonProperty(PropertyName = "locked")]
        public bool? Locked { get; set; }
        [JsonProperty(PropertyName = "bounced_email")]
        public bool? BouncedEmail { get; set; }
        [JsonProperty(PropertyName = "failed_login_attempts")]
        public int? FailedLoginAttempts { get; set; }
        [JsonProperty(PropertyName = "verified")]
        public bool? Verified { get; set; }
        [JsonProperty(PropertyName = "contact_methods")]
        public List<ContactMethod> ContactMethods { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

}
