using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XMedius.SendSecure
{
    public class JsonClient
    {
        private TraceSource TraceSource = new TraceSource("XMedius.SendSecure");

        private static readonly Uri DEFAULT_PORTAL_URI = new Uri("https://portal.xmedius.com");
        private static readonly string NEW_SAFEBOX_PATH = "api/v2/safeboxes/new?user_email={0}&locale={1}";
        private static readonly string SECURITY_PROFILES_PATH = "api/v2/enterprises/{0}/security_profiles?user_email={1}&locale={2}";
        private static readonly string ENTERPRISE_SETTINGS_PATH = "api/v2/enterprises/{0}/settings?locale={1}";
        private static readonly string COMMIT_SAFEBOX_PATH = "api/v2/safeboxes?locale={0}";

        private string Token;
        private Uri Endpoint;
        private string EnterpriseAccount;
        private Uri SendSecureUrl;
        private string Locale;

        public JsonClient(string token, string enterpriseAccount, Uri endpoint = null, string locale = "en",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (endpoint == null)
            {
                endpoint = DEFAULT_PORTAL_URI;
            }

            Endpoint = endpoint;
            EnterpriseAccount = enterpriseAccount;
            Token = token;
            Locale = locale;
        }

        public async Task<string> NewSafebox(string userEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpoint(cancellationToken), String.Format(NEW_SAFEBOX_PATH, userEmail, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequest(resourceAddress, Token, cancellationToken);
        }

        public async Task<string> UploadFile(Uri uploadUrl, string filePath, string contentType, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fileInfo = new FileInfo(filePath);
            Stream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await UploadFile(uploadUrl, fileStream, contentType, fileInfo.Name, fileInfo.Length, cancellationToken);
        }

        public async Task<string> UploadFile(Uri uploadUrl, Stream fileStream, string contentType, string fileName, long fileSize, CancellationToken cancellationToken = default(CancellationToken))
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

                string responseString = await Utils.HttpUtil.MakeRequest(requestMessage, cancellationToken);

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

        public async Task<string> CommitSafebox(string jsonParameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                Uri resourceAddress = new Uri(await GetSendSecureEndpoint(cancellationToken), String.Format(COMMIT_SAFEBOX_PATH, Locale));

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

                return await Utils.HttpUtil.MakeRequest(requestMessage, cancellationToken);
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

        public async Task<string> GetSecurityProfiles(string userEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpoint(cancellationToken), String.Format(SECURITY_PROFILES_PATH, EnterpriseAccount, userEmail, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequest(resourceAddress, Token, cancellationToken);
        }

        public async Task<string> GetEnterpriseSettings(CancellationToken cancellationToken = default(CancellationToken))
        {
            Uri resourceAddress = new Uri(await GetSendSecureEndpoint(cancellationToken), String.Format(ENTERPRISE_SETTINGS_PATH, EnterpriseAccount, Locale));

            return await Utils.HttpUtil.MakeAuthenticatedGetRequest(resourceAddress, Token, cancellationToken);
        }

        private async Task<Uri> GetSendSecureEndpoint(CancellationToken cancellationToken)
        {
            if (SendSecureUrl == null)
            {
                SendSecureUrl = new Uri(await Utils.SendSecureUrlUtil.GetSendSecureUrlForEnterpriseAccount(EnterpriseAccount, Endpoint, default(CancellationToken)));
            }

            return SendSecureUrl;
        }


    }
}
