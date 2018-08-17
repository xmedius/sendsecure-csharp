using System;
using Newtonsoft.Json;

namespace XMedius.SendSecure.JsonObjects
{
    public class CommitSafeboxResponseSuccess
    {
        [JsonProperty(PropertyName = "guid")]
        public string Guid { get; set; }
        [JsonProperty(PropertyName = "user_id")]
        public int UserId { get; set; }
        [JsonProperty(PropertyName = "enterprise_id")]
        public int EnterpriseId { get; set; }
        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }
        [JsonProperty(PropertyName = "expiration")]
        public DateTime? Expiration { get; set; }
        [JsonProperty(PropertyName = "notification_language")]
        public string NotificationLanguage { get; set; }
        [JsonProperty(PropertyName = "status")]
        public Helpers.Safebox.Status SafeboxStatus { get; set; }
        [JsonProperty(PropertyName = "security_profile_name")]
        public string SecurityProfileName { get; set; }
        [JsonProperty(PropertyName = "force_expiry_date")]
        public DateTime? ForceExpiryDate { get; set; }
        [JsonProperty(PropertyName = "preview_url")]
        public string PreviewUrl { get; set; }
        [JsonProperty(PropertyName = "encryption_key")]
        public string EncryptionKey { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty(PropertyName = "latest_activity")]
        public DateTime LatestActivity { get; set; }

        [JsonProperty(PropertyName = "security_code_length")]
        public int? SecurityCodeLength { get; set; }
        [JsonProperty(PropertyName = "allowed_login_attempts")]
        public int? AllowedLoginAttempts { get; set; }
        [JsonProperty(PropertyName = "allow_remember_me")]
        public bool? AllowRememberMe { get; set; }
        [JsonProperty(PropertyName = "allow_sms")]
        public bool? AllowSms { get; set; }
        [JsonProperty(PropertyName = "allow_voice")]
        public bool? AllowVoice { get; set; }
        [JsonProperty(PropertyName = "allow_email")]
        public bool? AllowEmail { get; set; }
        [JsonProperty(PropertyName = "code_time_limit")]
        public int? CodeTimeLimit { get; set; }
        [JsonProperty(PropertyName = "auto_extend_value")]
        public int? AutoExtendValue { get; set; }
        [JsonProperty(PropertyName = "auto_extend_unit")]
        public Helpers.SecurityEnums.TimeUnit? AutoExtendUnit { get; set; }
        [JsonProperty(PropertyName = "two_factor_required")]
        public bool? TwoFactorRequired { get; set; }
        [JsonProperty(PropertyName = "encrypt_message")]
        public bool? EncryptMessage { get; set; }
        [JsonProperty(PropertyName = "double_encryption")]
        public bool? DoubleEncryption { get; set; }
        [JsonProperty(PropertyName = "reply_enabled")]
        public bool? ReplyEnabled { get; set; }
        [JsonProperty(PropertyName = "group_replies")]
        public bool? GroupReplies { get; set; }
        [JsonProperty(PropertyName = "retention_period_type")]
        public Helpers.SecurityEnums.RetentionPeriod? RetentionPeriodType { get; set; }
        [JsonProperty(PropertyName = "retention_period_value")]
        public int? RetentionPeriodValue { get; set; }
        [JsonProperty(PropertyName = "retention_period_unit")]
        public Helpers.SecurityEnums.TimeUnit? RetentionPeriodUnit { get; set; }
        [JsonProperty(PropertyName = "delete_content_on")]
        public DateTime? DeleteContentOn { get; set; }
        [JsonProperty(PropertyName = "allow_manual_delete")]
        public bool? AllowManualDelete { get; set; }
        [JsonProperty(PropertyName = "allow_manual_close")]
        public bool? AllowManualClose { get; set; }

        public static CommitSafeboxResponseSuccess FromJson(string json)
        {
            return JsonConvert.DeserializeObject<CommitSafeboxResponseSuccess>(json);
        }
    }
}
