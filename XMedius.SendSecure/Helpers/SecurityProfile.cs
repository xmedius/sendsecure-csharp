using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace XMedius.SendSecure.Helpers
{
    public class SecurityProfile
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TimeUnit
        {
            [EnumMember(Value = "hours")] Hours,
            [EnumMember(Value = "days")] Days,
            [EnumMember(Value = "weeks")] Weeks,
            [EnumMember(Value = "months")] Months,
            [EnumMember(Value = "years")] Years
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum RetentionPeriod
        {
            [EnumMember(Value = "discard_at_expiration")] DiscardAtExpiration,
            [EnumMember(Value = "retain_at_expiration")] RetainAtExpiration,
            [EnumMember(Value = "do_not_discard")] DoNotDiscard
        }

        public class ValueT<T>
        {
            [JsonProperty(PropertyName = "value", Required = Required.AllowNull)]
            public T Value { get; set; }
            [JsonProperty(PropertyName = "modifiable", Required = Required.Always)]
            public bool Modifiable { get; set; }
        }

        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "allowed_login_attempts", Required = Required.Always)]
        public ValueT<int?> AllowedLoginAttempts { get; set; }
        [JsonProperty(PropertyName = "allow_remember_me", Required = Required.Always)]
        public ValueT<bool> AllowRememberMe { get; set; }
        [JsonProperty(PropertyName = "allow_sms", Required = Required.Always)]
        public ValueT<bool> AllowSms { get; set; }
        [JsonProperty(PropertyName = "allow_voice", Required = Required.Always)]
        public ValueT<bool> AllowVoice { get; set; }
        [JsonProperty(PropertyName = "allow_email", Required = Required.Always)]
        public ValueT<bool> AllowEmail { get; set; }
        [JsonProperty(PropertyName = "code_time_limit", Required = Required.Always)]
        public ValueT<int?> CodeTimeLimit { get; set; }
        [JsonProperty(PropertyName = "code_length", Required = Required.Always)]
        public ValueT<int?> CodeLength { get; set; }
        [JsonProperty(PropertyName = "auto_extend_value", Required = Required.Always)]
        public ValueT<int?> AutoExtendValue { get; set; }
        [JsonProperty(PropertyName = "auto_extend_unit", Required = Required.Always)]
        public ValueT<TimeUnit> AutoExtendUnit { get; set; }
        [JsonProperty(PropertyName = "two_factor_required", Required = Required.Always)]
        public ValueT<bool> TwoFactorRequired { get; set; }
        [JsonProperty(PropertyName = "encrypt_attachments", Required = Required.Always)]
        public ValueT<bool> EncryptAttachments { get; set; }
        [JsonProperty(PropertyName = "encrypt_message", Required = Required.Always)]
        public ValueT<bool> EncryptMessage { get; set; }
        [JsonProperty(PropertyName = "expiration_value", Required = Required.Always)]
        public ValueT<int> ExpirationValue { get; set; }
        [JsonProperty(PropertyName = "expiration_unit", Required = Required.Always)]
        public ValueT<TimeUnit> ExpirationUnit { get; set; }
        [JsonProperty(PropertyName = "reply_enabled", Required = Required.Always)]
        public ValueT<bool> ReplyEnabled { get; set; }
        [JsonProperty(PropertyName = "group_replies", Required = Required.Always)]
        public ValueT<bool> GroupReplies { get; set; }
        [JsonProperty(PropertyName = "double_encryption", Required = Required.Always)]
        public ValueT<bool> DoubleEncryption { get; set; }
        [JsonProperty(PropertyName = "retention_period_type", Required = Required.Always)]
        public ValueT<RetentionPeriod> RetentionPeriodType { get; set; }
        [JsonProperty(PropertyName = "retention_period_value", Required = Required.Always)]
        public ValueT<int?> RetentionPeriodValue { get; set; }
        [JsonProperty(PropertyName = "retention_period_unit", Required = Required.Always)]
        public ValueT<TimeUnit?> RetentionPeriodUnit { get; set; }

        public static SecurityProfile FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SecurityProfile>(json);
        }

        public static TimeUnit StringToTimeUnit(string str)
        {
            foreach (var value in Enum.GetValues(typeof(TimeUnit)))
            {
                if (Enum.GetName(typeof(TimeUnit), value).Equals(str, StringComparison.CurrentCultureIgnoreCase))
                {
                    return (TimeUnit)value;
                }
            }

            throw new Exceptions.SendSecureException();
        }

        public static String TimeUnitToString(TimeUnit timeUnit)
        {
            return Enum.GetName(typeof(TimeUnit), timeUnit).ToLower();
        }

        public static RetentionPeriod StringToRetentionPeriod(string str)
        {
            System.Globalization.TextInfo info = new System.Globalization.CultureInfo("en-US", false).TextInfo;

            foreach (var value in Enum.GetValues(typeof(RetentionPeriod)))
            {
                if (Enum.GetName(typeof(RetentionPeriod), value).Equals(info.ToTitleCase(str).Replace("_", "")))
                {
                    return (RetentionPeriod)value;
                }
            }

            throw new Exceptions.SendSecureException();
        }

        public static String RetentionPeriodToString(RetentionPeriod retentionPeriod)
        {
            string enumName = Enum.GetName(typeof(RetentionPeriod), retentionPeriod);

            return string.Concat(enumName.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }



    }
}
