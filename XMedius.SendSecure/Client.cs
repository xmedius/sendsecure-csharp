using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XMedius.SendSecure
{
    public class Client
    {
        private static TraceSource TraceSource = new TraceSource("XMedius.SendSecure");

        private static readonly Uri DEFAULT_PORTAL_URI = new Uri("https://portal.xmedius.com");
        private static readonly string USER_TOKEN_PATH = "api/user_token";

        private string EnterpriseAccount;
        private string ApiToken;
        private Uri EndPoint;
        private JsonClient JsonClient;
        private string Locale;

        public Client(string apiToken, string enterpriseAccount, Uri endpoint, string locale = "en")
        {
            EnterpriseAccount = enterpriseAccount;
            ApiToken = apiToken;
            EndPoint = endpoint;
            Locale = locale;
            JsonClient = new JsonClient(ApiToken, EnterpriseAccount, EndPoint);
        }

        public static async Task<string> GetUserTokenAsync(string enterpriseAccount, string username, string password, string deviceId, 
            string deviceName, string applicationType = "SendSecure C#", Uri endpoint = null, string oneTimePassword = "", 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (endpoint == null)
            {
                endpoint = DEFAULT_PORTAL_URI;
            }

            try
            {
                string portalUrl = await Utils.SendSecureUrlUtil.GetPortalUrlForEnterpriseAccountAsync(enterpriseAccount, endpoint, cancellationToken);

                Uri resourceAddress = new Uri(new Uri(portalUrl), USER_TOKEN_PATH);

                var jsonRequest = new JsonObjects.GetTokenRequest
                {
                    permalink = enterpriseAccount,
                    username = username,
                    password = password,
                    application_type = applicationType,
                    device_id = deviceId,
                    device_name = deviceName,
                    otp = oneTimePassword
                };

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, resourceAddress);
                StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(jsonRequest), Encoding.UTF8, "application/json");
                request.Content = jsonContent;
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string responseString = await Utils.HttpUtil.MakeRequestAsync(request, cancellationToken);

                var ResponseData = JsonConvert.DeserializeObject<JsonObjects.GetTokenResponseSuccess>(responseString);

                return ResponseData.Token;
            }
            catch (Exceptions.MakeRequestException e)
            {
                if (e.StatusCode > 0)
                {
                    if (!String.IsNullOrEmpty(e.ResponseString))
                    {
                        try
                        {
                            var responseData = JsonConvert.DeserializeObject<JsonObjects.GetTokenResponseError>(e.ResponseString);
                            throw new Exceptions.SendSecureException(responseData.Code, e.ResponseString, responseData.Message);
                        }
                        catch (JsonException)
                        {
                        }
                    }

                    throw new Exceptions.SendSecureException((int)e.StatusCode, e.Message);
                }
                else
                {
                    throw new Exceptions.UnexpectedServerResponseException();
                }

            }
            catch (JsonSerializationException)
            {
                throw new Exceptions.UnexpectedServerResponseException();
            }
            catch (TaskCanceledException)
            {
                TraceSource.TraceEvent(TraceEventType.Information, 0, "GetUserToken Cancelled.");
                throw new OperationCanceledException();
            }
            catch (Exception e)
            {
                TraceSource.TraceEvent(TraceEventType.Error, 0,
                    "Unexpected exception in GetToken ({0})", e.Message);
                throw new Exceptions.SendSecureException(e.Message);
            }
        }

        public async Task<Helpers.SafeboxResponse> SubmitSafeboxAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            await InitializeSafeboxAsync(safebox, cancellationToken);

            foreach(Helpers.Attachment attachment in safebox.Attachments)
            {
                await UploadAttachmentAsync(safebox, attachment, cancellationToken);
            }

            if(safebox.SecurityProfile == null)
            {
                safebox.SecurityProfile = await DefaultSecurityProfileAsync(safebox.UserEmail, cancellationToken);

                if(safebox.SecurityProfile == null)
                {
                    throw new Exceptions.SendSecureException();
                }
            }

            return await CommitSafeboxAsync(safebox, cancellationToken);
        }

        public async Task<Helpers.Safebox> InitializeSafeboxAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.NewSafeboxAsync(safebox.UserEmail, cancellationToken);

            var response = JsonConvert.DeserializeObject<JsonObjects.NewSafeboxResponseSuccess>(jsonResponse);

            safebox.Guid = response.guid;
            safebox.PublicEncryptionKey = response.public_encryption_key;
            safebox.UploadUrl = response.upload_url;

            return safebox;
        }

        public async Task<Helpers.Attachment> UploadAttachmentAsync(Helpers.Safebox safebox, Helpers.Attachment attachment, CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.UploadFileAsync(new Uri(safebox.UploadUrl), attachment.Stream, attachment.ContentType, attachment.FileName, attachment.Size, cancellationToken);

            var response = JsonConvert.DeserializeObject<JsonObjects.UploadFileResponseSuccess>(jsonResponse);

            attachment.Guid = response.temporary_document.document_guid;

            return attachment;
        }

        public async Task<Helpers.SafeboxResponse> CommitSafeboxAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new JsonObjects.CommitSafeboxRequest
            {
                safebox = new JsonObjects.CommitSafeboxRequest.SafeBox
                {
                    guid = safebox.Guid,
                    subject = safebox.Subject,
                    message = safebox.Message,
                    security_profile_id = safebox.SecurityProfile.Id,
                    public_encryption_key = safebox.PublicEncryptionKey,
                    notification_language = Locale,

                    reply_enabled = safebox.SecurityProfile.ReplyEnabled.Value,
                    group_replies = safebox.SecurityProfile.GroupReplies.Value,
                    expiration_value = safebox.SecurityProfile.ExpirationValue.Value,
                    expiration_unit = safebox.SecurityProfile.ExpirationUnit.Value,
                    retention_period_type = safebox.SecurityProfile.RetentionPeriodType.Value,
                    retention_period_value = safebox.SecurityProfile.RetentionPeriodValue.Value,
                    retention_period_unit = safebox.SecurityProfile.RetentionPeriodUnit.Value,
                    encrypt_message = safebox.SecurityProfile.ReplyEnabled.Value,
                    double_encryption = safebox.SecurityProfile.ReplyEnabled.Value,

                    document_ids = new List<string>(),
                    recipients = safebox.Recipients
                }
            };

            foreach (var attachment in safebox.Attachments)
            {
                request.safebox.document_ids.Add(attachment.Guid);
            }

            string json = JsonConvert.SerializeObject(request);

            string jsonResponse = await JsonClient.CommitSafeboxAsync(json, cancellationToken);

            return JsonConvert.DeserializeObject<Helpers.SafeboxResponse>(jsonResponse);
        }

        public async Task<List<Helpers.SecurityProfile>> SecurityProfilesAsync(string userEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.GetSecurityProfilesAsync(userEmail, cancellationToken);

            var response = JsonConvert.DeserializeObject<JsonObjects.GetSecurityProfilesResponseSuccess>(jsonResponse);

            return response.SecurityProfiles;
        }

        public async Task<Helpers.SecurityProfile> DefaultSecurityProfileAsync(string userEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            var enterpriseSettings = await EnterpriseSettingsAsync(cancellationToken);
            var securityProfiles = await SecurityProfilesAsync(userEmail, cancellationToken);

            return securityProfiles.Find(x => x.Id == enterpriseSettings.DefaultSecurityProfileId);
        }


        public async Task<Helpers.EnterpriseSettings> EnterpriseSettingsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.GetEnterpriseSettingsAsync(cancellationToken);

            var response = JsonConvert.DeserializeObject<Helpers.EnterpriseSettings>(jsonResponse);

            return response;
        }
    }
}