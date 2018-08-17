using Newtonsoft.Json;
using System;

namespace XMedius.SendSecure.Helpers
{
    public class SecurityOptions
    {
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
        public SecurityEnums.TimeUnit? AutoExtendUnit { get; set; }
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
        public SecurityEnums.RetentionPeriod? RetentionPeriodType { get; set; }
        [JsonProperty(PropertyName = "retention_period_value")]
        public int? RetentionPeriodValue { get; set; }
        [JsonProperty(PropertyName = "retention_period_unit")]
        public SecurityEnums.TimeUnit? RetentionPeriodUnit { get; set; }
        [JsonProperty(PropertyName = "allow_manual_delete")]
        public bool? AllowManualDelete { get; set; }
        [JsonProperty(PropertyName = "allow_manual_close")]
        public bool? AllowManualClose { get; set; }
        [JsonProperty(PropertyName = "expiration_unit")]
        public SecurityEnums.TimeUnit? ExpirationUnit { get; set; }
        [JsonProperty(PropertyName = "expiration_value")]
        public int? ExpirationValue { get; set; }
        [JsonProperty(PropertyName = "encrypt_attachments")]
        public bool? EncryptAttachments { get; set; }
        [JsonProperty(PropertyName = "consent_group_id")]
        public int? ConsentGroupId { get; set; }

        //Attributes to be used only when setting a Safebox expiration date
        [JsonProperty(PropertyName = "expiration_date")]
        public string ExpirationDate { get; set; }
        [JsonProperty(PropertyName = "expiration_time")]
        public string ExpirationTime { get; set; }
        [JsonProperty(PropertyName = "expiration_time_zone")]
        public string ExpirationTimeZone { get; set; }
    }
}
