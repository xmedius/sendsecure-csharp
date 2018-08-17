using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
        private int UserId;
        internal static JsonClient JsonClient;
        private string Locale;

        /// <summary>
        /// Client object constructor.
        /// </summary>
        /// <param name="jsonClient">An existing JsonClient</param>
        /// <param name="enterpriseAccount">The SendSecure enterprise account</param>
        /// <param name="endpoint">The URL to the SendSecure service</param>
        /// <param name="locale">The locale in which the server errors will be returned ("en" will be used by default if empty)</param>
        public Client(JsonClient jsonClient, string enterpriseAccount, Uri endpoint, string locale = "en")
        {
            EnterpriseAccount = enterpriseAccount;
            EndPoint = endpoint;
            Locale = locale;
            JsonClient = jsonClient;
        }

        /// <summary>
        /// Client object constructor.
        /// </summary>
        /// <param name="token">The API Token to be used for authentication with the SendSecure service</param>
        /// <param name="userId">The user id of the current user</param>
        /// <param name="enterpriseAccount">The SendSecure enterprise account</param>
        /// <param name="endpoint">The URL to the SendSecure service</param>
        /// <param name="locale">The locale in which the server errors will be returned ("en" will be used by default if empty)</param>
        public Client(string token, int userId, string enterpriseAccount, Uri endpoint, string locale = "en")
        {
            EnterpriseAccount = enterpriseAccount;
            ApiToken = token;
            UserId = userId;
            EndPoint = endpoint;
            Locale = locale;
            JsonClient = new JsonClient(ApiToken, UserId, EnterpriseAccount, EndPoint);
        }

        /// <summary>
        /// Gets an API Token for a specific user within a SendSecure enterprise account.
        /// </summary>
        /// <param name="enterpriseAccount">The SendSecure enterprise account</param>
        /// <param name="username">The username of a SendSecure user of the current enterprise account</param>
        /// <param name="password">The password of this user</param>
        /// <param name="deviceId">The unique ID of the device used to get the Token</param>
        /// <param name="deviceName">The name of the device used to get the Token</param>
        /// <param name="applicationType">The type/name of the application used to get the Token ("SendSecure C#" will be used by default if empty)</param>
        /// <param name="endpoint">The URL to the SendSecure service</param>
        /// <param name="oneTimePassword">The one-time password of this user (if any)</param>
        /// <param name="cancellationToken"></param>
        /// <returns>(API Token, user ID) to be used for the specified user</returns>
        public static async Task<JsonObjects.UserToken> GetUserTokenAsync(string enterpriseAccount, string username, string password, string deviceId,
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

                var ResponseData = JsonConvert.DeserializeObject<JsonObjects.UserToken>(responseString);

                return ResponseData;
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

        /// <summary>
        /// This method is a high-level combo that initializes the SafeBox, uploads all attachments and commits the SafeBox.
        /// </summary>
        /// <param name="safebox">A non-initialized Safebox object with security profile, recipient(s), subject,
        ///                       message and attachments(not yet uploaded) already defined.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated safebox</returns>
        public async Task<Helpers.Safebox> SubmitSafeboxAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            await InitializeSafeboxAsync(safebox, cancellationToken);

            foreach (Helpers.Attachment attachment in safebox.Attachments)
            {
                await UploadAttachmentAsync(safebox, attachment, cancellationToken);
            }

            if (safebox.SecurityProfileId == null)
            {
                var SecurityProfile = await DefaultSecurityProfileAsync(safebox.UserEmail, cancellationToken);
                safebox.SecurityProfileId = SecurityProfile.Id;
                if (safebox.SecurityProfileId == null)
                {
                    throw new Exceptions.SendSecureException("No Security Profile is configured");
                }
            }

            return await CommitSafeboxAsync(safebox, cancellationToken);
        }

        /// <summary>
        /// Pre-creates a SafeBox on the SendSecure system and initializes the Safebox object accordingly.
        /// </summary>
        /// <param name="safebox">A SafeBox object to be finalized by the SendSecure system</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated SafeBox object with the necessary system parameters (GUID, public encryption key, upload URL) filled out</returns>
        public async Task<Helpers.Safebox> InitializeSafeboxAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.NewSafeboxAsync(safebox.UserEmail, cancellationToken);

            safebox.Update(jsonResponse);

            return safebox;
        }

        /// <summary>
        /// Uploads the specified file as an Attachment of the specified SafeBox.
        /// </summary>
        /// <param name="safebox">An initialized Safebox object</param>
        /// <param name="attachment">An Attachment object - the file to upload to the SendSecure system</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated Attachment object with the GUID parameter filled out</returns>
        public async Task<Helpers.Attachment> UploadAttachmentAsync(Helpers.Safebox safebox, Helpers.Attachment attachment, CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.UploadFileAsync(new Uri(safebox.UploadUrl), attachment.Stream, attachment.ContentType, attachment.FileName, attachment.Size, cancellationToken);

            var response = JsonConvert.DeserializeObject<JsonObjects.UploadFileResponseSuccess>(jsonResponse);

            attachment.Guid = response.TemporaryDocument.DocumentGuid;

            return attachment;
        }

        /// <summary>
        /// This actually "Sends" the SafeBox with all content and contact info previously specified.
        /// </summary>
        /// <param name="safebox">A Safebox object already finalized, with security profile, recipient(s),
        ///                       subject and message already defined, and attachments already uploaded.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated safebox</returns>
        public async Task<Helpers.Safebox> CommitSafeboxAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            if (safebox.Participants.Count == 0)
            {
                throw new Exceptions.SendSecureException("Participants cannot be empty");
            }

            string jsonResponse = await JsonClient.CommitSafeboxAsync(safebox.ToJson(), cancellationToken);

            safebox.Update(jsonResponse);

            return safebox;
        }

        /// <summary>
        /// Retrieves all available security profiles of the enterprise account for a specific user.
        /// </summary>
        /// <param name="userEmail">The email address of a SendSecure user of the current enterprise account</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The list of all security profiles of the enterprise account, with all their setting values/properties</returns>
        public async Task<List<Helpers.SecurityProfile>> SecurityProfilesAsync(string userEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.GetSecurityProfilesAsync(userEmail, cancellationToken);

            var response = JsonConvert.DeserializeObject<JsonObjects.SecurityProfilesResponseSuccess>(jsonResponse);

            return response.SecurityProfiles;
        }

        /// <summary>
        /// Retrieves the default security profile of the enterprise account for a specific user.
        /// A default security profile must have been set in the enterprise account, otherwise the method will return nothing.
        /// </summary>
        /// <param name="userEmail">The email address of a SendSecure user of the current enterprise account</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The default security profile of the enterprise, with all its setting values/properties</returns>
        public async Task<Helpers.SecurityProfile> DefaultSecurityProfileAsync(string userEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            var enterpriseSettings = await EnterpriseSettingsAsync(cancellationToken);
            var securityProfiles = await SecurityProfilesAsync(userEmail, cancellationToken);

            return securityProfiles.Find(x => x.Id == enterpriseSettings.DefaultSecurityProfileId);
        }

        /// <summary>
        /// Retrieves all the current enterprise account's settings specific to a SendSecure Account.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>All values/properties of the enterprise account's settings specific to SendSecure</returns>
        public async Task<Helpers.EnterpriseSettings> EnterpriseSettingsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.GetEnterpriseSettingsAsync(cancellationToken);

            return Helpers.EnterpriseSettings.FromJson(jsonResponse);
        }

        /// <summary>
        /// Retrieves all the current user account's settings specific to SendSecure Account.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>All values/properties of the user account's settings specific to SendSecure</returns>
        public async Task<Helpers.UserSettings> UserSettingsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.GetUserSettingsAsync(cancellationToken);

            return Helpers.UserSettings.FromJson(jsonResponse);
        }

        /// <summary>
        /// Retrieves all favorites associated to a specific user.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The list of all favorites of the user account, with all their properties</returns>
        public async Task<List<Helpers.Favorite>> FavoritesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.GetFavoritesAsync(cancellationToken);

            var response = JsonConvert.DeserializeObject<JsonObjects.GetFavoritesResponseSuccess>(jsonResponse);

            return response.Favorites;
        }

        /// <summary>
        /// Creates a new favorite associated to a specific user.
        /// </summary>
        /// <param name="favorite">A Favorite object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated Favorite</returns>
        public async Task<Helpers.Favorite> CreateFavoriteAsync(Helpers.Favorite favorite, CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.CreateFavoriteAsync(favorite.UpdateFor<Helpers.Favorite.Creation>().ToJson(), cancellationToken);

            favorite.Update(jsonResponse);

            return favorite;
        }

        /// <summary>
        /// Update an existing favorite associated to a specific user.
        /// </summary>
        /// <param name="favorite">A Favorite object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated Favorite</returns>
        public async Task<Helpers.Favorite> EditFavoriteAsync(Helpers.Favorite favorite, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (favorite.Id == null)
            {
                throw new Exceptions.SendSecureException("Favorite id cannot be null");
            }

            string jsonResponse = await JsonClient.EditFavoriteAsync(favorite.UpdateFor<Helpers.Favorite.Edition>().ToJson(), favorite.Id, cancellationToken);

            favorite.Update(jsonResponse);

            return favorite;
        }

        /// <summary>
        /// Delete contact methods of an existing favorite associated to a specific user.
        /// </summary>
        /// <param name="favorite">A Favorite object</param>
        /// <param name="contactMethodsIds">A list of contact methods ids</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated Favorite</returns>
        public async Task<Helpers.Favorite> DeleteFavoriteContactMethods(Helpers.Favorite favorite, List<int?> contactMethodsIds, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (favorite.Id == null)
            {
                throw new Exceptions.SendSecureException("Favorite id cannot be null");
            }

            string jsonResponse = await JsonClient.EditFavoriteAsync(favorite.UpdateFor<Helpers.Favorite.Destruction>().OfFollowingContacts(contactMethodsIds).ToJson(), favorite.Id, cancellationToken);

            favorite.Update(jsonResponse);

            return favorite;
        }

        /// <summary>
        /// Delete an existing favorite associated to a specific user.
        /// </summary>
        /// <param name="favorite">A Favorite object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Nothing</returns>
        public async Task<string> DeleteFavoriteAsync(Helpers.Favorite favorite, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (favorite.Id == null)
            {
                throw new Exceptions.SendSecureException("Favorite id cannot be null");
            }

            return await JsonClient.DeleteFavoriteAsync(favorite.Id, cancellationToken);
        }

        /// <summary>
        /// Create a new participant for a specific safebox associated to the current user's account,
        /// and add the new participant to the Safebox object.
        /// </summary>
        /// <param name="participant">A Participant object</param>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated Participant</returns>
        public async Task<Helpers.Participant> CreateParticipantAsync(Helpers.Participant participant, Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            if (participant.Email == null)
            {
                throw new Exceptions.SendSecureException("Participant email cannot be null");
            }

            string jsonResponse = await JsonClient.CreateParticipantAsync(participant.UpdateFor<Helpers.Participant.Creation>().ToJson(), safebox.Guid, cancellationToken);

            participant.Update(jsonResponse);
            safebox.Participants.Add(participant);

            return participant;
        }

        /// <summary>
        /// Update an existing participant of a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="participant">A Participant object</param>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated Participant</returns>
        public async Task<Helpers.Participant> UpdateParticipantAsync(Helpers.Participant participant, Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            if (participant.Id == null)
            {
                throw new Exceptions.SendSecureException("Participant Id cannot be null");
            }

            string jsonResponse = await JsonClient.UpdateParticipantAsync(participant.UpdateFor<Helpers.Participant.Edition>().ToJson(), safebox.Guid, participant.Id, cancellationToken);

            participant.Update(jsonResponse);

            return participant;
        }

        /// <summary>
        /// Delete contact methods of an existing participant of a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="participant">A Participant object</param>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="contactMethodsIds">A list of contact method id</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated Participant</returns>
        public async Task<Helpers.Participant> DeleteParticipantContactMethods(Helpers.Participant participant, Helpers.Safebox safebox, List<int?> contactMethodsIds, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            if (participant.Id == null)
            {
                throw new Exceptions.SendSecureException("Participant id cannot be null");
            }

            string jsonResponse = await JsonClient.UpdateParticipantAsync(participant.UpdateFor<Helpers.Participant.Destruction>().OfFollowingContacts(contactMethodsIds).ToJson(), safebox.Guid, participant.Id, cancellationToken);

            participant.Update(jsonResponse);

            return participant;
        }

        /// <summary>
        /// Search the recipients for a SafeBox
        /// </summary>
        /// <param name="term">A Search term</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The list of recipients that matches the search term</returns>
        public async Task<JsonObjects.SearchRecipientResponseSuccess> SearchRecipientAsync(string term, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (term == null)
            {
                throw new Exceptions.SendSecureException("Search term cannot be null");
            }

            string jsonResponse = await JsonClient.SearchRecipientAsync(term, cancellationToken);

            return JsonObjects.SearchRecipientResponseSuccess.FromJson(jsonResponse);
        }

        /// <summary>
        /// Add time to the expiration date of a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="timeValue">Time value to be added to the expiration date</param>
        /// <param name="timeUnit">Time unit to be added to the expiration date</param>
        /// <param name="cancellationToken"></param>
        /// <returns>AddTimeResponseSuccess object containing the request result</returns>
        public async Task<JsonObjects.AddTimeResponseSuccess> AddTimeAsync(Helpers.Safebox safebox, int timeValue, Helpers.SecurityEnums.TimeUnit timeUnit, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            var json = "{\"safebox\":{\"add_time_value\":" + timeValue + ",\"add_time_unit\":\"" + Helpers.SecurityEnums.TimeUnitToString(timeUnit) + "\"}}";
            string jsonResponse = await JsonClient.AddTimeAsync(json, safebox.Guid, cancellationToken);
            var response = JsonConvert.DeserializeObject<JsonObjects.AddTimeResponseSuccess>(jsonResponse);

            safebox.Expiration = response.NewExpiration;

            return response;
        }

        /// <summary>
        /// Close a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>RequestResponse object containing the request result</returns>
        public async Task<JsonObjects.RequestResponse> CloseSafeboxAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            string jsonResponse = await JsonClient.CloseSafeboxAsync(safebox.Guid, cancellationToken);

            return JsonObjects.RequestResponse.FromJson(jsonResponse);
        }

        /// <summary>
        /// Delete content of a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>RequestResponse object containing the request result</returns>
        public async Task<JsonObjects.RequestResponse> DeleteSafeboxContentAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            string jsonResponse = await JsonClient.DeleteSafeboxContentAsync(safebox.Guid, cancellationToken);

            return JsonObjects.RequestResponse.FromJson(jsonResponse);
        }

        /// <summary>
        /// Mark all messages as read of a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>RequestResponse object containing the request result</returns>
        public async Task<JsonObjects.RequestResponse> MarkAsReadAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            string jsonResponse = await JsonClient.MarkAsReadAsync(safebox.Guid, cancellationToken);

            return JsonObjects.RequestResponse.FromJson(jsonResponse);
        }

        /// <summary>
        /// Mark all messages as unread of a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>RequestResponse object containing the request result</returns>
        public async Task<JsonObjects.RequestResponse> MarkAsUnreadAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            string jsonResponse = await JsonClient.MarkAsUnreadAsync(safebox.Guid, cancellationToken);

            return JsonObjects.RequestResponse.FromJson(jsonResponse);
        }

        /// <summary>
        /// Mark a message as read of a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="message">A Message object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>RequestResponse object containing the request result</returns>
        public async Task<JsonObjects.RequestResponse> MarkAsReadMessageAsync(Helpers.Safebox safebox, Helpers.Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            if (message.Id == null)
            {
                throw new Exceptions.SendSecureException("Message Id cannot be null");
            }

            string jsonResponse = await JsonClient.MarkAsReadMessageAsync(safebox.Guid, message.Id, cancellationToken);

            return JsonObjects.RequestResponse.FromJson(jsonResponse);
        }

        /// <summary>
        /// Mark a message as unread of a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="message">A Message object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>RequestResponse object containing the request result</returns>
        public async Task<JsonObjects.RequestResponse> MarkAsUnreadMessageAsync(Helpers.Safebox safebox, Helpers.Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            if (message.Id == null)
            {
                throw new Exceptions.SendSecureException("Message Id cannot be null");
            }

            string jsonResponse = await JsonClient.MarkAsUnreadMessageAsync(safebox.Guid, message.Id, cancellationToken);

            return JsonObjects.RequestResponse.FromJson(jsonResponse);
        }

        /// <summary>
        /// Retrieve a specific file url of a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="attachment">An Attachment object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The file url</returns>
        public async Task<string> FileUrlAsync(Helpers.Safebox safebox, Helpers.Attachment attachment, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            if (attachment.Guid == null)
            {
                throw new Exceptions.SendSecureException("Document Guid cannot be null");
            }

            string jsonResponse = await JsonClient.GetFileUrlAsync(safebox.Guid, attachment.Guid, safebox.UserEmail, cancellationToken);
            var response = JObject.Parse(jsonResponse);

            return response["url"].Value<string>();
        }

        /// <summary>
        /// Retrieve the audit record url of a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The Audit Record Url</returns>
        public async Task<string> AuditRecordUrlAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            string jsonResponse = await JsonClient.GetAuditRecordUrlAsync(safebox.Guid, cancellationToken);
            var response = JObject.Parse(jsonResponse);

            return response["url"].Value<string>();
        }

        /// <summary>
        /// Retrieve the audit record pdf of a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The audit record pdf stream</returns>
        public async Task<string> AuditRecordPdfAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            string url = await AuditRecordUrlAsync(safebox, cancellationToken);

            return await JsonClient.GetAuditRecordPdfAsync(url, cancellationToken);
        }

        /// <summary>
        /// Retrieve a filtered list of safeboxes for the current user account.
        /// </summary>
        /// <param name="status">Filter by SafeBox status [in_progress, closed, content_deleted, unread]</param>
        /// <param name="search">Search term in SafeBox subject and ID, message text, participant email, first name and last name, file name and fingerprint</param>
        /// <param name="per_page">Splits the list in several pages, with 0 < per_page <= 1000 (default is 100)</param>
        /// <param name="page">Selects the page to return</param>
        /// <param name="cancellationToken"></param>
        /// <returns>SafeboxesResponse containing the count, previous page url, the next page url and a list of Safebox</returns>
        public async Task<JsonObjects.SafeboxesResponse> SafeboxesAsync(Helpers.Safebox.Status status, string search, int per_page, int page, CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.GetSafeboxesAsync(Helpers.Safebox.StatusToString(status), search, per_page, page, cancellationToken);
            var response = JObject.Parse(jsonResponse);

            var safeboxes = new JArray();
            foreach (JObject safebox in response["safeboxes"])
            {
                safeboxes.Add(safebox["safebox"]);
            }
            response["safeboxes"] = safeboxes;

            return JsonObjects.SafeboxesResponse.FromJson(response.ToString());
        }

        /// <summary>
        /// Retrieve the list of safeboxes for the current user account with an URL.
        /// </summary>
        /// <param name="url">The complete search url</param>
        /// <param name="cancellationToken"></param>
        /// <returns>SafeboxesResponse containing the count, previous page url, the next page url and a list of Safebox</returns>
        public async Task<JsonObjects.SafeboxesResponse> SafeboxesAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.GetSafeboxesAsync(url, cancellationToken);
            var response = JObject.Parse(jsonResponse);

            var safeboxes = new JArray();
            foreach (JObject safebox in response["safeboxes"])
            {
                safeboxes.Add(safebox["safebox"]);
            }
            response["safeboxes"] = safeboxes;

            return JsonObjects.SafeboxesResponse.FromJson(response.ToString());
        }

        /// <summary>
        /// Retrieve the list of safeboxes for the current user account.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>SafeboxesResponse containing the count, previous page url, the next page url and a list of Safebox</returns>
        public async Task<JsonObjects.SafeboxesResponse> SafeboxesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string jsonResponse = await JsonClient.GetSafeboxesAsync(cancellationToken);
            var response = JObject.Parse(jsonResponse);

            var safeboxes = new JArray();
            foreach (JObject safebox in response["safeboxes"])
            {
                safeboxes.Add(safebox["safebox"]);
            }
            response["safeboxes"] = safeboxes;

            return JsonObjects.SafeboxesResponse.FromJson(response.ToString());
        }

        /// <summary>
        /// Retrieve all info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="sections">The list of sections to be retrieved</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated safebox containing all the informations on the specified sections.
        ///          If no sections are specified, it will return all safebox infos.</returns>
        public async Task<Helpers.Safebox> SafeboxInfoAsync(Helpers.Safebox safebox, string[] sections, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            string parameters = sections != null ? String.Join(",", sections) : "";
            string jsonResponse = await JsonClient.GetSafeboxInfoAsync(safebox.Guid, parameters, cancellationToken);
            var response = JObject.Parse(jsonResponse);

            safebox.Update(response["safebox"].ToString());

            return safebox;
        }

        /// <summary>
        /// Retrieve all participants info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The list of participants</returns>
        public async Task<List<Helpers.Participant>> ParticipantsAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            string jsonResponse = await JsonClient.GetSafeboxParticipantsAsync(safebox.Guid, cancellationToken);

            safebox.Update(jsonResponse);

            return safebox.Participants;
        }

        /// <summary>
        /// Retrieve all messages info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The list of messages</returns>
        public async Task<List<Helpers.Message>> MessagesAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            string jsonResponse = await JsonClient.GetSafeboxMessagesAsync(safebox.Guid, cancellationToken);

            safebox.Update(jsonResponse);

            return safebox.Messages;
        }

        /// <summary>
        /// Retrieve all event_history info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The list of EventHistory</returns>
        public async Task<List<Helpers.EventHistory>> EventHistoryAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            string jsonResponse = await JsonClient.GetSafeboxEventHistoryAsync(safebox.Guid, cancellationToken);

            safebox.Update(jsonResponse);

            return safebox.EventHistory;
        }

        /// <summary>
        /// Retrieve all security options info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The Security Options</returns>
        public async Task<Helpers.SecurityOptions> SecurityOptionsAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            string jsonResponse = await JsonClient.GetSafeboxSecurityOptionsAsync(safebox.Guid, cancellationToken);

            safebox.Update(jsonResponse);

            return safebox.SecurityOptions;
        }

        /// <summary>
        /// Retrieve all download activity info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The Download Activity</returns>
        public async Task<Helpers.DownloadActivity> DownloadActivityAsync(Helpers.Safebox safebox, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            string jsonResponse = await JsonClient.GetSafeboxDownloadActivityAsync(safebox.Guid, cancellationToken);

            safebox.Update(jsonResponse);

            return safebox.DownloadActivity;
        }

        /// <summary>
        /// Retrieve a specific Safebox by its guid.
        /// </summary>
        /// <param name="safeboxGuid">The safebox GUID</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A Safebox object</returns>
        public async Task<Helpers.Safebox> GetSafeboxAsync(string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            var jsonResponse = await JsonClient.GetSafeboxInfoAsync(safeboxGuid, String.Empty, cancellationToken);
            var response = JObject.Parse(jsonResponse);
            return Helpers.Safebox.FromJson(response["safebox"].ToString());
        }

        /// <summary>
        /// Reply to a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safebox">A Safebox object</param>
        /// <param name="reply">A Reply object</param>
        /// <param name="cancellationToken"></param>
        /// <returns>RequestResponse object containing the request result</returns>
        public async Task<JsonObjects.RequestResponse> ReplyAsync(Helpers.Safebox safebox, Helpers.Reply reply, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (safebox.Guid == null)
            {
                throw new Exceptions.SendSecureException("Safebox Guid cannot be null");
            }

            foreach(Helpers.Attachment attachment in reply.Attachments)
            {
                var fileParamsJson = safebox.TemporaryDocument(attachment.Size);
                var newFileResponseJson = await JsonClient.NewFileAsync(safebox.Guid, fileParamsJson, cancellationToken);
                var newFileResponse = JsonObjects.NewFileResponseSuccess.FromJson(newFileResponseJson);
                var uploadResponseJson = await JsonClient.UploadFileAsync(new Uri(newFileResponse.UploadUrl), attachment.Stream, attachment.ContentType, attachment.FileName, attachment.Size, cancellationToken);
                var uploadResponse = JsonConvert.DeserializeObject<JsonObjects.UploadFileResponseSuccess>(uploadResponseJson);
                reply.DocumentIds.Add(uploadResponse.TemporaryDocument.DocumentGuid);
            };

            var response = await JsonClient.ReplyAsync(safebox.Guid, reply.ToJson(), cancellationToken);
            return JsonObjects.RequestResponse.FromJson(response);
        }

        /// <summary>
        /// Call to get the list of all the localized messages of a consent group.
        /// </summary>
        /// <param name="consentGroupId">The id of the consent group</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The list of all the localized messages</returns>
        public async Task<Helpers.ConsentMessageGroup> GetConsentGroupMessagesAsync(int consentGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var jsonResponse = await JsonClient.GetConsentGroupMessagesAsync(consentGroupId, cancellationToken);
            var response = JObject.Parse(jsonResponse);
            return Helpers.ConsentMessageGroup.FromJson(response["consent_message_group"].ToString());
        }

    }
}
