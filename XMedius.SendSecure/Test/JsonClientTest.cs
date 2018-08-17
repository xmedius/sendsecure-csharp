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
using XMedius.SendSecure.Test.Constants;

namespace XMedius.SendSecure.Test
{
    [TestClass]
    public class JsonClientTest
    {
        private static Mock<HttpClientHandler> mockHandler;

        private static readonly string SAFEBOX_GUID = "d65979185c1a4bbe85ef8ce3458de55b";
        private static readonly string PARTICIPANT_ID = "7a3c51e00a004917a8f5db807180fcc5";
        private static readonly string DOCUMENT_GUID = "97334293-23c2-4c94-8cee-369ddfabb678";
        private static readonly int FAVORITE_ID = 456;
        private static readonly int USER_ID = 123;
        private static readonly string MESSAGE_ID = "152664";
        private static readonly string UPLOAD_URL = "https://fileserver.xmedius.com/xmss/upload";

        private static JsonClient jsonClientSuccess = new JsonClient("29401642-b24f-4986-af3d-67af2e3f893c", USER_ID, "testsuccess");
        private static JsonClient jsonClientError = new JsonClient("29401642-b24f-4986-af3d-67af2e3f893c", USER_ID, "testerror", new Uri("https://testerror.portal.com"));

        [TestInitialize()]
        public void Initialize()
        {
            mockHandler = new Mock<HttpClientHandler>();

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SendSecureServerSuccess)), 
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("https://sendsecure.xmedius.com") });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SendSecureServerError)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("https://sendsecure.testerror.portal.com") });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SendSecureServerSuccess2)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("https://sendsecure.xmedius.com") });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SendSecureServerNotFound)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            Utils.HttpUtil.HttpClient = new HttpClient(mockHandler.Object);
        }

        [TestMethod]
        public async Task NewSafeboxAsync_Success()
        {
            var expectedJson = "{\"guid\":\\" + SAFEBOX_GUID + "\",\"public_encryption_key\":\"key\",\"upload_url\":\\" + UPLOAD_URL + "\"}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.NewSafeboxSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            string json = await jsonClientSuccess.NewSafeboxAsync("testsuccess@xmedius.com");

            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task NewSafeboxAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.NewSafeboxError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));

            try
            {
                string json = await jsonClientError.NewSafeboxAsync("testerror@xmedius.com");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetSecurityProfilesAsync_Success()
        {
            var expectedJson = "{\"security_profiles\":[]}";
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SecurityProfilesSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            string json = await jsonClientSuccess.GetSecurityProfilesAsync("testsuccess@xmedius.com");

            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetSecurityProfilesAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SecurityProfilesError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));
            try
            {
                var error = await jsonClientError.GetSecurityProfilesAsync("testerror@xmedius.com");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetEnterpriseSettingsAsync_Success()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.EnterpriseSettingsSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new Helpers.EnterpriseSettings
                    {
                        DefaultSecurityProfileId = 14,
                        PdfLanguage = "en",
                        UsePdfaAuditRecords = false,
                        InternationalDialingPlan = "us",
                        ExtensionFilter = new Helpers.ExtensionFilter
                        {
                            Mode = "forbid",
                            List = new List<string> { "bat", "bin" }
                        },
                        VirusScanEnabled = false,
                        MaxFileSizeValue = null,
                        MaxFileSizeUnit = null,
                        IncludeUsersInAutocomplete = true,
                        IncludeFavoritesInAutocomplete = true,
                        UsersPublicUrl = true
                    }))
                });

            string json = await jsonClientSuccess.GetEnterpriseSettingsAsync();

            Assert.AreEqual("{\"created_at\":null," +
                "\"updated_at\":null," +
                "\"default_security_profile_id\":14," +
                "\"pdf_language\":\"en\"," +
                "\"use_pdfa_audit_records\":false," +
                "\"international_dialing_plan\":\"us\"," +
                "\"extension_filter\":{\"mode\":\"forbid\",\"list\":[\"bat\",\"bin\"]}," +
                "\"virus_scan_enabled\":false," +
                "\"max_file_size_value\":null," +
                "\"max_file_size_unit\":null," +
                "\"include_users_in_autocomplete\":true," +
                "\"include_favorites_in_autocomplete\":true," +
                "\"users_public_url\":true}", json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetEnterpriseSettingsAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.EnterpriseSettingsError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));
            try
            {
                var error = await jsonClientError.GetEnterpriseSettingsAsync();
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task UploadFileAsync_Success()
        {
            var expectedJson = "{ \"temporary_document\": { \"document_guid\": \"65f53ec1282c454fa98439dbda134093\" } }";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.UploadFileSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });
            
            var testStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("test"));
            string json = await jsonClientSuccess.UploadFileAsync(Urls.GetUri(Urls.UploadFileSuccess), testStream, "application/octet-stream", "Test", 17850);

            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        public async Task CommitSafeboxAsync_Success()
        {
            var expectedJson = "{ \"guid\": \"845459484b674055bec4ddf2ba5ab60e\"," +
                                 "\"user_id\": 4," +
                                 "\"enterprise_id\": 4," +
                                 "\"subject\": \"Donec rutrum congue leo eget malesuada.\"," +
                                 "\"expiration\": \"2017-05-31T14:42:27.258Z\"," +
                                 "\"notification_language\": \"fr\"," +
                                 "\"status\": \"in_progress\"," +
                                 "\"security_profile_name\": \"All Contact Method Allowed!\"," +
                                 "\"force_expiry_date\": null," +
                                 "\"security_code_length\": 6," +
                                 "\"allowed_login_attempts\": 10," +
                                 "\"allow_remember_me\": true," +
                                 "\"allow_sms\": true," +
                                 "\"allow_voice\": true," +
                                 "\"allow_email\": true," +
                                 "\"reply_enabled\": true," +
                                 "\"group_replies\": true," +
                                 "\"code_time_limit\": 5," +
                                 "\"encrypt_message\": true," +
                                 "\"two_factor_required\": true," +
                                 "\"auto_extend_value\": 6," +
                                 "\"auto_extend_unit\": \"hours\"," +
                                 "\"retention_period_type\": \"discard_at_expiration\"," +
                                 "\"retention_period_value\": null," +
                                 "\"retention_period_unit\": \"hours\"," +
                                 "\"delete_content_on\": null," +
                                 "\"allow_manual_delete\": true," +
                                 "\"allow_manual_close\": true," +
                                 "\"email_notification_enabled\": true," +
                                 "\"preview_url\": \"https://sendsecure.integration.xmedius.com/s/845459484b674055bec4ddf2ba5ab60e/preview\",}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.CommitSafeboxSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var safeboxJson = "{ \"safebox\": {" +
                                   "\"guid\": \"dc6f21e0f02c4112874f8b5653b795e4\"," +
                                   "\"recipients\": [{" +
                                      "\"first_name\": \"\"," +
                                      "\"last_name\": \"\"," +
                                      "\"company_name\": \"\"," +
                                      "\"email\": \"john.smith@example.com\"," +
                                      "\"contact_methods\": [" +
                                          "{" +
                                             "\"destination_type\": \"cell_phone\"," +
                                             "\"destination\": \"+15145550000\"" +
                                          "}," +
                                          "{" +
                                             "\"destination_type\": \"office_phone\"," +
                                             "\"destination\": \"+15145551111\"" +
                                          "}]}]," +
                                   "\"subject\": \"Donec rutrum congue leo eget malesuada. \"," +
                                   "\"message\": \"Donec rutrum congue leo eget malesuada. Proin eget tortor risus...\"," +
                                   "\"document_ids\": [" +
                                     "\"97334293-23c2-4c94-8cee-369ddfabb678\"," +
                                     "\"74268a39-ae88-49bc-9cc9-423d4126bd33\"]," +
                                   "\"security_profile_id\": 7," +
                                   "\"reply_enabled\": true," +
                                   "\"group_replies\": true," +
                                   "\"expiration_value\": 1," +
                                   "\"expiration_unit\": \"months\"," +
                                   "\"retention_period_type\": \"discard_at_expiration\"," +
                                   "\"retention_period_value\": null," +
                                   "\"retention_period_unit\": null," +
                                   "\"encrypt_message\":  true," +
                                   "\"double_encryption\": false," +
                                   "\"public_encryption_key\": \"RBQISzMk9KwkdBKDVw8sQK0gQe4MOTBGaM7hLdVOmJJ56nCZ8N7h2J0Zhy9rb9ax\"," +
                                   "\"notification_language\": \"en\"," +
                                   "\"user_email\": \"john@example.com\" }}";

            var json = await jsonClientSuccess.CommitSafeboxAsync(safeboxJson);

            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task CommitSafeboxAsync_Error()
        {
            var expectedError = "{\"error\":\"Some entered values are incorrect.\",\"attributes\":{\"language\":[\"cannot be blank\"]}}";
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.CommitSafeboxError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, expectedError));

            var safeboxJson = "{ \"safebox\": {" +
                                   "\"guid\": \"dc6f21e0f02c4112874f8b5653b795e4\"," +
                                   "\"recipients\": [{" +
                                      "\"first_name\": \"\"," +
                                      "\"last_name\": \"\"," +
                                      "\"company_name\": \"\"," +
                                      "\"email\": \"john.smith@example.com\"," +
                                      "\"contact_methods\": [" +
                                          "{" +
                                             "\"destination_type\": \"cell_phone\"," +
                                             "\"destination\": \"+15145550000\"" +
                                          "}," +
                                          "{" +
                                             "\"destination_type\": \"office_phone\"," +
                                             "\"destination\": \"+15145551111\"" +
                                          "}]}]," +
                                   "\"subject\": \"Donec rutrum congue leo eget malesuada. \"," +
                                   "\"message\": \"Donec rutrum congue leo eget malesuada. Proin eget tortor risus...\"," +
                                   "\"document_ids\": [" +
                                     "\"97334293-23c2-4c94-8cee-369ddfabb678\"," +
                                     "\"74268a39-ae88-49bc-9cc9-423d4126bd33\"]," +
                                   "\"security_profile_id\": 7," +
                                   "\"reply_enabled\": true," +
                                   "\"group_replies\": true," +
                                   "\"expiration_value\": 1," +
                                   "\"expiration_unit\": \"months\"," +
                                   "\"retention_period_type\": \"discard_at_expiration\"," +
                                   "\"retention_period_value\": null," +
                                   "\"retention_period_unit\": null," +
                                   "\"encrypt_message\":  true," +
                                   "\"double_encryption\": false," +
                                   "\"public_encryption_key\": \"RBQISzMk9KwkdBKDVw8sQK0gQe4MOTBGaM7hLdVOmJJ56nCZ8N7h2J0Zhy9rb9ax\"," +
                                   "\"notification_language\": \"en\"," +
                                   "\"user_email\": \"john@example.com\" }}";
            try
            {
                var error = await jsonClientError.CommitSafeboxAsync(safeboxJson);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedError, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetFavoritesAsync_Success()
        {
            var expectedJson = "{ \"favorites\": [{" +
                                     "\"email\": \"john.smith@example.com\"," +
                                     "\"id\": 4," +
                                     "\"first_name\": \"John\"," +
                                     "\"last_name\": \"\"," +
                                     "\"company_name\": \"Acme\"," +
                                     "\"order_number\": 1," +
                                     "\"created_at\": \"2017-04-12T15:41:39.767Z\"," +
                                     "\"updated_at\": \"2017-04-12T15:41:47.144Z\"," +
                                        "\"contact_methods\": [{" +
                                            "\"id\": 49," +
                                            "\"destination\": \"5551234\"," +
                                            "\"destination_type\": \"cell_phone\"," +
                                            "\"created_at\": \"2017-04-28T17:14:55.304Z\"," +
                                            "\"updated_at\": \"2017-04-28T17:14:55.304Z\" }]" +
                                         "}]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.FavoritesSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetFavoritesAsync();
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetFavoritesAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.FavoritesError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));
            try
            {
                string json = await jsonClientError.GetFavoritesAsync();
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task CreateFavoriteAsync_Success()
        {
            var expectedJson = "{ \"email\": \"john.smith@example.com\"," +
                                    "\"first_name\": \"John\"," +
                                    "\"last_name\": \"\"," +
                                    "\"id\": 9," +
                                    "\"company_name\": \"Acme\"," +
                                    "\"order_number\": 4," +
                                    "\"created_at\": \"2017-04-28T17:18:30.850Z\"," +
                                    "\"updated_at\": \"2017-04-28T17:18:30.850Z\"," +
                                        "\"contact_methods\": [{" +
                                           "\"id\": 50," +
                                           "\"destination\": \"5551234\"," +
                                           "\"destination_type\": \"cell_phone\"," +
                                           "\"created_at\": \"2017-04-28T17:18:30.850Z\"," +
                                           "\"updated_at\": \"2017-04-28T17:18:30.850Z\" }]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.FavoritesSuccess) && message.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var favoriteJson = "{ \"favorite\": {" +
                                    "\"email\": \"john.smith@example.com\"," +
                                    "\"first_name\": \"John\"," +
                                     "\"last_name\": \"\"," +
                                      "\"company_name\": \"Acme\"," +
                                       "\"contact_methods\": [{" +
                                          "\"destination\": \"5551234\"," +
                                           "\"destination_type\": \"cell_phone\" }]}}";

            var json = await jsonClientSuccess.CreateFavoriteAsync(favoriteJson);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task CreateFavoriteAsync_Error()
        {
            var expectedError = "{\"error\":\"Some entered values are incorrect.\",\"attributes\": {\"email\":[\"cannot be blank\"]}";
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.FavoritesError) && message.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, expectedError));

            var favoriteJson = "{ \"favorite\":{" +
                                     "\"email\": \"\"," +
                                     "\"first_name\": \"John\"," +
                                     "\"last_name\": \"\"," +
                                     "\"company_name\": \"Acme\"," +
                                     "\"contact_methods\": [{" +
                                         "\"destination\": \"5551234\"," +
                                         "\"destination_type\": \"cell_phone\" }]}}";
            try
            {
                var json = await jsonClientError.CreateFavoriteAsync(favoriteJson);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedError, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task EditFavoriteAsync_Success()
        {
            var expectedJson = "{ \"email\": \"john.smith@example.com\"," +
                                 "\"first_name\": \"John\"," +
                                 "\"last_name\": \"\"," +
                                 "\"id\": 456," +
                                 "\"company_name\": \"Acme\"," +
                                 "\"order_number\": 4," +
                                 "\"created_at\": \"2017-04-28T17:18:30.850Z\"," +
                                 "\"updated_at\": \"2018-04-28T17:18:30.850Z\"," +
                                 "\"contact_methods\": [{" +
                                     "\"id\": 50," +
                                     "\"destination\": \"5551234\"," +
                                     "\"destination_type\": \"cell_phone\"," +
                                     "\"created_at\": \"2017-04-28T17:18:30.850Z\"," +
                                     "\"updated_at\": \"2018-04-28T17:18:30.850Z\" }]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.EditDeleteFavoriteSuccess) && message.Method == new HttpMethod("PATCH")),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var favoriteJson = "{ \"favorite\": {" +
                                    "\"email\": \"john.smith@example.com\"," +
                                    "\"first_name\": \"John\"," +
                                    "\"last_name\": \"\"," +
                                    "\"id\": 456," +
                                    "\"company_name\": \"Acme\"," +
                                    "\"order_number\": 4," +
                                    "\"contact_methods\": [{" +
                                       "\"id\": 50," +
                                       "\"destination\": \"5551234\"," +
                                       "\"destination_type\": \"cell_phone\" }]}}";
            var json = await jsonClientSuccess.EditFavoriteAsync(favoriteJson, FAVORITE_ID);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task EditFavoriteAsync_Error()
        {
            var expectedError = "{\"error\":\"Some entered values are incorrect.\",\"attributes\": {\"email\":[\"cannot be blank\"]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.EditDeleteFavoriteError) && message.Method == new HttpMethod("PATCH")),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, expectedError));

            var favoriteJson = "{ \"favorite\": {" +
                                    "\"email\": \"john.smith@example.com\"," +
                                    "\"first_name\": \"John\"," +
                                    "\"last_name\": \"\"," +
                                    "\"id\": 456," +
                                    "\"company_name\": \"Acme\"," +
                                    "\"order_number\": 4," +
                                    "\"contact_methods\": [{" +
                                       "\"id\": 50," +
                                       "\"destination\": \"5551234\"," +
                                       "\"destination_type\": \"cell_phone\" }]}}";
            try
            {
                var json = await jsonClientError.EditFavoriteAsync(favoriteJson, FAVORITE_ID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedError, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task DeleteFavoriteAsync_Success()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.EditDeleteFavoriteSuccess) && message.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent)
                {
                    Content = new StringContent(String.Empty)
                });
            var response = await jsonClientSuccess.DeleteFavoriteAsync(FAVORITE_ID);
            Assert.AreEqual(String.Empty, response);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task DeleteFavoriteAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.EditDeleteFavoriteError) && message.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));
            try
            {
                var error = await jsonClientError.DeleteFavoriteAsync(FAVORITE_ID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task CreateParticipantAsync_Success()
        {
            var expectedJson = "{ \"id\": \"23a3c8ec897548dc82f50a9a1550e52c\"," +
                                 "\"first_name\": \"John\"," +
                                 "\"last_name\": \"Smith\"," +
                                 "\"email\": \"johny.smith@example.com\"," +
                                 "\"type\": \"guest\"," +
                                 "\"role\": \"guest\"," +
                                 "\"guest_options\": {" +
                                    "\"company_name\": \"ACME\"," +
                                    "\"locked\": false," +
                                    "\"bounced_email\": false," +
                                    "\"failed_login_attempts\": 0," +
                                    "\"verified\": false," +
                                    "\"created_at\": \"2017-05-26T19:27:27.798Z\"," +
                                    "\"updated_at\": \"2017-05-26T19:27:27.798Z\"," +
                                    "\"contact_methods\": [{" +
                                        "\"id\": 35105," +
                                        "\"destination\": \"+5551234\"," +
                                        "\"destination_type\": \"cell_phone\"," +
                                        "\"verified\": false," +
                                        "\"created_at\": \"2017-05-26T19:27:27.864Z\"," +
                                        "\"updated_at\": \"2017-05-26T19:27:27.864Z\" }]}}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.ParticipantsSuccess) && message.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var participant_json = "{ \"participant\": {" +
                                         "\"first_name\": \"John\"," +
                                         "\"last_name\": \"Smith\"," +
                                         "\"company_name\": \"ACME\"," +
                                         "\"email\": \"johny.smith@example.com\"," +
                                         "\"contact_methods\": [{" +
                                              "\"destination\": \"+5551234\"," +
                                              " \"destination_type\": \"cell_phone\" }]}}";
            var json = await jsonClientSuccess.CreateParticipantAsync(participant_json, SAFEBOX_GUID);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task CreateParticipantAsync_Error()
        {
            var expectedError = "{\"error\":\"Some entered values are incorrect.\",\"attributes\": {\"email\":[\"cannot be blank\"]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.ParticipantsError) && message.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, expectedError));

            var participant_json = "{ \"participant\": {" +
                                        "\"first_name\": \"John\"," +
                                        "\"last_name\": \"Smith\"," +
                                        "\"company_name\": \"ACME\"," +
                                        "\"email\": \"\"," +
                                        "\"contact_methods\": [{" +
                                            "\"destination\": \"+5551234\"," +
                                            "\"destination_type\": \"cell_phone\"" +
                                            "}]}}";
            try
            {
                var error = await jsonClientError.CreateParticipantAsync(participant_json, SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedError, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task UpdateParticipantAsync_Success()
        {
            var expectedJson = "{\"id\": \"7a3c51e00a004917a8f5db807180fcc5\"," +
                                "\"first_name\": \"John\"," +
                                "\"last_name\": \"Smith\"," +
                                "\"email\": \"johny.smith@example.com\"," +
                                "\"type\": \"guest\"," +
                                "\"role\": \"guest\"," +
                                "\"guest_options\": {" +
                                    "\"company_name\": \"XMedius\"," +
                                    "\"locked\": false," +
                                    "\"bounced_email\": false," +
                                    "\"failed_login_attempts\": 0," +
                                    "\"verified\": false," +
                                    "\"created_at\": \"2017-05-26T19:27:27.798Z\"," +
                                    "\"updated_at\": \"2017-05-28T18:27:27.798Z\"," +
                                    "\"contact_methods\": [{" +
                                        "\"id\": 35105," +
                                        "\"destination\": \"+5551234\"," +
                                        "\"destination_type\": \"cell_phone\"," +
                                        "\"verified\": false," +
                                        "\"created_at\": \"2017-05-26T19:27:27.864Z\"," +
                                        "\"updated_at\": \"2017-05-28T18:27:27.864Z\"}]}}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
               ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.UpdateParticipantSuccess)),
               ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
               {
                   Content = new StringContent(expectedJson)
               });

            var participantJson = "{ \"participant\": { \"id\": \"7a3c51e00a004917a8f5db807180fcc5\"," +
                                                        "\"first_name\": \"John\"," +
                                                        "\"last_name\": \"Smith\"," +
                                                        "\"email\": \"johny.smith@example.com\"," +
                                                        "\"type\": \"guest\"," +
                                                        "\"role\": \"guest\"," +
                                                        "\"guest_options\": {" +
                                                             "\"company_name\": \"XMedius\"," +
                                                             "\"locked\": false," +
                                                             "\"contact_methods\": [{" +
                                                                 "\"id\": 35105," +
                                                                 "\"destination\": \"+5551234\"," +
                                                                 "\"destination_type\": \"cell_phone\" }]}}";
            var json = await jsonClientSuccess.UpdateParticipantAsync(participantJson, SAFEBOX_GUID, PARTICIPANT_ID);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task UpdateParticipantAsync_Error()
        {
            var expectedError = "{\"error\":\"Some entered values are incorrect.\",\"attributes\": {\"email\":[\"cannot be blank\"]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.UpdateParticipantError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, expectedError));

            var participantJson = "{ \"participant\": { \"id\": \"7a3c51e00a004917a8f5db807180fcc5\"," +
                                                        "\"first_name\": \"John\"," +
                                                        "\"last_name\": \"Smith\"," +
                                                        "\"email\": \"johny.smith@example.com\"," +
                                                        "\"type\": \"guest\"," +
                                                        "\"role\": \"guest\"," +
                                                        "\"guest_options\": {" +
                                                             "\"company_name\": \"XMedius\"," +
                                                             "\"locked\": false," +
                                                             "\"contact_methods\": [{" +
                                                                 "\"id\": 35105," +
                                                                 "\"destination\": \"+5551234\"," +
                                                                 "\"destination_type\": \"cell_phone\" }]}}";
            try
            {
                var error = await jsonClientError.UpdateParticipantAsync(participantJson, SAFEBOX_GUID, PARTICIPANT_ID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedError, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task CloseSafeboxAsync_Success()
        {
            var expectedJson = "{ \"result\": true, \"message\": \"SafeBox successfully closed.\" }";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.CloseSafeboxSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var response = await jsonClientSuccess.CloseSafeboxAsync(SAFEBOX_GUID);
            Assert.AreEqual(expectedJson, response);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task CloseSafeboxAsync_Error()
        {
            var expectedJson = "{\"result\": false, \"message\": \"Unable to close the SafeBox.\" }";
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.CloseSafeboxError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, expectedJson));

            try
            {
                var error = await jsonClientError.CloseSafeboxAsync(SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedJson, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task DeleteSafeboxContentAsync_Success()
        {
            var expectedJson = "{ \"result\": true, \"message\": \"SafeBox content successfully deleted.\" }";
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.DeleteSafeboxContentSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var response = await jsonClientSuccess.DeleteSafeboxContentAsync(SAFEBOX_GUID);
            Assert.AreEqual(expectedJson, response);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task DeleteSafeboxContentAsync_Error()
        {
            var expectedError = "{\"result\": false, \"message\": \"Unable to delete the SafeBox content.\" }";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.DeleteSafeboxContentError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, expectedError));

            try
            {
                var error = await jsonClientError.DeleteSafeboxContentAsync(SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedError, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task MarkAsReadAsync_Success()
        {
            var expectedJson = "{ \"result\": true }";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.MarkAsReadSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var response = await jsonClientSuccess.MarkAsReadAsync(SAFEBOX_GUID);
            Assert.AreEqual(expectedJson, response);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task MarkAsReadAsync_Error()
        {
            var expectedError = "{\"result\": false, \"message\": \"Unable to mark the SafeBox as Read.\" }";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.MarkAsReadError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, expectedError));

            try
            {
                var error = await jsonClientError.MarkAsReadAsync(SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedError, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task MarkAsUnreadAsync_Success()
        {
            var expectedJson = "{ \"result\": true }";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
               ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.MarkAsUnreadSuccess)),
               ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
               {
                   Content = new StringContent(expectedJson)
               });

            var response = await jsonClientSuccess.MarkAsUnreadAsync(SAFEBOX_GUID);
            Assert.AreEqual(expectedJson, response);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task MarkAsUnreadAsync_Error()
        {
            var expectedError = "{\"result\": false, \"message\": \"Unable to mark the SafeBox as Unread.\" }";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.MarkAsUnreadError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest,expectedError));

            try
            {
                var error = await jsonClientError.MarkAsUnreadAsync(SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedError, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task MarkAsReadMessageAsync_Success()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.MarkAsReadMessageSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"result\":true}")
                });

            var json = await jsonClientSuccess.MarkAsReadMessageAsync(SAFEBOX_GUID, MESSAGE_ID);

            Assert.AreEqual("{\"result\":true}", json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task MarkAsReadMessageAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.MarkAsReadMessageError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, "{\"result\":false}"));

            try
            {
                var error = await jsonClientError.MarkAsReadMessageAsync(SAFEBOX_GUID, MESSAGE_ID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual("{\"result\":false}", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task MarkAsUnreadMessageAsync_Success()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.MarkAsUnreadMessageSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"result\":true}")
                });

            var json = await jsonClientSuccess.MarkAsUnreadMessageAsync(SAFEBOX_GUID, MESSAGE_ID);

            Assert.AreEqual("{\"result\":true}", json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task MarkAsUnreadMessageAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.MarkAsUnreadMessageError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, "{\"result\":false}"));

            try
            {
                var error = await jsonClientError.MarkAsUnreadMessageAsync(SAFEBOX_GUID, MESSAGE_ID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual("{\"result\":false}", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetFileUrlAsync_Success()
        {
            var expectedJson = "{ \"url\": \"https://fileserver.integration.xmedius.com/xmss/DteeDmb-2\" }";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.FileUrlSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetFileUrlAsync(SAFEBOX_GUID, DOCUMENT_GUID, "testsuccess@xmedius.com");
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetFileUrlAsync_Error()
        {
            var expectedError = "{\"error\": \"User email not found\"}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.FileUrlError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, expectedError));

            try
            {
                var error = await jsonClientError.GetFileUrlAsync(SAFEBOX_GUID, DOCUMENT_GUID, "testerror@xmedius.com");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedError, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task AddTimeAsync_Success()
        {
            var expectedJson = "{ \"result\": true," +
                                 "\"message\": \"SafeBox duration successfully extended.\"," +
                                 "\"new_expiration\": \"2017-05-14T18:09:05.662Z\" }";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.AddTimeSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var time_json = "{ \"safebox\": { \"add_time_value\": 2, \"add_time_unit\": \"weeks\" } }";
            var json = await jsonClientSuccess.AddTimeAsync(time_json, SAFEBOX_GUID);

            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task AddTimeAsync_Error()
        {
            var expectedError = "{\"result\": false, \"message\": \"Unable to extend the SafeBox duration.\" }";
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.AddTimeError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, expectedError));

            var time_json = "{ \"safebox\": { \"add_time_value\": 2, \"add_time_unit\": \"weeks\" } }";

            try
            {
                var error = await jsonClientError.AddTimeAsync(time_json, SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedError, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetAuditRecordUrlAsync_Succcess()
        {
            var expectedJson = "{ \"url\": \"http://sendsecure.integration.xmedius.com/s/73af62f766ee459e81f46e4f533085a4.pdf\" }";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.AuditRecordUrlSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetAuditRecordUrlAsync(SAFEBOX_GUID);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetAuditRecordUrlAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.AuditRecordUrlError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));

            try
            {
                var error = await jsonClientError.GetAuditRecordUrlAsync(SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetSafeboxesAsync_Success()
        {
            var expectedJson = "{ \"count\": 1," +
                                 "\"previous_page_url\": null," +
                                 "\"next_page_url\": \"api/v2/safeboxes?status=unread&search=test&page=2\"," +
                                 "\"safeboxes\": [{ " +
                                    "\"guid\": \"73af62f766ee459e81f46e4f533085a4\"," +
                                    "\"user_id\": 1," +
                                    "\"enterprise_id\": 1" +
                                   "}]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxesSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetSafeboxesAsync();
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        public async Task GetSafeboxesWithFilterAsync_Success()
        {
            var expectedJson = "{ \"count\": 1," +
                                 "\"previous_page_url\": null," +
                                 "\"next_page_url\": \"api/v2/safeboxes?status=unread&search=test&page=2\"," +
                                 "\"safeboxes\": [{" +
                                    "\"guid\": \"73af62f766ee459e81f46e4f533085a4\"," +
                                    "\"user_id\": 1," +
                                    "\"enterprise_id\": 1 }]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxesFilterSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetSafeboxesAsync("in_progress", "safebox", 1, 1);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        public async Task GetSafeboxesWithUrlAsync_Success()
        {
            var expectedJson = "{ \"count\": 1," +
                                 "\"previous_page_url\": null," +
                                 "\"next_page_url\": \"api/v2/safeboxes?status=unread&search=test&page=2\"," +
                                 "\"safeboxes\": [{" +
                                    "\"guid\": \"73af62f766ee459e81f46e4f533085a4\"," +
                                    "\"user_id\": 1," +
                                    "\"enterprise_id\": 1 }]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxesFilterSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetSafeboxesAsync("https://sendsecure.xmedius.com/api/v2/safeboxes.json?status=in_progress&search=safebox&per_page=1&page=1&locale=en");
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetSafeboxesAsync_Error()
        {
            var expectedError = "{\"error\": \"Invalid per_page parameter value (1001)\"}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxesError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest, expectedError));

            try
            {
                var error = await jsonClientError.GetSafeboxesAsync("in_progress", "safebox", 1001, 1);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                Assert.AreEqual(expectedError, e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetSafeboxInfoAsync_Success()
        {
            var expectedJson = "{ \"safebox\": { \"guid\": \"73af62f766ee459e81f46e4f533085a4\"," +
                                    "\"security_options\": {" +
                                        "\"security_code_length\": 4," +
                                        "\"allowed_login_attempts\": 3," +
                                        "\"allow_remember_me\": true," +
                                        "\"allow_sms\": true," +
                                        "\"allow_voice\": true," +
                                        "\"allow_email\": false," +
                                        "\"reply_enabled\": true," +
                                        "\"group_replies\": false," +
                                        "\"code_time_limit\": 5," +
                                        "\"encrypt_message\": true," +
                                        "\"two_factor_required\": true," +
                                        "\"auto_extend_value\": 3," +
                                        "\"auto_extend_unit\": \"days\"," +
                                        "\"retention_period_type\": \"do_not_discard\"," +
                                        "\"retention_period_value\": null," +
                                        "\"retention_period_unit\": \"hours\"," +
                                        "\"delete_content_on\": null," +
                                        "\"allow_manual_delete\": true," +
                                        "\"allow_manual_close\": false" +
                                    "}," +
                                    "\"participants\": [{" +
                                        "\"id\": \"7a3c51e00a004917a8f5db807180fcc5\"," +
                                        "\"first_name\": \"\"," +
                                        "\"last_name\": \"\"," +
                                        "\"email\": \"john.smith@example.com\"," +
                                        "\"type\": \"guest\"," +
                                        "\"role\": \"guest\"," +
                                        "\"guest_options\": {" +
                                            "\"company_name\": \"\"," +
                                            "\"locked\": false," +
                                            "\"bounced_email\": false," +
                                            "\"failed_login_attempts\": 0," +
                                            "\"verified\": false," +
                                            "\"contact_methods\": [{" +
                                                "\"id\": 35016," +
                                                "\"destination\": \"+15145550000\"," +
                                                "\"destination_type\": \"cell_phone\"," +
                                                "\"verified\": false," +
                                                "\"created_at\": \"2017-05-24T14:45:35.453Z\"," +
                                                "\"updated_at\": \"2017-05-24T14:45:35.453Z\" }]" +
                                            "}}," +
                                        "{" +
                                            "\"id\": 34208," +
                                            "\"first_name\": \"Jane\"," +
                                            "\"last_name\": \"Doe\"," +
                                            "\"email\": \"jane.doe@example.com\"," +
                                            "\"type\": \"user\"," +
                                            "\"role\": \"owner\" }]," +
                                    "\"messages\": [{ \"note\": \"Lorem Ipsum...\"," +
                                        "\"note_size\": 148," +
                                        "\"read\": true," +
                                        "\"author_id\": \"3\"," +
                                        "\"author_type\": \"guest\"," +
                                        "\"created_at\": \"2017-04-05T14:49:35.198Z\"," +
                                        "\"documents\": [" +
                                        "{" +
                                            "\"id\": \"5a3df276aaa24e43af5aca9b2204a535\"," +
                                            "\"name\": \"Axient-soapui-project.xml\"," +
                                            "\"sha\": \"724ae04430315c60ca17f4dbee775a37f5b18c05aee99c9c\"," +
                                            "\"size\": 129961," +
                                            "\"url\": \"https://sendsecure.xmedius.com/api/v2/safeboxes/b4d898ada15f42f293e31905c514607f/documents/5a3df276aaa24e43af5aca9b2204a535/url\"" +
                                        "}] }]," +
                                    "\"download_activity\": {}," +
                                    "\"event_history\": [{" +
                                        "\"type\": \"safebox_created_owner\"," +
                                        "\"date\": \"2017-03-30T18:09:05.966Z\"," +
                                        "\"metadata\": {" +
                                        "\"emails\": [ \"john44@example.com\" ]," +
                                        "\"attachment_count\": 0" +
                                        "}," +
                                        "\"message\": \"SafeBox created by john.smith@example.com with 0 attachment(s) from 0.0.0.0 for john.smith@example.com\"" +
                                    "}]}}";
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxInfoSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetSafeboxInfoAsync(SAFEBOX_GUID, String.Empty);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        public async Task GetSafeboxInfoWithSectionsAsync_Success()
        {
            var expectedJson = "{ \"safebox\": { \"guid\": \"73af62f766ee459e81f46e4f533085a4\"," +
                                    "\"security_options\": {}," +
                                    "\"participants\": [{ \"id\": \"7a3c51e00a004917a8f5db807180fcc5\" }," +
                                                       "{ \"id\": \"we564eg64egw5rf5db807180fcc5\" }]," +
                                    "\"messages\": [{ \"id\": \"5a3df276aaa24e43af5aca9b2204a535\"}]}}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxInfoSectionsSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetSafeboxInfoAsync(SAFEBOX_GUID, "participants,messages");
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetSafeboxInfoAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxInfoError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));

            try
            {
                var error = await jsonClientError.GetSafeboxInfoAsync(SAFEBOX_GUID, String.Empty);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetSafeboxParticipantsAsync_Success()
        {
            var expectedJson = "{ \"participants\": [{" +
                                                 "\"id\": \"7a3c51e00a004917a8f5db807180fcc5\"," +
                                                 "\"first_name\": \"\"," +
                                                 "\"last_name\": \"\"," +
                                                 "\"email\": \"john.smith@example.com\"," +
                                                 "\"type\": \"guest\"," +
                                                 "\"role\": \"guest\"," +
                                                 "\"guest_options\": {" +
                                                   "\"company_name\": \"\"," +
                                                   "\"locked\": false," +
                                                   "\"bounced_email\": false," +
                                                   "\"failed_login_attempts\": 0," +
                                                   "\"verified\": false," +
                                                   "\"contact_methods\": [{" +
                                                      "\"id\": 35016," +
                                                      "\"destination\": \"+15145550000\"," +
                                                      "\"destination_type\": \"cell_phone\"," +
                                                      "\"verified\": false," +
                                                      "\"created_at\": \"2017-05-24T14:45:35.453Z\"," +
                                                      "\"updated_at\": \"2017-05-24T14:45:35.453Z\" }]}}," +
                                               "{\"id\": 34208," +
                                                 "\"first_name\": \"Jane\"," +
                                                 "\"last_name\": \"Doe\"," +
                                                 "\"email\": \"jane.doe@example.com\"," +
                                                 "\"type\": \"user\"," +
                                                 "\"role\": \"owner\" }] }";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.ParticipantsSuccess) && message.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetSafeboxParticipantsAsync(SAFEBOX_GUID);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetSafeboxParticipantsAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.ParticipantsError) && message.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));

            try
            {
                var error = await jsonClientError.GetSafeboxParticipantsAsync(SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetSafeboxMessagesAsync_Success()
        {
            var expectedJson = "{ \"messages\": [{ \"note\": \"Lorem Ipsum...\"," +
                                             "\"note_size\": 148," +
                                             "\"read\": true," +
                                             "\"author_id\": \"3\"," +
                                             "\"author_type\": \"guest\"," +
                                             "\"created_at\": \"2017-04-05T14:49:35.198Z\"," +
                                             "\"documents\": [ { \"id\": \"5a3df276aaa24e43af5aca9b2204a535\"," +
                                                  "\"name\": \"Axient-soapui-project.xml\"," +
                                                  "\"sha\": \"724ae04430315c60ca17f4dbee775a37f5b18c05aee99c9c\"," +
                                                  "\"size\": 129961," +
                                                  "\"url\": \"https://sendsecure.xmedius.com/api/v2/safeboxes/b4d898ada15f42f293e31905c514607f/documents/5a3df276aaa24e43af5aca9b2204a535/url\"}] }]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxMessagesSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetSafeboxMessagesAsync(SAFEBOX_GUID);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetSafeboxMessagesAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
               ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxMessagesError)),
               ItExpr.IsAny<CancellationToken>())
               .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));

            try
            {
                var error = await jsonClientError.GetSafeboxMessagesAsync(SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetSafeboxSecurityOptionsAsync_Success()
        {
            var expectedJson = "{ \"security_options\": {" +
                                                    "\"security_code_length\": 4," +
                                                    "\"allowed_login_attempts\": 3," +
                                                    "\"allow_remember_me\": true," +
                                                    "\"allow_sms\": true," +
                                                    "\"allow_voice\": true," +
                                                    "\"allow_email\": false," +
                                                    "\"reply_enabled\": true," +
                                                    "\"group_replies\": false," +
                                                    "\"code_time_limit\": 5," +
                                                    "\"encrypt_message\": true," +
                                                    "\"two_factor_required\": true," +
                                                    "\"auto_extend_value\": 3," +
                                                    "\"auto_extend_unit\": \"days\"," +
                                                    "\"retention_period_type\": \"do_not_discard\"," +
                                                    "\"retention_period_value\": null," +
                                                    "\"retention_period_unit\": \"hours\"," +
                                                    "\"delete_content_on\": null," +
                                                    "\"allow_manual_delete\": true," +
                                                    "\"allow_manual_close\": false }}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxSecurityOptionsSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetSafeboxSecurityOptionsAsync(SAFEBOX_GUID);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetSafeboxSecurityOptionsAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxSecurityOptionsError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));

            try
            {
                var error = await jsonClientError.GetSafeboxSecurityOptionsAsync(SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetSafeboxDownloadActivityAsync_Success()
        {
            var expectedJson = "{ \"download_activity\": {" +
                                    "\"guests\": [{" +
                                       "\"id\": \"42220c777c30486e80cd3bbfa7f8e82f\"," +
                                       "\"documents\": [{" +
                                           "\"id\": \"5a3df276aaa24e43af5aca9b2204a535\"," +
                                           "\"downloaded_bytes\": 0," +
                                           "\"download_date\": null }] }]," +
                                    "\"owner\": {" +
                                      "\"id\": 72," +
                                      "\"documents\": [] }}}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxDownloadActivitySuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetSafeboxDownloadActivityAsync(SAFEBOX_GUID);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetSafeboxDownloadActivityAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxDownloadActivityError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));

            try
            {
                var error = await jsonClientError.GetSafeboxDownloadActivityAsync(SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetSafeboxEventHistoryAsync_Success()
        {
            var expectedJson = "{ \"event_history\": [{" +
                                 "\"type\": \"safebox_created_owner\"," +
                                 "\"date\": \"2017-03-30T18:09:05.966Z\"," +
                                 "\"metadata\": {" +
                                   "\"emails\": [ \"john44@example.com\" ]," +
                                   "\"attachment_count\": 0" +
                                  "}," +
                                 "\"message\": \"SafeBox created by john.smith@example.com with 0 attachment(s) from 0.0.0.0 for john.smith@example.com\"}]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxEventHistorySuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetSafeboxEventHistoryAsync(SAFEBOX_GUID);
            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetSafeboxEventHistoryAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SafeboxEventHistoryError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden, "Access denied"));

            try
            {
                var error = await jsonClientError.GetSafeboxEventHistoryAsync(SAFEBOX_GUID);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                Assert.AreEqual("Access denied", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task SearchRecipientAsync_Success()
        {
            var expectedJson = "{ \"results\": [{" +
                                 "\"id\": 1," +
                                 "\"type\": \"favorite\"," +
                                 "\"first_name\": \"John\"," +
                                 "\"last_name\": \"Doe\"," +
                                 "\"email\": \"john@xmedius.com\"," +
                                 "\"company_name\": \"\"}]}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.SearchRecipientSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.SearchRecipientAsync("John");

            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        public async Task NewFileAsync_Success()
        {
            var expectedJson = "{\"temporary_document_guid\":\"6c58a1335a3142d6b140c06003c45d58\",\"upload_url\":\"http://fileserver.lvh.me/xmss/DteeDmb-2zfN5WtC111OcWbl96EVtI=\"}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.NewFileSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var fileParamsJson = "{\"temporary_document\": {" +
                                       "\"document_file_size\": \"10485760\"}," +
                                       "\"multipart\": \"false\"," +
                                       "\"public_encryption_key\": \"RBQISzMk9KwkdBKDVw8sQK0g2J0Zhy9rb9ax\"}";

            var json = await jsonClientSuccess.NewFileAsync(SAFEBOX_GUID, fileParamsJson);

            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task NewFileAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.NewFileError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.BadRequest));

            try
            {
                var error = await jsonClientError.NewFileAsync(SAFEBOX_GUID, "{}");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(400, e.Code);
                throw;
            }
        }

        [TestMethod]
        public async Task ReplyAsync_Success()
        {
            var expectedJson = "{\"result\": true,\"message\": \"SafeBox successfully updated.\"}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.ReplySuccess) && message.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var replyJson = "{\"safebox\":{\"message\":\"a note\",\"consent\":true,\"document_ids\": [\"1\", \"2\"]}}";

            var json = await jsonClientSuccess.ReplyAsync(SAFEBOX_GUID, replyJson);

            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task ReplyAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.ReplyError) && message.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.Forbidden));

            try
            {
                var replyJson = "{\"safebox\":{\"message\":\"a note\",\"consent\":true,\"document_ids\": [\"1\", \"2\"]}}";
                var error = await jsonClientError.ReplyAsync(SAFEBOX_GUID, replyJson);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(403, e.Code);
                throw;
            }
        }

        [TestMethod]
        public async Task GetConsentGroupMessagesAsync_Success()
        {
            var expectedJson = "{ \"consent_message_group\": {" +
                                     "\"id\": 1," +
                                     "\"name\": \"Default\"," +
                                     "\"created_at\": \"2016-08-29T14:52:26.085Z\"," +
                                     "\"updated_at\": \"2016-08-29T14:52:26.085Z\"," +
                                     "\"consent_messages\": [{" +
                                        "\"locale\": \"en\"," +
                                        "\"value\": \"Lorem ipsum\"," +
                                        "\"created_at\": \"2016-08-29T14:52:26.085Z\"," +
                                        "\"updated_at\": \"2016-08-29T14:52:26.085Z\" }]}}";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.ConsentMessageGroupSuccess)),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedJson)
                });

            var json = await jsonClientSuccess.GetConsentGroupMessagesAsync(1);

            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetConsentGroupMessagesAsync_Error()
        {
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == Urls.GetUri(Urls.ConsentMessageGroupError)),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exceptions.MakeRequestException(HttpStatusCode.NotFound));

            try
            {
                var error = await jsonClientError.GetConsentGroupMessagesAsync(42);
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(404, e.Code);
                throw;
            }
        }

    }
}
