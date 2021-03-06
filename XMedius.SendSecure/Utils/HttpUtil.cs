﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace XMedius.SendSecure.Utils
{
    internal class HttpUtil
    {
        internal static HttpClient HttpClient = new HttpClient();
        private static TraceSource TraceSource = new TraceSource("XMedius.SendSecure");

        public static async Task<String> MakeRequestAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            try
            {
                TraceSource.TraceEvent(TraceEventType.Verbose, 0, "SendSecure sending request to {0}...", requestMessage.RequestUri);
                var response = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);
                TraceSource.TraceEvent(TraceEventType.Verbose, 0, "SendSecure sending request to {0}... Done.", requestMessage.RequestUri);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Content == null)
                    {
                        TraceSource.TraceEvent(TraceEventType.Error, 0, "SendSecure response for request to {0} has no content", requestMessage.RequestUri);
                        throw new Exceptions.MakeRequestException();
                    }

                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    TraceSource.TraceEvent(TraceEventType.Error, 0,
                        "SendSecureService request to {0} failed ({1} {2}).", requestMessage.RequestUri, (int)response.StatusCode, response.ReasonPhrase);

                    if (response.Content != null)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        throw new Exceptions.MakeRequestException(response.StatusCode, response.ReasonPhrase, responseString);
                    }
                    else
                    {
                        throw new Exceptions.MakeRequestException(response.StatusCode, response.ReasonPhrase);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.InnerException != null)
                {
                    TraceSource.TraceEvent(TraceEventType.Error, 0,
                        "HttpRequestException in request to {0} ({1} : {2}).", requestMessage.RequestUri, e.Message, e.InnerException.Message);
                }
                else
                {
                    TraceSource.TraceEvent(TraceEventType.Error, 0,
                        "HttpRequestException in request to {0} ({1}).", requestMessage.RequestUri, e.Message);
                }

                throw;
            }
        }

        public static HttpRequestMessage PrepareAuthenticatedRequest(Uri requestUri, string token)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = requestUri;
            request.Headers.Add("XM-Token-Authorization", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return request;
        }

        private static async Task<string> MakeAuthenticatedRequestAsync(HttpMethod method, Uri requestUri, string token, StringContent content, CancellationToken cancellationToken)
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = requestUri,
                    Method = method,
                };
                request.Headers.Add("XM-Token-Authorization", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (content != null)
                {
                    request.Content = content;
                }
                return await MakeRequestAsync(request, cancellationToken);
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
                TraceSource.TraceEvent(TraceEventType.Information, 0, "{0} request to {1} Cancelled.", method.ToString(), requestUri);
                throw new OperationCanceledException();
            }
            catch (Exception e)
            {
                TraceSource.TraceEvent(TraceEventType.Error, 0,
                    "Unexpected exception in {0}} request to {1} ({2})", method.ToString(), requestUri, e.Message);
                throw new Exceptions.SendSecureException(e.Message);
            }
        }

        public static async Task<string> MakeAuthenticatedGetRequestAsync(Uri requestUri, string token, CancellationToken cancellationToken)
        {
            return await MakeAuthenticatedRequestAsync(HttpMethod.Get, requestUri, token, null, cancellationToken);
        }

        public static async Task<string> MakeAuthenticatedPostRequestAsync(string jsonParameters, Uri requestUri, string token, CancellationToken cancellationToken)
        {
            StringContent jsonContent = new StringContent(jsonParameters);
            jsonContent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            return await MakeAuthenticatedRequestAsync(HttpMethod.Post, requestUri, token, jsonContent, cancellationToken);
        }

        public static async Task<string> MakeAuthenticatedPatchRequestAsync(string jsonParameters, Uri requestUri, string token, CancellationToken cancellationToken)
        {
            StringContent jsonContent = new StringContent(jsonParameters);
            jsonContent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            return await MakeAuthenticatedRequestAsync(new HttpMethod("PATCH"), requestUri, token, jsonContent, cancellationToken);
        }

        public static async Task<string> MakeAuthenticatedDeleteRequestAsync(Uri requestUri, string token, CancellationToken cancellationToken)
        {
            return await MakeAuthenticatedRequestAsync(HttpMethod.Delete, requestUri, token, null, cancellationToken);
        }
        
    }
}
