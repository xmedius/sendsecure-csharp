using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using Moq;
using Moq.Protected;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XMedius.SendSecure.Test
{
    [TestClass]
    public class JsonClientTest
    {
        private static Mock<HttpClientHandler> mockHandler;

        private static readonly Uri SendSecureServerUrlUriSuccess = new Uri("https://portal.xmedius.com/services/testsuccess/sendsecure/server/url");
        private static readonly Uri SendSecureServerUrlUriSuccess2 = new Uri("https://portal.xmedius.com/services/testurisuccess/sendsecure/server/url");
        private static readonly Uri NewSafeboxUriSuccess = new Uri("https://sendsecure.xmedius.com/api/v2/safeboxes/new?user_email=testsuccess@xmedius.com&locale=en");
        private static readonly Uri GetSecurityProfilesUriSuccess = new Uri("https://sendsecure.xmedius.com/api/v2/enterprises/testsuccess/security_profiles?user_email=testsuccess@xmedius.com&locale=en");
        private static readonly Uri GetEnterpriseSettingsUriSuccess = new Uri("https://sendsecure.xmedius.com/api/v2/enterprises/testsuccess/settings?locale=en");

        private static readonly Uri SendSecureServerUrlUriNotFound = new Uri("https://portal.xmedius.com/services/testnotfound/sendsecure/server/url");
        private static readonly Uri NewSafeboxUriNotFound = new Uri("https://sendsecure.xmedius.com/api/v2/safeboxes/new?user_email=testnotfound@xmedius.com&locale=en");
        private static readonly string GetSecurityProfilesUriNotFound = @"https://sendsecure.xmedius.com/api/v2/enterprises/\w+/security_profiles\?user_email=testnotfound@xmedius.com&locale=en";
        private static readonly Uri GetEnterpriseSettingsUriNotFound = new Uri("https://sendsecure.xmedius.com/api/v2/enterprises/testurisuccess/settings?locale=en");

        private static readonly string GUID = "d65979185c1a4bbe85ef8ce3458de55b";
        private static readonly string UPLOAD_URL = "https://fileserver.xmedius.com/xmss/upload";

        [TestInitialize()]
        public void Initialize()
        {
            mockHandler = new Mock<HttpClientHandler>();

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == SendSecureServerUrlUriSuccess), 
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("https://sendsecure.xmedius.com") });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == SendSecureServerUrlUriSuccess2),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("https://sendsecure.xmedius.com") });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == SendSecureServerUrlUriNotFound),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == NewSafeboxUriSuccess), 
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(new JsonObjects.NewSafeboxResponseSuccess
                {
                    guid = GUID,
                    public_encryption_key = "key",
                    upload_url = UPLOAD_URL
                }))
                });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == NewSafeboxUriNotFound),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == GetSecurityProfilesUriSuccess), 
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(new JsonObjects.GetSecurityProfilesResponseSuccess
                {
                    SecurityProfiles = new List<Helpers.SecurityProfile>()
                }))
                });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => Regex.Match(message.RequestUri.ToString(), GetSecurityProfilesUriNotFound).Success),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == GetEnterpriseSettingsUriSuccess), 
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(new Helpers.EnterpriseSettings
                {
                    PdfLanguage = "en",
                    UsePdfaAuditRecords = false,
                    InternationalDialingPlan = "us",
                    ExtensionFilter = new Helpers.ExtensionFilter
                    {
                        Mode = "allow",
                        List = new List<string>()
                    },
                    IncludeUsersInAutocomplete = true,
                    IncludeFavoritesInAutocomplete = true
                }))
                });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == GetEnterpriseSettingsUriNotFound),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            Utils.HttpUtil.HttpClient = new HttpClient(mockHandler.Object);
        }

        [TestMethod]
        public async Task NewSafeboxAsync_Success()
        {
            var jsonClient = new JsonClient("29401642-b24f-4986-af3d-67af2e3f893c", "testsuccess");

            string json = await jsonClient.NewSafeboxAsync("testsuccess@xmedius.com");

            Assert.AreEqual("{\"guid\":\"" + GUID + "\",\"public_encryption_key\":\"key\",\"upload_url\":\"" + UPLOAD_URL + "\"}"
                , json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task NewSafeboxAsync_UrlError()
        {
            var jsonClient = new JsonClient("29401642-b24f-4986-af3d-67af2e3f893c", "testnotfound");

            try
            {
                string json = await jsonClient.NewSafeboxAsync("testsuccess@xmedius.com");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(404, e.Code);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task NewSafeboxAsync_NewSafeboxError()
        {
            var jsonClient = new JsonClient("29401642-b24f-4986-af3d-67af2e3f893c", "testsuccess");

            try
            {
                string json = await jsonClient.NewSafeboxAsync("testnotfound@xmedius.com");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(404, e.Code);
                throw;
            }
        }

        [TestMethod]
        public async Task GetSecurityProfilesAsync_Success()
        {
            Mock<HttpClientHandler> mockHandler = new Mock<HttpClientHandler>();

            var jsonClient = new JsonClient("29401642-b24f-4986-af3d-67af2e3f893c", "testsuccess");

            string json = await jsonClient.GetSecurityProfilesAsync("testsuccess@xmedius.com");

            Assert.AreEqual("{\"security_profiles\":[]}", json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetSecurityProfilesAsync_UrlError()
        {
            var jsonClient = new JsonClient("29401642-b24f-4986-af3d-67af2e3f893c", "testnotfound");

            try
            {
                string json = await jsonClient.GetSecurityProfilesAsync("testsuccess@xmedius.com");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(404, e.Code);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetSecurityProfilesAsync_GetSecurityProfilesError()
        {
            var jsonClient = new JsonClient("29401642-b24f-4986-af3d-67af2e3f893c", "testsuccess");

            try
            {
                string json = await jsonClient.GetSecurityProfilesAsync("testnotfound@xmedius.com");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(404, e.Code);
                throw;
            }
        }

        [TestMethod]
        public async Task GetEnterpriseSettingsAsync_Success()
        {
            var jsonClient = new JsonClient("29401642-b24f-4986-af3d-67af2e3f893c", "testsuccess");

            string json = await jsonClient.GetEnterpriseSettingsAsync();

            Assert.AreEqual("{\"extension_filter\":{\"mode\":\"allow\",\"list\":[]},\"created_at\":\"0001-01-01T00:00:00\",\"updated_at\":\"0001-01-01T00:00:00\",\"default_security_profile_id\":null,\"pdf_language\":\"en\",\"use_pdfa_audit_records\":false,\"international_dialing_plan\":\"us\",\"include_users_in_autocomplete\":true,\"include_favorites_in_autocomplete\":true}", json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetEnterpriseSettingsAsync_UrlError()
        {
            var jsonClient = new JsonClient("29401642-b24f-4986-af3d-67af2e3f893c", "testnotfound");

            try
            {
                string json = await jsonClient.GetEnterpriseSettingsAsync();
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(404, e.Code);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetEnterpriseSettingsAsync_GetEnterpriseSettingsError()
        {
            var jsonClient = new JsonClient("29401642-b24f-4986-af3d-67af2e3f893c", "testurisuccess");

            try
            {
                string json = await jsonClient.GetEnterpriseSettingsAsync();
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(404, e.Code);
                throw;
            }
        }
    }
}
