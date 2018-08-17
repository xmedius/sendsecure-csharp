using Newtonsoft.Json;
using System;

namespace XMedius.SendSecure.Helpers
{
    public class EnterpriseSettings
    {
        [JsonProperty(PropertyName = "created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "default_security_profile_id")]
        public int? DefaultSecurityProfileId { get; set; }
        [JsonProperty(PropertyName = "pdf_language")]
        public string PdfLanguage { get; set; }
        [JsonProperty(PropertyName = "use_pdfa_audit_records")]
        public bool? UsePdfaAuditRecords { get; set; }
        [JsonProperty(PropertyName = "international_dialing_plan")]
        public string InternationalDialingPlan { get; set; }

        [JsonProperty(PropertyName = "extension_filter")]
        public ExtensionFilter ExtensionFilter { get; set; }
        [JsonProperty(PropertyName = "virus_scan_enabled")]
        public bool? VirusScanEnabled { get; set; }
        [JsonProperty(PropertyName = "max_file_size_value")]
        public int? MaxFileSizeValue { get; set; }
        [JsonProperty(PropertyName = "max_file_size_unit")]
        public string MaxFileSizeUnit { get; set; }
        [JsonProperty(PropertyName = "include_users_in_autocomplete")]
        public bool? IncludeUsersInAutocomplete { get; set; }
        [JsonProperty(PropertyName = "include_favorites_in_autocomplete")]
        public bool? IncludeFavoritesInAutocomplete { get; set; }
        [JsonProperty(PropertyName = "users_public_url")]
        public bool? UsersPublicUrl { get; set; }


        public static EnterpriseSettings FromJson(string json)
        {
            return JsonConvert.DeserializeObject<EnterpriseSettings>(json);
        }
    }
}
