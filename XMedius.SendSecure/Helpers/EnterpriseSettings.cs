using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.Helpers
{
    public class EnterpriseSettings
    {
        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "default_security_profile_id", Required = Required.AllowNull)]
        public int? DefaultSecurityProfileId { get; set; }
        [JsonProperty(PropertyName = "pdf_language", Required = Required.Always)]
        public string PdfLanguage { get; set; }
        [JsonProperty(PropertyName = "use_pdfa_audit_records", Required = Required.Always)]
        public bool UsePdfaAuditRecords { get; set; }
        [JsonProperty(PropertyName = "international_dialing_plan", Required = Required.Always)]
        public string InternationalDialingPlan { get; set; }

        [JsonProperty(PropertyName = "extension_filter", Required = Required.Always)]
        public ExtensionFilter ExtensionFilter;
        [JsonProperty(PropertyName = "include_users_in_autocomplete", Required = Required.Always)]
        public bool IncludeUsersInAutocomplete { get; set; }
        [JsonProperty(PropertyName = "include_favorites_in_autocomplete", Required = Required.Always)]
        public bool IncludeFavoritesInAutocomplete { get; set; }


        public static EnterpriseSettings FromJson(string json)
        {
            return JsonConvert.DeserializeObject<EnterpriseSettings>(json);
        }
    }
}
