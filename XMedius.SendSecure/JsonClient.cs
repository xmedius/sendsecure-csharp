using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using XMedius.SendSecure.Constants;

namespace XMedius.SendSecure
{
    public class JsonClient
    {
        private TraceSource TraceSource = new TraceSource("XMedius.SendSecure");

        private static readonly Uri DEFAULT_PORTAL_URI = new Uri("https://portal.xmedius.com");
       
        private string Token;
        private Uri Endpoint;
        private string EnterpriseAccount;
        private Uri SendSecureUrl;
        private string Locale;
        private int UserId;

        public JsonClient() { }

        /// <summary>
        /// JsonClient object constructor.
        /// </summary>
        /// <param name="token">The API Token to be used for authentication with the SendSecure service</param>
        /// <param name="userId">The user id of the current user</param>
        /// <param name="enterpriseAccount">The SendSecure enterprise account</param>
        /// <param name="endpoint">The URL to the SendSecure service</param>
        /// <param name="locale">The locale in which the server errors will be returned ("en" will be used by default if empty)</param>
        /// <param name="cancellationToken"></param>
        public JsonClient(string token, int userId, string enterpriseAccount, Uri endpoint = null, string locale = "en",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (endpoint == null)
            {
                endpoint = DEFAULT_PORTAL_URI;
            }

            Endpoint = endpoint;
            EnterpriseAccount = enterpriseAccount;
            Token = token;
            UserId = userId;
            Locale = locale;
        }

        /// <summary>
        /// Pre-creates a SafeBox on the SendSecure system and initializes the Safebox object accordingly.
        /// </summary>
        /// <param name="userEmail">The email address of a SendSecure user of the current enterprise account</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the guid, public encryption key and upload url of the initialized SafeBox</returns>
        public virtual async Task<string> NewSafeboxAsync(string userEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.NEW_SAFEBOX, userEmail, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Pre-creates a document on the SendSecure system.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the existing safebox</param>
        /// <param name="fileParamsJson">The full json expected by the server</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the temporary document GUID and the upload URL</returns>
        public virtual async Task<string> NewFileAsync(string safeboxGuid, string fileParamsJson, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.NEW_FILE, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPostRequestAsync(fileParamsJson, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Uploads the specified file as an Attachment of the specified SafeBox.
        /// </summary>
        /// <param name="uploadUrl">The url returned by the initializeSafeBox (can be used multiple times)</param>
        /// <param name="filePath">The path of the file to be uploaded</param>
        /// <param name="contentType">The MIME content type of the file to be uploaded</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the guid of the uploaded file</returns>
        public virtual async Task<string> UploadFileAsync(Uri uploadUrl, string filePath, string contentType, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fileInfo = new FileInfo(filePath);
            Stream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return await UploadFileAsync(uploadUrl, fileStream, contentType, fileInfo.Name, fileInfo.Length, cancellationToken);
        }

        /// <summary>
        /// Uploads the specified file as an Attachment of the specified SafeBox.
        /// </summary>
        /// <param name="uploadUrl">The url returned by the initializeSafeBox (can be used multiple times)</param>
        /// <param name="fileStream">The stream of the file to be uploaded</param>
        /// <param name="contentType">The MIME content type of the file to be uploaded</param>
        /// <param name="fileName">The file name</param>
        /// <param name="fileSize">The filesize</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the guid of the uploaded file</returns>
        public virtual async Task<string> UploadFileAsync(Uri uploadUrl, Stream fileStream, string contentType, string fileName, long fileSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var multipartContent = new MultipartFormDataContent("----" + DateTime.Now.Ticks.ToString("x"));
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"file\"",
                    FileName = "\"" + fileName + "\""
                };
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                streamContent.Headers.ContentLength = fileSize;
                multipartContent.Add(streamContent);

                var requestMessage = new HttpRequestMessage
                {
                    RequestUri = uploadUrl,
                    Method = HttpMethod.Post,
                    Content = multipartContent
                };

                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string responseString = await Utils.HttpUtil.MakeRequestAsync(requestMessage, cancellationToken);

                return responseString;
            }
            catch (Exceptions.MakeRequestException e)
            {
                if (e.StatusCode > 0)
                {
                    if (!String.IsNullOrEmpty(e.ResponseString))
                    {
                        throw new Exceptions.SendSecureException((int)e.StatusCode, e.ResponseString, e.Message);
                    }

                    throw new Exceptions.SendSecureException((int)e.StatusCode, e.Message);
                }
                else
                {
                    throw new Exceptions.UnexpectedServerResponseException();
                }

            }
            catch (TaskCanceledException)
            {
                TraceSource.TraceEvent(TraceEventType.Information, 0, "UploadFile Cancelled.");
                throw new OperationCanceledException();
            }
            catch (Exception e)
            {
                TraceSource.TraceEvent(TraceEventType.Error, 0,
                    "Unexpected exception in UploadFile ({0})", e.Message);
                throw new Exceptions.SendSecureException(e.Message);
            }
        }

        /// <summary>
        /// Finalizes the creation (commit) of the SafeBox on the SendSecure system. This actually "Sends" the SafeBox with
        /// all its content and contact info previously specified.
        /// </summary>
        /// <param name="jsonParameters">The full json of the Safebox expected by the server</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing all the informations of the created Safebox</returns>
        public virtual async Task<string> CommitSafeboxAsync(string jsonParameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.COMMIT_SAFEBOX, Locale));

