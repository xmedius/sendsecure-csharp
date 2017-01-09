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

namespace XMedius.SendSecure.Utils
{
    internal class SendSecureUrlUtil
    {
        private static readonly string PORTAL_HOST_PATH = "services/{0}/portal/host";
        private static readonly string SENDSECURE_SERVER_URL_PATH = "services/{0}/sendsecure/server/url";

        private static TraceSource TraceSource = new TraceSource("XMedius.SendSecure");

        internal static async Task<string> GetPortalUrlForEnterpriseAccountAsync(string enterpriseAccount, Uri portalUrl, CancellationToken cancellationToken)
        {
            Uri resourceAddress = new Uri(portalUrl, String.Format(PORTAL_HOST_PATH, enterpriseAccount));

            var requestMessage = new HttpRequestMessage
            {
                RequestUri = resourceAddress,
                Method = HttpMethod.Get,
            };

            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

            string responseString = await HttpUtil.MakeRequestAsync(requestMessage, cancellationToken);

            return responseString;
        }

        internal static async Task<string> GetSendSecureUrlForEnterpriseAccountAsync(string enterpriseAccount, Uri portalUrl, CancellationToken cancellationToken)
        {
            try
            {
                Uri resourceAddress = new Uri(portalUrl, String.Format(SENDSECURE_SERVER_URL_PATH, enterpriseAccount));

                var requestMessage = new HttpRequestMessage
                {
                    RequestUri = resourceAddress,
                    Method = HttpMethod.Get,
                };

                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                string responseString = await HttpUtil.MakeRequestAsync(requestMessage, cancellationToken);

                return responseString;
            }
            catch (Exceptions.MakeRequestException e)
            {
                if (e.StatusCode > 0)
                {
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
                TraceSource.TraceEvent(TraceEventType.Information, 0, "GetSendSecureUrlForEnterpriseAccount Cancelled.");
                throw new OperationCanceledException();
            }
            catch (Exception e)
            {
                TraceSource.TraceEvent(TraceEventType.Error, 0,
                    "Unexpected exception in GetSendSecureUrlForEnterpriseAccount ({0})", e.Message);
                throw new Exceptions.SendSecureException(e.Message);
            }
        }
    }
}
