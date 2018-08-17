using System;

namespace XMedius.SendSecure.Test.Constants
{
    public class Urls
    {
        public const string SendSecureServerSuccess = "https://portal.xmedius.com/services/testsuccess/sendsecure/server/url";

        public const string SendSecureServerSuccess2 = "https://portal.xmedius.com/services/testsuccess/sendsecure/server/url";

        public const string NewSafeboxSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/new.json?user_email=testsuccess@xmedius.com&locale=en";

        public const string CommitSafeboxSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes.json?locale=en";

        public const string SecurityProfilesSuccess = "https://sendsecure.xmedius.com/api/v2/enterprises/testsuccess/security_profiles.json?user_email=testsuccess@xmedius.com&locale=en";

        public const string EnterpriseSettingsSuccess = "https://sendsecure.xmedius.com/api/v2/enterprises/testsuccess/settings.json?locale=en";

        public const string UserSettingsSuccess = "https://sendsecure.xmedius.com/api/v2/enterprises/testsuccess/users/123/settings.json?locale=en";

        public const string FavoritesSuccess = "https://sendsecure.xmedius.com/api/v2/enterprises/testsuccess/users/123/favorites.json?locale=en";

        public const string EditDeleteFavoriteSuccess = "https://sendsecure.xmedius.com/api/v2/enterprises/testsuccess/users/123/favorites/456.json?locale=en";

        public const string ParticipantsSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/participants.json?locale=en";

        public const string UpdateParticipantSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/participants/7a3c51e00a004917a8f5db807180fcc5.json?locale=en";

        public const string AddTimeSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/add_time.json?locale=en";

        public const string CloseSafeboxSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/close.json?locale=en";

        public const string DeleteSafeboxContentSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/delete_content.json?locale=en";

        public const string MarkAsReadSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/mark_as_read.json?locale=en";

        public const string MarkAsUnreadSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/mark_as_unread.json?locale=en";

        public const string FileUrlSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/documents/97334293-23c2-4c94-8cee-369ddfabb678/url.json?user_email=testsuccess@xmedius.com&locale=en";

        public const string AuditRecordUrlSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/audit_record_pdf.json?locale=en";

        public const string SafeboxesSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes.json&locale=en";

        public const string SafeboxesFilterSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes.json?status=in_progress&search=safebox&per_page=1&page=1&locale=en";

        public const string SafeboxInfoSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b.json&locale=en";

        public const string SafeboxInfoSectionsSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b.json?sections=participants,messages&locale=en";

        public const string SafeboxMessagesSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/messages.json?locale=en";

        public const string SafeboxSecurityOptionsSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/security_options.json?locale=en";

        public const string SafeboxDownloadActivitySuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/download_activity.json?locale=en";

        public const string SafeboxEventHistorySuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/event_history.json?locale=en";

        public const string UploadFileSuccess = "http://fileserver.lvh.me/xmss/DteeDmb-2zfN5WtC7111OcWbl96EVtI=";

        public const string MarkAsReadMessageSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/messages/152664/read?locale=en";

        public const string MarkAsUnreadMessageSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/messages/152664/unread?locale=en";

        public const string ArchiveSafeboxSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/tag/archive?locale=en";

        public const string UnarchiveSafeboxSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/untag/archive?locale=en";

        public const string SearchRecipientSuccess = "https://sendsecure.xmedius.com/api/v2/participants/autocomplete?term=John&locale=en";

        public const string NewFileSuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/uploads?locale=en";

        public const string ReplySuccess = "https://sendsecure.xmedius.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/messages.json?locale=en";

        public const string ConsentMessageGroupSuccess = "https://sendsecure.xmedius.com/api/v2/enterprises/testsuccess/consent_message_groups/1?locale=en";


        public const string SendSecureServerError = "https://testerror.portal.com/services/testerror/sendsecure/server/url";

        public const string SendSecureServerNotFound = "https://portal.xmedius.com/services/testnotfound/sendsecure/server/url";

        public const string CommitSafeboxError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes.json?locale=en";

        public const string NewSafeboxError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/new.json?user_email=testerror@xmedius.com&locale=en";

        public const string SecurityProfilesError = "https://sendsecure.testerror.portal.com/api/v2/enterprises/testerror/security_profiles.json?user_email=testerror@xmedius.com&locale=en";

        public const string EnterpriseSettingsError = "https://sendsecure.testerror.portal.com/api/v2/enterprises/testerror/settings.json?locale=en";

        public const string UserSettingsError = "https://sendsecure.testerror.portal.com/api/v2/enterprises/testerror/users/123/settings.json?locale=en";

        public const string FavoritesError = "https://sendsecure.testerror.portal.com/api/v2/enterprises/testerror/users/123/favorites.json?locale=en";

        public const string EditDeleteFavoriteError = "https://sendsecure.testerror.portal.com/api/v2/enterprises/testerror/users/123/favorites/456.json?locale=en";

        public const string ParticipantsError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/participants.json?locale=en";

        public const string UpdateParticipantError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/participants/7a3c51e00a004917a8f5db807180fcc5.json?locale=en";

        public const string AddTimeError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/add_time.json?locale=en";

        public const string CloseSafeboxError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/close.json?locale=en";

        public const string DeleteSafeboxContentError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/delete_content.json?locale=en";

        public const string MarkAsReadError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/mark_as_read.json?locale=en";

        public const string MarkAsUnreadError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/mark_as_unread.json?locale=en";

        public const string FileUrlError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/documents/97334293-23c2-4c94-8cee-369ddfabb678/url.json?user_email=testerror@xmedius.com&locale=en";

        public const string AuditRecordUrlError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/audit_record_pdf.json?locale=en";

        public const string SafeboxesError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes.json?status=in_progress&search=safebox&per_page=1001&page=1&locale=en";

        public const string SafeboxInfoError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b.json&locale=en";

        public const string SafeboxMessagesError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/messages.json?locale=en";

        public const string SafeboxSecurityOptionsError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/security_options.json?locale=en";

        public const string SafeboxDownloadActivityError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/download_activity.json?locale=en";

        public const string SafeboxEventHistoryError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/event_history.json?locale=en";

        public const string MarkAsReadMessageError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/messages/152664/read?locale=en";

        public const string MarkAsUnreadMessageError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/messages/152664/unread?locale=en";

        public const string ArchiveSafeboxError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/tag/archive?locale=en";

        public const string UnarchiveSafeboxError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/untag/archive?locale=en";

        public const string SearchRecipientError = "https://sendsecure.testerror.portal.com/api/v2/participants/autocomplete?term=John&locale=en";

        public const string NewFileError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/uploads?locale=en";

        public const string ReplyError = "https://sendsecure.testerror.portal.com/api/v2/safeboxes/d65979185c1a4bbe85ef8ce3458de55b/messages.json?locale=en";

        public const string ConsentMessageGroupError = "https://sendsecure.testerror.portal.com/api/v2/enterprises/testerror/consent_message_groups/42?locale=en";

        public static Uri GetUri(string str)
        {
            return new Uri(str);
        }
    }
}