                StringContent jsonContent = new StringContent(jsonParameters);
                jsonContent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                var requestMessage = new HttpRequestMessage
                {
                    RequestUri = resourceAddress,
                    Method = HttpMethod.Post,
                    Content = jsonContent
                };

                requestMessage.Headers.Add("XM-Token-Authorization", Token);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                return await Utils.HttpUtil.MakeRequestAsync(requestMessage, cancellationToken);
            }
            catch (Exceptions.MakeRequestException e)
            {
                if (e.StatusCode > 0)
                {
                    if (!String.IsNullOrEmpty(e.ResponseString))
                    {
                        throw new Exceptions.SendSecureException((int)e.StatusCode, e.ResponseString, e.Message);
                    }

                    throw new Exceptions.SendSecureException((int)e.StatusCode, e.Message);
                }
                else
                {
                    throw new Exceptions.UnexpectedServerResponseException();
                }

            }
            catch (TaskCanceledException)
            {
                TraceSource.TraceEvent(TraceEventType.Information, 0, "UploadFile Cancelled.");
                throw new OperationCanceledException();
            }
            catch (Exception e)
            {
                TraceSource.TraceEvent(TraceEventType.Error, 0,
                    "Unexpected exception in UploadFile ({0})", e.Message);
                throw new Exceptions.SendSecureException(e.Message);
            }
        }

        /// <summary>
        /// Retrieves all available security profiles of the enterprise account for a specific user.
        /// </summary>
        /// <param name="userEmail">The email address of a SendSecure user of the current enterprise account</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing a list of Security Profiles</returns>
        public virtual async Task<string> GetSecurityProfilesAsync(string userEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.SECURITY_PROFILES, EnterpriseAccount, userEmail, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Get the Enterprise Settings of the current enterprise account.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the enterprise settings</returns>
        public virtual async Task<string> GetEnterpriseSettingsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.ENTERPRISE_SETTINGS, EnterpriseAccount, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Get the User Settings of the current user account
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the user settings</returns>
        public virtual async Task<string> GetUserSettingsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.USER_SETTINGS, EnterpriseAccount, UserId, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Retrieves all favorites for the current user account.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing a list of Favorite</returns>
        public virtual async Task<string> GetFavoritesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.GET_CREATE_FAVORITE, EnterpriseAccount, UserId, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Create a new favorite for the current user account.
        /// </summary>
        /// <param name="json">The json of the favorite expected by the server</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing all the informations of the created Favorite</returns>
        public virtual async Task<string> CreateFavoriteAsync(string json, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.GET_CREATE_FAVORITE, EnterpriseAccount, UserId, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPostRequestAsync(json, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Update an existing favorite for the current user account.
        /// </summary>
        /// <param name="json">The json of the favorite expected by the server</param>
        /// <param name="favoriteId">The id of the favorite to be updated</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing all the informations of the updated Favorite</returns>
        public virtual async Task<string> EditFavoriteAsync(string json, int? favoriteId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.EDIT_DELETE_FAVORITE, EnterpriseAccount, UserId, favoriteId, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPatchRequestAsync(json, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Delete an existing favorite for the current user account.
        /// </summary>
        /// <param name="favoriteId">The id of the favorite to be deleted</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Nothing</returns>
        public virtual async Task<string> DeleteFavoriteAsync(int? favoriteId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.EDIT_DELETE_FAVORITE, EnterpriseAccount, UserId, favoriteId, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedDeleteRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Create a new participant for a specific open safebox of the current user account.
        /// </summary>
        /// <param name="json">The json of the participant expected by the server</param>
        /// <param name="safeboxGuid">The guid of the safebox to be updated</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing all the informations of the created Participant</returns>
        public virtual async Task<string> CreateParticipantAsync(string json, string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.GET_CREATE_PARTICIPANT, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPostRequestAsync(json, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Update an existing participant for a specific open safebox of the current user account.
        /// </summary>
        /// <param name="json">The json of the participant expected by the server</param>
        /// <param name="safeboxGuid"></param>
        /// <param name="participantId">The guid of the safebox to be updated</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing all the informations of the updated Participant</returns>
        public virtual async Task<string> UpdateParticipantAsync(string json, string safeboxGuid, string participantId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.UPDATE_PARTICIPANT, safeboxGuid, participantId, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPatchRequestAsync(json, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Search the recipients for a safebox
        /// </summary>
        /// <param name="term">A Search term</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the search result</returns>
        public virtual async Task<string> SearchRecipientAsync(string term, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.SEARCH_RECIPIENT, term, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Manually add time to expiration date for a specific open safebox of the current user account.
        /// </summary>
        /// <param name="json">The time json expected by the server</param>
        /// <param name="safeboxGuid">The guid of the safebox to be updated</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the new expiration date</returns>
        public virtual async Task<string> AddTimeAsync(string json, string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.ADD_TIME, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPatchRequestAsync(json, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Manually close an existing safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox to be closed</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the request result</returns>
        public virtual async Task<string> CloseSafeboxAsync(string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.CLOSE_SAFEBOX, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPatchRequestAsync(String.Empty, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Manually delete the content of a closed safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the request result</returns>
        public virtual async Task<string> DeleteSafeboxContentAsync(string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.DELETE_SAFEBOX_CONTENT, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPatchRequestAsync(String.Empty, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Manually mark as read an existing safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the request result</returns>
        public virtual async Task<string> MarkAsReadAsync(string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.MARK_AS_READ, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPatchRequestAsync(String.Empty, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Manually mark as unread an existing safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the request result</returns>
        public virtual async Task<string> MarkAsUnreadAsync(string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.MARK_AS_UNREAD, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPatchRequestAsync(String.Empty, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Manually mark as read an existing message.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="messageId">The id of the message to be marked as read</param>
        /// <param name="cancellationToken">The json containing the request result</param>
        /// <returns></returns>
        public virtual async Task<string> MarkAsReadMessageAsync(string safeboxGuid, string messageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.MARK_AS_READ_MESSAGE, safeboxGuid, messageId, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPatchRequestAsync(String.Empty, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Manually mark as unread an existing message.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="messageId">The id of the message to be marked as unread</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the request result</returns>
        public virtual async Task<string> MarkAsUnreadMessageAsync(string safeboxGuid, string messageId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.MARK_AS_UNREAD_MESSAGE, safeboxGuid, messageId, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPatchRequestAsync(String.Empty, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve a specific file url of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="documentGuid">The guid of the file</param>
        /// <param name="user_email">The current user email</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the file url on the fileserver</returns>
        public virtual async Task<string> GetFileUrlAsync(string safeboxGuid, string documentGuid, string user_email, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.FILE_URL, safeboxGuid, documentGuid, user_email, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve the url of the audit record of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the url</returns>
        public virtual async Task<string> GetAuditRecordUrlAsync(string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.AUDIT_RECORD, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve the audit record of an existing safebox for the current user account.
        /// </summary>
        /// <param name="url">The url of the safebox audit record</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The pdf stream</returns>
        public virtual async Task<string> GetAuditRecordPdfAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(new Uri(url), Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve the list of safeboxes for the current user account.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the count, previous page url, the next page url and a list of Safebox</returns>
        public virtual async Task<string> GetSafeboxesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.SAFEBOXES, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve a filtered list of safeboxes for the current user account.
        /// </summary>
        /// <param name="status">Filter by SafeBox status [in_progress, closed, content_deleted, unread]</param>
        /// <param name="search">Search term in SafeBox subject and ID, message text, participant email, first name and last name, file name and fingerprint</param>
        /// <param name="per_page">Splits the list in several pages, with 0 < per_page <= 1000 (default is 100)</param>
        /// <param name="page">Selects the page to return</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the count, previous page url, the next page url and a list of Safebox</returns>
        public virtual async Task<string> GetSafeboxesAsync(string status, string search, int per_page, int page, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.SAFEBOXES_FILTER, status, search, per_page, page, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve the list of safeboxes for the current user account with an URL.
        /// </summary>
        /// <param name="url">The complete search url</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the count, previous page url, the next page url and a list of Safebox</returns>
        public virtual async Task<string> GetSafeboxesAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(new Uri(url), Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve all info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox to be updated</param>
        /// <param name="sections">The string containing the list of sections to be retrieved</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing all the informations on the specified sections.
        ///          If no sections are specified, it will return all safebox infos.</returns>
        public virtual async Task<string> GetSafeboxInfoAsync(string safeboxGuid, string sections, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = String.IsNullOrEmpty(sections) ? new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.SAFEBOX_INFO, safeboxGuid, Locale))
                                                                 : new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.SAFEBOX_INFO_SECTIONS, safeboxGuid, sections, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve all participants info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the list of participants</returns>
        public virtual async Task<string> GetSafeboxParticipantsAsync(string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.GET_CREATE_PARTICIPANT, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve all messages info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the list of messages</returns>
        public virtual async Task<string> GetSafeboxMessagesAsync(string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.MESSAGES, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve all security options info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the Security Options</returns>
        public virtual async Task<string> GetSafeboxSecurityOptionsAsync(string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.SECURITY_OPTIONS, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve all download activity info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the Download Activity</returns>
        public virtual async Task<string> GetSafeboxDownloadActivityAsync(string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.DOWNLOAD_ACTIVITY, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Retrieve all event_history info of an existing safebox for the current user account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing a list of EventHistory</returns>
        public virtual async Task<string> GetSafeboxEventHistoryAsync(string safeboxGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.EVENT_HISTORY, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Reply to a specific safebox associated to the current user's account.
        /// </summary>
        /// <param name="safeboxGuid">The guid of the safebox</param>
        /// <param name="replyJson">The reply json expected by the server</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the request result</returns>
        public virtual async Task<string> ReplyAsync(string safeboxGuid, string replyJson, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.REPLY, safeboxGuid, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedPostRequestAsync(replyJson, resourceAddress, Token, cancellationToken);
        }

        /// <summary>
        /// Call to get the list of all the localized messages of a consent group.
        /// </summary>
        /// <param name="consentGroupId">The id of the consent group</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The json containing the list of all the localized messages</returns>
        public virtual async Task<string> GetConsentGroupMessagesAsync(int consentGroupId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpointAsync(cancellationToken), String.Format(Constants.Path.CONSENT_GROUP, EnterpriseAccount, consentGroupId, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequestAsync(resourceAddress, Token, cancellationToken);
        }

        private async Task<Uri> GetSendSecureEndpointAsync(CancellationToken cancellationToken)
        {
            if (SendSecureUrl == null)
            {
                SendSecureUrl = new Uri(await Utils.SendSecureUrlUtil.GetSendSecureUrlForEnterpriseAccountAsync(EnterpriseAccount, Endpoint, default(CancellationToken)));
            }

            return SendSecureUrl;
        }


    }
}
