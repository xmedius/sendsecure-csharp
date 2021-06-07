using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Text;

namespace XMedius.SendSecure.Helpers
{
    [JsonConverter(typeof(JsonObjects.Serializers.SafeboxSerializer))]
    public class Safebox
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Status
        {
            [EnumMember(Value = "in_progress")] InProgress,
            [EnumMember(Value = "closed")] Closed,
            [EnumMember(Value = "content_deleted")] ContentDeleted,
            [EnumMember(Value = "unread")] Unread
        };

        public Safebox(string userEmail, int? securityProfileId = null, string message = null, string subject = null, string notificationLanguage="en")
        {
            UserEmail = userEmail;
            Attachments = new List<Attachment>();
            Participants = new List<Participant>();
            SecurityOptions = new SecurityOptions();
            NotificationLanguage = notificationLanguage;
            SecurityProfileId = securityProfileId;
            Message = message;
            Subject = subject;
        }

        [JsonProperty(PropertyName = "guid")]
        public string Guid { get; set; }
        [JsonProperty(PropertyName = "user_id")]
        public int? UserId { get; set; }
        [JsonProperty(PropertyName = "enterprise_id")]
        public int? EnterpriseId { get; set; }
        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }
        [JsonProperty(PropertyName = "notification_language")]
        public string NotificationLanguage { get; set; }
        [JsonProperty(PropertyName = "status")]
        public Status? SafeboxStatus { get; set; }
        [JsonProperty(PropertyName = "security_profile_name")]
        public string SecurityProfileName { get; set; }
        [JsonProperty(PropertyName = "unread_count")]
        public int? UnreadCount { get; set; }
        [JsonProperty(PropertyName = "double_encryption_status")]
        public string DoubleEncryptionStatus { get; set; }
        [JsonProperty(PropertyName = "audit_record_pdf")]
        public string AuditRecordPdf { get; set; }
        [JsonProperty(PropertyName = "secure_link")]
        public string SecureLink { get; set; }
        [JsonProperty(PropertyName = "secure_link_title")]
        public string SecureLinkTitle { get; set; }
        [JsonProperty(PropertyName = "email_notification_enabled")]
        public bool? EmailNotificationEnabled { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime? UpdatedAt { get; set; }
        [JsonProperty(PropertyName = "assigned_at")]
        public DateTime? AssignedAt { get; set; }
        [JsonProperty(PropertyName = "latest_activity")]
        public DateTime? LatestActivity { get; set; }
        [JsonProperty(PropertyName = "expiration")]
        public DateTime? Expiration { get; set; }
        [JsonProperty(PropertyName = "closed_at")]
        public DateTime? ClosedAt { get; set; }
        [JsonProperty(PropertyName = "content_deleted_at")]
        public DateTime? ContentDeletedAt { get; set; }
        [JsonProperty(PropertyName = "security_options")]
        public SecurityOptions SecurityOptions { get; set; }
        [JsonProperty(PropertyName = "participants")]
        public List<Participant> Participants { get; set; }
        [JsonProperty(PropertyName = "messages")]
        public List<Message> Messages { get; set; }
        [JsonProperty(PropertyName = "download_activity")]
        public DownloadActivity DownloadActivity { get; set; }
        [JsonProperty(PropertyName = "event_history")]
        public List<EventHistory> EventHistory { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        [JsonIgnore]
        public List<Attachment> Attachments { get; set; }
        [JsonProperty(PropertyName = "upload_url")]
        public string UploadUrl { get; set; }
        [JsonProperty(PropertyName = "security_profile_id")]
        public int? SecurityProfileId { get; set; }
        [JsonProperty(PropertyName = "public_encryption_key")]
        public string PublicEncryptionKey { get; set; }
        [JsonProperty(PropertyName = "user_email")]
        public string UserEmail { get; set; }
        [JsonProperty(PropertyName = "preview_url")]
        public string PreviewUrl { get; set; }
        [JsonProperty(PropertyName = "encryption_key")]
        public string EncryptionKey { get; set; }

        // SecurityOptions attributes used only to update a safebox SecurityOptions after creation
        // The following properties alternate getters are intentionally omitted
        [JsonIgnore]
        public int? SecurityCodeLength { get; set; }
        [JsonProperty("security_code_length")]
        private int? SecurityCodeLengthAlternateSetter
        {
            set { SecurityOptions.SecurityCodeLength = value; }
        }

        [JsonIgnore]
        public SecurityEnums.RetentionPeriod? RetentionPeriodType { get; set; }
        [JsonProperty("retention_period_type")]
        private SecurityEnums.RetentionPeriod? RetentionPeriodTypeAlternateSetter
        {
            set { SecurityOptions.RetentionPeriodType = value; }
        }

        [JsonIgnore]
        public SecurityEnums.TimeUnit? RetentionPeriodUnit { get; set; }
        [JsonProperty("retention_period_unit")]
        private SecurityEnums.TimeUnit? RetentionPeriodUnitAlternateSetter
        {
            set { SecurityOptions.RetentionPeriodUnit = value; }
        }

        [JsonIgnore]
        public bool? TwoFactorRequired { get; set; }
        [JsonProperty("two_factor_required")]
        private bool? TwoFactorRequiredAlternateSetter
        {
            set { SecurityOptions.TwoFactorRequired = value; }
        }

        [JsonIgnore]
        public bool? EncryptMessage { get; set; }
        [JsonProperty("encrypt_message")]
        private bool? EncryptMessageAlternateSetter
        {
            set { SecurityOptions.EncryptMessage = value; }
        }

        [JsonIgnore]
        public int? CodeTimeLimit { get; set; }
        [JsonProperty("code_time_limit")]
        private int? CodeTimeLimitAlternateSetter
        {
            set { SecurityOptions.CodeTimeLimit = value; }
        }

        [JsonIgnore]
        public bool? GroupReplies { get; set; }
        [JsonProperty("group_replies")]
        private bool? GroupRepliesAlternateSetter
        {
            set { SecurityOptions.GroupReplies = value; }
        }

        [JsonIgnore]
        public bool? ReplyEnabled { get; set; }
        [JsonProperty("reply_enabled")]
        private bool? ReplyEnabledAlternateSetter
        {
            set { SecurityOptions.ReplyEnabled = value; }
        }

        [JsonIgnore]
        public bool? AllowVoice { get; set; }
        [JsonProperty("allow_voice")]
        private bool? AllowVoiceAlternateSetter
        {
            set { SecurityOptions.AllowVoice = value; }
        }

        [JsonIgnore]
        public bool? AllowEmail { get; set; }
        [JsonProperty("allow_email")]
        private bool? AllowEmailAlternateSetter
        {
            set { SecurityOptions.AllowEmail = value; }
        }

        [JsonIgnore]
        public bool? AllowSms { get; set; }
        [JsonProperty("allow_sms")]
        private bool? AllowSmsAlternateSetter
        {
            set { SecurityOptions.AllowSms = value; }
        }

        [JsonIgnore]
        public int? AllowedLoginAttempts { get; set; }
        [JsonProperty("allowed_login_attempts")]
        private int? AllowedLoginAttemptsAlternateSetter
        {
            set { SecurityOptions.AllowedLoginAttempts = value; }
        }

        [JsonIgnore]
        public bool? AllowRememberMe { get; set; }
        [JsonProperty("allow_remember_me")]
        private bool? AllowRememberMeAlternateSetter
        {
            set { SecurityOptions.AllowRememberMe = value; }
        }

        [JsonIgnore]
        public int? RetentionPeriodValue { get; set; }
        [JsonProperty("retention_period_value")]
        private int? RetentionPeriodValueAlternateSetter
        {
            set { SecurityOptions.RetentionPeriodValue = value; }
        }

        [JsonIgnore]
        public int? AutoExtendValue { get; set; }
        [JsonProperty("auto_extend_value")]
        private int? AutoExtendValueAlternateSetter
        {
            set { SecurityOptions.AutoExtendValue = value; }
        }

        [JsonIgnore]
        public SecurityEnums.TimeUnit? AutoExtendUnit { get; set; }
        [JsonProperty("auto_extend_unit")]
        private SecurityEnums.TimeUnit? AutoExtendUnitAlternateSetter
        {
            set { SecurityOptions.AutoExtendUnit = value; }
        }

        [JsonIgnore]
        public bool? AllowManualClose { get; set; }
        [JsonProperty("allow_manual_close")]
        private bool? AllowManualCloseAlternateSetter
        {
            set { SecurityOptions.AllowManualClose = value; }
        }

        [JsonIgnore]
        public bool? AllowManualDelete { get; set; }
        [JsonProperty("allow_manual_delete")]
        private bool? AllowManualDeleteAlternateSetter
        {
            set { SecurityOptions.AllowManualDelete = value; }
        }

        [JsonIgnore]
        public int? ConsentGroupId { get; set; }
        [JsonProperty("consent_group_id")]
        private int? ConsentGroupIdAlternateSetter
        {
            set { SecurityOptions.ConsentGroupId = value; }
        }

        public void SetExpirationValues(DateTime dateTime)
        {
            if (SafeboxStatus == null)
            {
                throw new Exceptions.SendSecureException("Cannot change the expiration of a committed safebox, please see the method addTime to extend the lifetime of the safebox");
            }
            SecurityOptions.ExpirationDate = dateTime.ToString("yyyy-MM-dd");
            SecurityOptions.ExpirationTime = dateTime.ToString("HH:mm:ss");
            SecurityOptions.ExpirationTimeZone = dateTime.ToString("zzz");
        }

        public string ToJson()
        {
            dynamic safeboxWrapper = new
            {
                safebox = this
            };

            JObject jSafebox = JObject.FromObject(safeboxWrapper);

            jSafebox["safebox"].RemoveEntries(new List<string> { "user_id", "enterprise_id", "safebox_status", "security_profile_name", "unread_count",
                                                                 "double_encryption_status", "audit_record_pdf", "secure_link", "secure_link_title",
                                                                 "created_at", "updated_at", "assigned_at", "latest_activity",
                                                                 "expiration", "closed_at", "content_deleted_at", "security_options", "messages", "download_activity",
                                                                 "event_history", "upload_url", "preview_url", "encryption_key", "force_expiry_date", "security_code_length",
                                                                 "allowed_login_attempts", "allow_remember_me", "allow_sms", "allow_voice", "allow_email", "code_time_limit",
                                                                 "auto_extend_value", "auto_extend_unit", "two_factor_required", "allow_manual_delete",
                                                                 "allow_manual_close", "encrypt_attachments", "consent_group_id" })
                    ["recipients"].RemoveEntries(new List<string> { "guest_options", "message_read_count", "message_total_count" });

            return jSafebox.ToString(Formatting.None);
        }

        public void Update(string json)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };

            JsonConvert.PopulateObject(json, this, jsonSerializerSettings);
        }

        public static Safebox FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Safebox>(json);
        }

        public static Status StringToStatus(string str)
        {
            System.Globalization.TextInfo info = new System.Globalization.CultureInfo("en-US", false).TextInfo;

            foreach (var value in Enum.GetValues(typeof(Status)))
            {
                if (Enum.GetName(typeof(Status), value).Equals(info.ToTitleCase(str).Replace("_", "")))
                {
                    return (Status)value;
                }
            }

            throw new Exceptions.SendSecureException();
        }

        public static String StatusToString(Status status)
        {
            string enumName = Enum.GetName(typeof(Status), status);

            return string.Concat(enumName.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        public string TemporaryDocument(long fileSize)
        {
            StringBuilder sb =
                new StringBuilder("{\"temporary_document\":{")
                    .Append("\"document_file_size\":")
                    .Append(fileSize)
                    .Append("},\"multipart\":false");
            if (!String.IsNullOrEmpty(PublicEncryptionKey))
            {
                sb.Append(",\"public_encryption_key\":")
                  .Append("\"" + PublicEncryptionKey + "\"");
            }
            return sb.Append("}").ToString();
        }
    }
}
