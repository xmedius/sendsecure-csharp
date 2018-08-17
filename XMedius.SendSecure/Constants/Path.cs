namespace XMedius.SendSecure.Constants
{
    public class Path
    {
        public const string NEW_SAFEBOX = "api/v2/safeboxes/new.json?user_email={0}&locale={1}";

        public const string SECURITY_PROFILES = "api/v2/enterprises/{0}/security_profiles.json?user_email={1}&locale={2}";

        public const string ENTERPRISE_SETTINGS = "api/v2/enterprises/{0}/settings.json?locale={1}";

        public const string COMMIT_SAFEBOX = "api/v2/safeboxes.json?locale={0}";

        public const string USER_SETTINGS = "api/v2/enterprises/{0}/users/{1}/settings.json?locale={2}";

        public const string GET_CREATE_FAVORITE = "api/v2/enterprises/{0}/users/{1}/favorites.json?locale={2}";

        public const string EDIT_DELETE_FAVORITE = "api/v2/enterprises/{0}/users/{1}/favorites/{2}.json?locale={3}";

        public const string GET_CREATE_PARTICIPANT = "api/v2/safeboxes/{0}/participants.json?locale={1}";

        public const string UPDATE_PARTICIPANT = "api/v2/safeboxes/{0}/participants/{1}.json?locale={2}";

        public const string ADD_TIME = "api/v2/safeboxes/{0}/add_time.json?locale={1}";

        public const string CLOSE_SAFEBOX = "api/v2/safeboxes/{0}/close.json?locale={1}";

        public const string DELETE_SAFEBOX_CONTENT = "api/v2/safeboxes/{0}/delete_content.json?locale={1}";

        public const string MARK_AS_READ = "api/v2/safeboxes/{0}/mark_as_read.json?locale={1}";

        public const string MARK_AS_UNREAD = "api/v2/safeboxes/{0}/mark_as_unread.json?locale={1}";

        public const string FILE_URL = "api/v2/safeboxes/{0}/documents/{1}/url.json?user_email={2}&locale={3}";

        public const string AUDIT_RECORD = "api/v2/safeboxes/{0}/audit_record_pdf.json?locale={1}";

        public const string SAFEBOXES = "api/v2/safeboxes.json&locale={0}";

        public const string SAFEBOXES_FILTER = "api/v2/safeboxes.json?status={0}&search={1}&per_page={2}&page={3}&locale={4}";

        public const string SAFEBOX_INFO = "api/v2/safeboxes/{0}.json&locale={1}";

        public const string SAFEBOX_INFO_SECTIONS = "api/v2/safeboxes/{0}.json?sections={1}&locale={2}";

        public const string MESSAGES = "api/v2/safeboxes/{0}/messages.json?locale={1}";

        public const string DOWNLOAD_ACTIVITY = "api/v2/safeboxes/{0}/download_activity.json?locale={1}";

        public const string EVENT_HISTORY = "api/v2/safeboxes/{0}/event_history.json?locale={1}";

        public const string SECURITY_OPTIONS = "api/v2/safeboxes/{0}/security_options.json?locale={1}";

        public const string MARK_AS_READ_MESSAGE = "api/v2/safeboxes/{0}/messages/{1}/read?locale={2}";

        public const string MARK_AS_UNREAD_MESSAGE = "api/v2/safeboxes/{0}/messages/{1}/unread?locale={2}";

        public const string ARCHIVE_SAFEBOX = "api/v2/safeboxes/{0}/tag/archive?locale={1}";

        public const string UNARCHIVE_SAFEBOX = "api/v2/safeboxes/{0}/untag/archive?locale={1}";

        public const string SEARCH_RECIPIENT = "api/v2/participants/autocomplete?term={0}&locale={1}";

        public const string NEW_FILE = "api/v2/safeboxes/{0}/uploads?locale={1}";

        public const string REPLY = "api/v2/safeboxes/{0}/messages.json?locale={1}";

        public const string CONSENT_GROUP = "api/v2/enterprises/{0}/consent_message_groups/{1}?locale={2}";
    }
}
