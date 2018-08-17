using Newtonsoft.Json;
using System;

namespace XMedius.SendSecure.Helpers
{
    public class SecurityProfile
    {
        public class ValueT<T>
        {
            [JsonProperty(PropertyName = "value", Required = Required.AllowNull)]
            public T Value { get; set; }
            [JsonProperty(PropertyName = "modifiable", Required = Required.Always)]
            public bool? Modifiable { get; set; }
        }

        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "allowed_login_attempts")]
        public ValueT<int?> AllowedLoginAttempts { get; set; }
        [JsonProperty(PropertyName = "allow_remember_me")]
        public ValueT<bool?> AllowRememberMe { get; set; }
        [JsonProperty(PropertyName = "allow_sms")]
        public ValueT<bool?> AllowSms { get; set; }
        [JsonProperty(PropertyName = "allow_voice")]
        public ValueT<bool?> AllowVoice { get; set; }
        [JsonProperty(PropertyName = "allow_email")]
        public ValueT<bool?> AllowEmail { get; set; }
        [JsonProperty(PropertyName = "code_time_limit")]
        public ValueT<int?> CodeTimeLimit { get; set; }
        [JsonProperty(PropertyName = "code_length")]
        public ValueT<int?> CodeLength { get; set; }
        [JsonProperty(PropertyName = "auto_extend_value")]
        public ValueT<int?> AutoExtendValue { get; set; }
        [JsonProperty(PropertyName = "auto_extend_unit")]
        public ValueT<SecurityEnums.TimeUnit> AutoExtendUnit { get; set; }
        [JsonProperty(PropertyName = "two_factor_required")]
        public ValueT<bool?> TwoFactorRequired { get; set; }
        [JsonProperty(PropertyName = "encrypt_attachments")]
        public ValueT<bool?> EncryptAttachments { get; set; }
        [JsonProperty(PropertyName = "encrypt_message")]
        public ValueT<bool?> EncryptMessage { get; set; }
        [JsonProperty(PropertyName = "expiration_value")]
        public ValueT<int> ExpirationValue { get; set; }
        [JsonProperty(PropertyName = "expiration_unit")]
        public ValueT<SecurityEnums.TimeUnit> ExpirationUnit { get; set; }
        [JsonProperty(PropertyName = "reply_enabled")]
        public ValueT<bool?> ReplyEnabled { get; set; }
        [JsonProperty(PropertyName = "group_replies")]
        public ValueT<bool?> GroupReplies { get; set; }
        [JsonProperty(PropertyName = "double_encryption")]
        public ValueT<bool?> DoubleEncryption { get; set; }
        [JsonProperty(PropertyName = "retention_period_type")]
        public ValueT<SecurityEnums.RetentionPeriod> RetentionPeriodType { get; set; }
        [JsonProperty(PropertyName = "retention_period_value")]
        public ValueT<int?> RetentionPeriodValue { get; set; }
        [JsonProperty(PropertyName = "retention_period_unit")]
        public ValueT<SecurityEnums.TimeUnit?> RetentionPeriodUnit { get; set; }
        [JsonProperty(PropertyName = "allow_manual_delete")]
        public ValueT<bool?> AllowManualDelete { get; set; }
        [JsonProperty(PropertyName = "allow_manual_close")]
        public ValueT<bool?> AllowManualClose { get; set; }
        [JsonProperty(PropertyName = "allow_for_secure_links")]
        public ValueT<bool?> AllowForSecureLinks { get; set; }
        [JsonProperty(PropertyName = "use_captcha")]
        public ValueT<bool?> UseCaptcha { get; set; }
        [JsonProperty(PropertyName = "verify_email")]
        public ValueT<bool?> VerifyEmail { get; set; }
        [JsonProperty(PropertyName = "distribute_key")]
        public ValueT<bool?> DistributeKey { get; set; }
        [JsonProperty(PropertyName = "consent_group_id")]
        public ValueT<int> ConsentGroupId { get; set; }


        public static SecurityProfile FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SecurityProfile>(json);
        }
    }
}
