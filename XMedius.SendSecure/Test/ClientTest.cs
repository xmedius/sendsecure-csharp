using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using Moq;
using Moq.Protected;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.IO;

namespace XMedius.SendSecure.Test
{
    [TestClass]
    public class ClientTest
    {
        private Client client;
        private static Mock<HttpClientHandler> mockHandler;
        private static Mock<JsonClient> jsonClientMock;
        private static readonly Uri PortalUrlUriSuccess = new Uri("https://portal.xmedius.com/services/testsuccess/portal/host");
        private static readonly Uri PortalUrlUriSuccess2 = new Uri("https://portal.xmedius.com/services/testurisuccess/portal/host");
        private static readonly Uri UserTokenUriSuccess = new Uri("https://portal.xmedius.com/api/user_token");

        private static readonly Uri PortalUrlUriNotFound = new Uri("https://portal.xmedius.com/services/testnotfound/portal/host");
        private static readonly Uri Endpoint = new Uri("https://portal.xmedius.com");

        private static readonly string TOKEN = "USER|29401642-b24f-4986-af3d-67af2e3f893c";
        private static readonly int? USER_ID = 123;

        [TestInitialize()]
        public void Initialize()
        {
            jsonClientMock = new Mock<JsonClient>();
            client = new Client(jsonClientMock.Object, "test_company", Endpoint);
            
            mockHandler = new Mock<HttpClientHandler>();

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == PortalUrlUriSuccess),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("https://portal.xmedius.com") });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == PortalUrlUriSuccess2),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("https://portal.xmedius.com") });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == PortalUrlUriNotFound),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == UserTokenUriSuccess
            && JsonConvert.DeserializeObject<JsonObjects.GetTokenRequest>(message.Content.ReadAsStringAsync().Result).username == "testuser"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(new JsonObjects.UserToken
                {
                    Result = true,
                    Token = TOKEN,
                    UserId = USER_ID
                }))
                });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == UserTokenUriSuccess 
                && JsonConvert.DeserializeObject<JsonObjects.GetTokenRequest>(message.Content.ReadAsStringAsync().Result).permalink == "testurisuccess"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("{\"status\":\"404\",\"error\":\"Not Found\"}") });

            var response = new JsonObjects.GetTokenResponseError { Code = 100, Message = "unexpected error", Result = false };
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == UserTokenUriSuccess 
            && JsonConvert.DeserializeObject<JsonObjects.GetTokenRequest>(message.Content.ReadAsStringAsync().Result).username == "wronguser"), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(JsonConvert.SerializeObject(new JsonObjects.GetTokenResponseError
            {
                Code = 102,
                Message = "invalid credentials",
                Result = false
            }))
            });

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(message => message.RequestUri == UserTokenUriSuccess
            && JsonConvert.DeserializeObject<JsonObjects.GetTokenRequest>(message.Content.ReadAsStringAsync().Result).username == "wronguser"), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new JsonObjects.GetTokenResponseError
                {
                    Code = 102,
                    Message = "invalid credentials",
                    Result = false
                }))
            });

        }

        [TestMethod]
        public async Task GetUserTokenAsyncTest_Success()
        {
            Utils.HttpUtil.HttpClient = new HttpClient(mockHandler.Object);

            JsonObjects.UserToken token = await Client.GetUserTokenAsync("testsuccess", "testuser", "testpass", "test", "test");

            Assert.AreEqual(TOKEN, token.Token);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetUserTokenAsyncTest_UrlNotFound()
        {
            Utils.HttpUtil.HttpClient = new HttpClient(mockHandler.Object);

            try
            {
                JsonObjects.UserToken token = await Client.GetUserTokenAsync("testnotfound", "testuser", "testpass", "test", "test");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(404, e.Code);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetUserTokenAsyncTest_UserTokenNotFound()
        {
            Utils.HttpUtil.HttpClient = new HttpClient(mockHandler.Object);

            try
            {
                JsonObjects.UserToken token = await Client.GetUserTokenAsync("testurisuccess", "testuser", "testpass", "test", "test");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(404, e.Code);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetUserTokenAsyncTest_DefaultUserTokenWrongCredentials()
        {
            Utils.HttpUtil.HttpClient = new HttpClient(mockHandler.Object);

            try
            {
                JsonObjects.UserToken token = await Client.GetUserTokenAsync("testsuccess", "wronguser", "testpass", "test", "test");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(102, e.Code);
                throw;
            }
        }

        [TestMethod]
        public async Task InitializeSafebox_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            string expectedJson = @"{""guid"":""dc6f21e0f02c4112874f8b5653b795e4"",""public_encryption_key"":""public_encryption_key"", ""upload_url"":""upload_url""}";
            jsonClientMock.Setup(x => x.NewSafeboxAsync(safebox.UserEmail, default(CancellationToken))).ReturnsAsync(expectedJson);

            var returnedSafebox = await client.InitializeSafeboxAsync(safebox, default(CancellationToken));
            Assert.AreSame(returnedSafebox, safebox);
            Assert.AreEqual("dc6f21e0f02c4112874f8b5653b795e4", safebox.Guid);
            Assert.AreEqual("public_encryption_key", safebox.PublicEncryptionKey);
            Assert.AreEqual("upload_url", safebox.UploadUrl);
        }

        [TestMethod]
        public async Task UploadAttachment_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.UploadUrl = "http://fileserver.lvh.me/xmss/DteeDmb-2zfN5WtC...7111OcWbl96EVtI=";
            var attachment = new Helpers.Attachment(new MemoryStream(new byte[0]), "testFile.txt", 0);

            string expectedJson = @"{""temporary_document"":{""document_guid"":""65f53ec1282c454fa98439dbda134093""}}";
            jsonClientMock.Setup(x => x.UploadFileAsync(new Uri(safebox.UploadUrl), attachment.Stream, attachment.ContentType, attachment.FileName, attachment.Size, default(CancellationToken))).ReturnsAsync(expectedJson);

            var returnedAttachment = await client.UploadAttachmentAsync(safebox, attachment, default(CancellationToken));
            Assert.AreSame(returnedAttachment, attachment);
            Assert.AreEqual("65f53ec1282c454fa98439dbda134093", attachment.Guid);
        }

        [TestMethod]
        public async Task CommitSafebox_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";
            safebox.Participants.Add(new Helpers.Participant("participant@example.com"));

            string expectedJson = @"{ ""guid"": ""dc6f21e0f02c4112874f8b5653b795e4"",
                                      ""user_id"": 4,
                                      ""enterprise_id"": 4,
                                      ""subject"": ""Donec rutrum congue leo eget malesuada. "",
                                      ""expiration"": ""2017 -05-31T14:42:27.258Z"",
                                      ""notification_language"": ""en"",
                                      ""status"": ""in_progress"",
                                      ""security_profile_name"": ""All Contact Method Allowed!"",
                                      ""security_code_length"": 6,
                                      ""allowed_login_attempts"": 10,
                                      ""allow_remember_me"": true,
                                      ""allow_sms"": true,
                                      ""allow_voice"": true,
                                      ""allow_email"": true,
                                      ""reply_enabled"": true,
                                      ""group_replies"": true,
                                      ""code_time_limit"": 5,
                                      ""encrypt_message"": true,
                                      ""two_factor_required"": true,
                                      ""auto_extend_value"": 6,
                                      ""auto_extend_unit"": ""hours"",
                                      ""retention_period_type"": ""discard_at_expiration"",
                                      ""retention_period_value"": null,
                                      ""retention_period_unit"": ""hours"",
                                      ""delete_content_on"": null,
                                      ""allow_manual_delete"": true,
                                      ""allow_manual_close"": true,
                                      ""email_notification_enabled"": true,
                                      ""preview_url"": ""https://sendsecure.integration.xmedius.com/s/845459484b674055bec4ddf2ba5ab60e/preview"",
                                      ""encryption_key"": null,
                                      ""created_at"": ""2017-05-24T14:42:27.289Z"",
                                      ""updated_at"": ""2017-05-24T14:42:27.526Z"",
                                      ""latest_activity"": ""2017-05-24T14:42:27.463Z""
                                   }";
            jsonClientMock.Setup(x => x.CommitSafeboxAsync(safebox.ToJson(), default(CancellationToken))).ReturnsAsync(expectedJson);

            var returnedSafebox = await client.CommitSafeboxAsync(safebox, default(CancellationToken));
            Assert.AreSame(returnedSafebox, safebox);
            Assert.AreEqual("dc6f21e0f02c4112874f8b5653b795e4", safebox.Guid);
            Assert.AreEqual(4, safebox.UserId);
            Assert.AreEqual(DateTime.Parse("2017-05-24T14:42:27.289Z").ToUniversalTime(), safebox.CreatedAt);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task CommitSafebox_nullSafeboxError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var returnedSafebox = await client.CommitSafeboxAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task CommitSafebox_noRecipientsError()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            try
            {
                var returnedSafebox = await client.CommitSafeboxAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Participants cannot be empty", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task SecurityProfiles_success()
        {
            string expectedJson = @"{ ""default"": 7,
                                      ""security_profiles"": [
                                            { ""id"": 7,
                                              ""name"": ""Default"",
                                              ""description"": null,
                                              ""created_at"": ""2016-08-29T14:52:26.085Z"",
                                              ""updated_at"": ""2016-08-29T14:52:26.085Z"",
                                              ""allowed_login_attempts"":
                                                    { ""value"": 3,
                                                      ""modifiable"": false } },
                                            { ""id"": 1,
                                              ""name"": ""Security Profile"",
                                              ""description"": ""Description"",
                                              ""created_at"": ""2016-08-29T14:52:26.085Z"",
                                              ""updated_at"": ""2016-08-29T14:52:26.085Z"",
                                              ""allowed_login_attempts"":
                                                    { ""value"": 1,
                                                      ""modifiable"": false } }
                                        ]
                                    }";
            jsonClientMock.Setup(x => x.GetSecurityProfilesAsync("user@example.com", default(CancellationToken))).ReturnsAsync(expectedJson);

            List<Helpers.SecurityProfile> returnedSecurityProfiles = await client.SecurityProfilesAsync("user@example.com", default(CancellationToken));
            Assert.AreEqual(2, returnedSecurityProfiles.Count);
            Assert.AreEqual(7, returnedSecurityProfiles[0].Id);
            Assert.IsInstanceOfType(returnedSecurityProfiles[0].AllowedLoginAttempts, typeof(Helpers.SecurityProfile.ValueT<int?>));
            Assert.IsInstanceOfType(returnedSecurityProfiles[1].AllowedLoginAttempts, typeof(Helpers.SecurityProfile.ValueT<int?>));
        }

        [TestMethod]
        public async Task EnterpriseSettings_success()
        {
            string expectedJson = @"{ ""default_security_profile_id"": 7,
                                      ""pdf_language"": ""en"",
                                      ""use_pdfa_audit_records"": false,
                                      ""extension_filter"":{
                                           ""mode"": ""forbid"",
                                           ""list"": [""bat""]
                                      },
                                      ""created_at"": ""2016-09-08T18:54:43.018Z"",
                                      ""updated_at"": ""2017-03-23T16:12:09.411Z"",
                                    }";
            jsonClientMock.Setup(x => x.GetEnterpriseSettingsAsync(default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.EnterpriseSettings returnedEnterpriseSettings = await client.EnterpriseSettingsAsync(default(CancellationToken));
            Assert.AreEqual(7, returnedEnterpriseSettings.DefaultSecurityProfileId);
            Assert.IsInstanceOfType(returnedEnterpriseSettings.ExtensionFilter, typeof(Helpers.ExtensionFilter));
            Assert.AreEqual("forbid", returnedEnterpriseSettings.ExtensionFilter.Mode);
        }

        [TestMethod]
        public async Task DefaultSecurityProfile_success()
        {
            string expectedSettingsJson = @"{ ""default_security_profile_id"": 7,
                                              ""pdf_language"": ""en"",
                                              ""use_pdfa_audit_records"": false,
                                              ""extension_filter"":{
                                                   ""mode"": ""forbid"",
                                                   ""list"": [""bat""]
                                              },
                                              ""created_at"": ""2016-09-08T18:54:43.018Z"",
                                              ""updated_at"": ""2017-03-23T16:12:09.411Z"",
                                            }";

            string expectedProfilesJson = @"{ ""default"": 7,
                                              ""security_profiles"": [
                                                    { ""id"": 7,
                                                      ""name"": ""Default"",
                                                      ""description"": null,
                                                      ""created_at"": ""2016-08-29T14:52:26.085Z"",
                                                      ""updated_at"": ""2016-08-29T14:52:26.085Z"",
                                                      ""allowed_login_attempts"":
                                                            { ""value"": 3,
                                                              ""modifiable"": false } }
                                                ]
                                            }";
            jsonClientMock.Setup(x => x.GetEnterpriseSettingsAsync(default(CancellationToken))).ReturnsAsync(expectedSettingsJson);
            jsonClientMock.Setup(x => x.GetSecurityProfilesAsync("user@example.com", default(CancellationToken))).ReturnsAsync(expectedProfilesJson);

            Helpers.SecurityProfile returnedProfile = await client.DefaultSecurityProfileAsync("user@example.com",default(CancellationToken));
            Assert.AreEqual(7, returnedProfile.Id);
        }

        [TestMethod]
        public async Task UserSettings_success()
        {
            string expectedJson = @"{ ""remember_key"": true,
                                      ""mark_as_read"": true,
                                      ""open_first_transaction"": false,
                                      ""recipient_language"": ""fr"",
                                      ""secure_link"":{
                                           ""security_profile_id"": 1,
                                           ""url"": ""url"",
                                           ""enabled"": ""true""
                                      },
                                      ""created_at"": ""2016-09-08T18:54:43.018Z"",
                                      ""updated_at"": ""2017-03-23T16:12:09.411Z"",
                                    }";
            jsonClientMock.Setup(x => x.GetUserSettingsAsync(default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.UserSettings returnedUserSettings = await client.UserSettingsAsync(default(CancellationToken));
            Assert.AreEqual(true, returnedUserSettings.RememberKey);
            Assert.IsInstanceOfType(returnedUserSettings.SecureLink, typeof(Helpers.PersonnalSecureLink));
        }

        [TestMethod]
        public async Task Favorites_success()
        {
            string expectedJson = @"{ ""favorites"": [
                                        { ""email"":""favorite1@example.com"",
                                          ""contact_methods"":[
                                                {""id"":1,
                                                ""destination_type"":""office_phone"",
                                                ""destination"":""5145550001"",
                                                ""created_at"":""2017-05-26T19:27:27.798Z"",
                                                ""updated_at"":""2017-05-24T14:45:35.062Z""}
                                            ] },
                                        { ""email"":""favorite2@example.com"",
                                          ""contact_methods"":[
                                                {""id"":2,
                                                ""destination_type"":""home_phone"",
                                                ""destination"":""5145550002"",
                                                ""created_at"":""2017-05-26T19:27:27.798Z"",
                                                ""updated_at"":""2017-05-24T14:45:35.062Z""},
                                                {""id"":3,
                                                ""destination_type"":""cell_phone"",
                                                ""destination"":""5145550003"",
                                                ""created_at"":""2017-05-26T19:27:27.798Z"",
                                                ""updated_at"":""2017-05-24T14:45:35.062Z""}
                                        ] }
                                        ]
                                    }";

            jsonClientMock.Setup(x => x.GetFavoritesAsync(default(CancellationToken))).ReturnsAsync(expectedJson);

            List<Helpers.Favorite> returnedFavorites = await client.FavoritesAsync(default(CancellationToken));
            Assert.AreEqual(2, returnedFavorites.Count);
            Assert.AreEqual("favorite1@example.com", returnedFavorites[0].Email);
            Assert.AreEqual(2, returnedFavorites[1].ContactMethods.Count);
            Assert.IsInstanceOfType(returnedFavorites[1].ContactMethods[1], typeof(Helpers.ContactMethod));
            Assert.AreEqual(3, returnedFavorites[1].ContactMethods[1].Id);
        }

        [TestMethod]
        public async Task CreateFavorite_success()
        {
            var favorite = new Helpers.Favorite("favorite@example.com");

            string expectedJson = @"{ ""id"":123,
                                      ""first_name"":""Test"",
                                      ""last_name"":""Favorite"",
                                      ""email"":""favorite@example.com"",
                                      ""company_name"":""Test Company"",
                                      ""order_number"":1,
                                      ""created_at"":""2017-05-26T19:27:27.798Z"",
                                      ""updated_at"":""2017-05-24T14:45:35.062Z"",
                                      ""contact_methods"":[
                                        {""id"":1,
                                            ""destination_type"":""home_phone"",
                                            ""destination"":""5145550001"",
                                            ""created_at"":""2017-05-26T19:27:27.798Z"",
                                            ""updated_at"":""2017-05-24T14:45:35.062Z""}
                                        ]
                                    }";
            jsonClientMock.Setup(x => x.CreateFavoriteAsync(favorite.UpdateFor<Helpers.Favorite.Creation>().ToJson(), default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.Favorite returnedFavorite = await client.CreateFavoriteAsync(favorite, default(CancellationToken));
            Assert.AreEqual(1, returnedFavorite.OrderNumber);
            Assert.AreEqual(1, returnedFavorite.ContactMethods.Count);
            Assert.IsInstanceOfType(returnedFavorite.ContactMethods[0], typeof(Helpers.ContactMethod));
            Assert.AreEqual(1, returnedFavorite.ContactMethods[0].Id);
        }

        [TestMethod]
        public async Task EditFavorite_success()
        {
            var favorite = new Helpers.Favorite("favorite@example.com");
            favorite.Id = 123;

            string expectedJson = @"{ ""id"":123,
                                      ""first_name"":""Test"",
                                      ""last_name"":""Favorite"",
                                      ""email"":""favorite@example.com"",
                                      ""company_name"":""Test Company"",
                                      ""order_number"":1,
                                      ""created_at"":""2017-05-26T19:27:27.798Z"",
                                      ""updated_at"":""2017-05-24T14:45:35.062Z"",
                                      ""contact_methods"":[
                                        {""id"":1,
                                            ""destination_type"":""home_phone"",
                                            ""destination"":""5145550001"",
                                            ""created_at"":""2017-05-26T19:27:27.798Z"",
                                            ""updated_at"":""2017-05-24T14:45:35.062Z""}
                                        ]
                                    }";
            jsonClientMock.Setup(x => x.EditFavoriteAsync(favorite.UpdateFor<Helpers.Favorite.Edition>().ToJson(), favorite.Id, default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.Favorite returnedFavorite = await client.EditFavoriteAsync(favorite, default(CancellationToken));
            Assert.AreEqual(1, returnedFavorite.OrderNumber);
            Assert.AreEqual(1, returnedFavorite.ContactMethods.Count);
            Assert.IsInstanceOfType(returnedFavorite.ContactMethods[0], typeof(Helpers.ContactMethod));
            Assert.AreEqual(1, returnedFavorite.ContactMethods[0].Id);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task EditFavorite_noIdError()
        {
            var favorite = new Helpers.Favorite("favorite@example.com");

            try
            {
                var returnedSafebox = await client.EditFavoriteAsync(favorite, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Favorite id cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task DeleteFavoriteContactMethods_success()
        {
            var favorite = Helpers.Favorite.FromJson(@"{ ""id"":123,
                                                          ""first_name"":""Test"",
                                                          ""last_name"":""Favorite"",
                                                          ""email"":""favorite@example.com"",
                                                          ""company_name"":""Test Company"",
                                                          ""order_number"":1,
                                                          ""created_at"":""2017-05-26T19:27:27.798Z"",
                                                          ""updated_at"":""2017-05-24T14:45:35.062Z"",
                                                          ""contact_methods"":[
                                                            { ""id"":1,
                                                              ""verified"": false,
                                                              ""destination_type"":""home_phone"",
                                                              ""destination"":""5145550001"",
                                                              ""created_at"":""2017-05-26T19:27:27.798Z"",
                                                              ""updated_at"":""2017-05-24T14:45:35.062Z"" }]}");

            string expectedJson = @"{ ""id"":123,
                                      ""first_name"":""Test"",
                                      ""last_name"":""Favorite"",
                                      ""email"":""favorite@example.com"",
                                      ""company_name"":""Test Company"",
                                      ""order_number"":1,
                                      ""created_at"":""2017-05-26T19:27:27.798Z"",
                                      ""updated_at"":""2017-05-24T14:45:35.062Z"",
                                      ""contact_methods"":[]
                                    }";
            var updatedFavorite = favorite.UpdateFor<Helpers.Favorite.Destruction>().OfFollowingContacts(new List<int?> { 1 }).ToJson();
            jsonClientMock.Setup(x => x.EditFavoriteAsync(updatedFavorite, favorite.Id, default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.Favorite returnedFavorite = await client.DeleteFavoriteContactMethods(favorite, new List<int?> { 1 }, default(CancellationToken));
            Assert.AreEqual(0, returnedFavorite.ContactMethods.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task DeleteFavoriteContactMethods_noIdError()
        {
            var favorite = new Helpers.Favorite("favorite@example.com");

            try
            {
                var returnedSafebox = await client.DeleteFavoriteContactMethods(favorite, new List<int?> { 1 }, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Favorite id cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task DeleteFavorite_success()
        {
            var favorite = new Helpers.Favorite("favorite@example.com");
            favorite.Id = 123;

            jsonClientMock.Setup(x => x.DeleteFavoriteAsync(favorite.Id, default(CancellationToken))).ReturnsAsync("");

            string response = await client.DeleteFavoriteAsync(favorite, default(CancellationToken));
            Assert.AreEqual("", response);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task DeleteFavorite_noIdError()
        {
            var favorite = new Helpers.Favorite("favorite@example.com");

            try
            {
                var returnedSafebox = await client.DeleteFavoriteAsync(favorite, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Favorite id cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task CreateParticipant_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";
            var participant = new Helpers.Participant("participant@example.com");
            participant.Id = "7a3c51e00a004917a8f5db807180fcc5";

            string expectedJson = @"{ ""id"":""7a3c51e00a004917a8f5db807180fcc5"",
                                      ""first_name"":""Test"",
                                      ""last_name"":""Participant"",
                                      ""email"":""participant@example.com"",
                                      ""type"":""guest"",
                                      ""role"":""guest"",
                                      ""guest_options"":{
                                            ""company_name"":""Test Company"",
                                            ""locked"":false,
                                            ""bounced_email"":false,
                                            ""failed_login_attempts"":0,
                                            ""verified"":true,
                                            ""created_at"":""2017-05-26T19:27:27.798Z"",
                                            ""updated_at"":""2017-05-24T14:45:35.062Z"",
                                            ""contact_methods"":[
                                                {""id"":1,
                                                    ""destination_type"":""home_phone"",
                                                    ""destination"":""5145550001"",
                                                    ""verified"":true,
                                                    ""created_at"":""2017-05-26T19:27:27.798Z"",
                                                    ""updated_at"":""2017-05-24T14:45:35.062Z""}
                                            ]
                                        }
                                    }";
            jsonClientMock.Setup(x => x.CreateParticipantAsync(participant.UpdateFor<Helpers.Participant.Creation>().ToJson(), safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.Participant returnedParticipant = await client.CreateParticipantAsync(participant, safebox, default(CancellationToken));
            Assert.AreEqual("7a3c51e00a004917a8f5db807180fcc5", returnedParticipant.Id);
            Assert.IsInstanceOfType(returnedParticipant.GuestOptions, typeof(Helpers.GuestOptions));
            Assert.AreEqual("Test Company", returnedParticipant.GuestOptions.CompanyName);
            Assert.IsInstanceOfType(returnedParticipant.GuestOptions.ContactMethods[0], typeof(Helpers.ContactMethod));
            Assert.AreEqual(1, returnedParticipant.GuestOptions.ContactMethods[0].Id);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task CreateParticipant_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var returnedParticipant = await client.CreateParticipantAsync(null, safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task UpdateParticipant_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";
            var participant = new Helpers.Participant("participant@example.com");
            participant.Id = "7a3c51e00a004917a8f5db807180fcc5";

            string expectedJson = @"{ ""id"":""7a3c51e00a004917a8f5db807180fcc5"",
                                      ""first_name"":""Test"",
                                      ""last_name"":""Participant"",
                                      ""email"":""participant@example.com"",
                                      ""type"":""guest"",
                                      ""role"":""guest"",
                                      ""guest_options"":{
                                            ""company_name"":""Test Company"",
                                            ""locked"":false,
                                            ""bounced_email"":false,
                                            ""failed_login_attempts"":0,
                                            ""verified"":true,
                                            ""created_at"":""2017-05-26T19:27:27.798Z"",
                                            ""updated_at"":""2017-05-24T14:45:35.062Z"",
                                            ""contact_methods"":[
                                                {""id"":1,
                                                    ""destination_type"":""home_phone"",
                                                    ""destination"":""5145550001"",
                                                    ""verified"":true,
                                                    ""created_at"":""2017-05-26T19:27:27.798Z"",
                                                    ""updated_at"":""2017-05-24T14:45:35.062Z""}
                                            ]
                                        }
                                    }";
            jsonClientMock.Setup(x => x.UpdateParticipantAsync(participant.UpdateFor<Helpers.Participant.Edition>().ToJson(), safebox.Guid, participant.Id, default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.Participant returnedParticipant = await client.UpdateParticipantAsync(participant, safebox, default(CancellationToken));
            Assert.AreEqual("7a3c51e00a004917a8f5db807180fcc5", returnedParticipant.Id);
            Assert.IsInstanceOfType(returnedParticipant.GuestOptions, typeof(Helpers.GuestOptions));
            Assert.AreEqual("Test Company", returnedParticipant.GuestOptions.CompanyName);
            Assert.IsInstanceOfType(returnedParticipant.GuestOptions.ContactMethods[0], typeof(Helpers.ContactMethod));
            Assert.AreEqual(1, returnedParticipant.GuestOptions.ContactMethods[0].Id);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task UpdateParticipant_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var returnedParticipant = await client.UpdateParticipantAsync(null, safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task UpdateParticipant_noParticipantIdError()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";
            var participant = new Helpers.Participant("participant@example.com");

            try
            {
                var returnedParticipant = await client.UpdateParticipantAsync(participant, safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Participant Id cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task DeleteParticipantContactMethods_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";
            var participant = Helpers.Participant.FromJson(@"{ ""id"":""7a3c51e00a004917a8f5db807180fcc5"",
                                                               ""first_name"":""Test"",
                                                               ""last_name"":""Participant"",
                                                               ""email"":""participant@example.com"",
                                                               ""type"":""guest"",
                                                               ""role"":""guest"",
                                                               ""guest_options"":{
                                                                  ""company_name"":""Test Company"",
                                                                  ""locked"":false,
                                                                  ""bounced_email"":false,
                                                                  ""failed_login_attempts"":0,
                                                                  ""verified"":true,
                                                                  ""created_at"":""2017-05-26T19:27:27.798Z"",
                                                                  ""updated_at"":""2017-05-24T14:45:35.062Z"",
                                                                  ""contact_methods"":[
                                                                     { ""id"":1,
                                                                       ""destination_type"":""home_phone"",
                                                                       ""destination"":""5145550001"",
                                                                       ""verified"":true,
                                                                       ""created_at"":""2017-05-26T19:27:27.798Z"",
                                                                       ""updated_at"":""2017-05-24T14:45:35.062Z""}
                                                                  ]}}");

            string expectedJson = @"{ ""id"":""7a3c51e00a004917a8f5db807180fcc5"",
                                      ""first_name"":""Test"",
                                      ""last_name"":""Participant"",
                                      ""email"":""participant@example.com"",
                                      ""type"":""guest"",
                                      ""role"":""guest"",
                                      ""guest_options"":{
                                            ""company_name"":""Test Company"",
                                            ""locked"":false,
                                            ""bounced_email"":false,
                                            ""failed_login_attempts"":0,
                                            ""verified"":true,
                                            ""created_at"":""2017-05-26T19:27:27.798Z"",
                                            ""updated_at"":""2017-05-24T14:45:35.062Z"",
                                            ""contact_methods"":[] }}";

            var updatedParticipant = participant.UpdateFor<Helpers.Participant.Destruction>().OfFollowingContacts(new List<int?> { 1 });
            jsonClientMock.Setup(x => x.UpdateParticipantAsync(updatedParticipant.ToJson(), safebox.Guid, participant.Id, default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.Participant returnedParticipant = await client.DeleteParticipantContactMethods(participant, safebox, new List<int?> { 1 }, default(CancellationToken));
            Assert.AreEqual(0, returnedParticipant.GuestOptions.ContactMethods.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task DeleteParticipantContactMethods_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            var participant = new Helpers.Participant("participant@example.com");

            try
            {
                var returnedParticipant = await client.DeleteParticipantContactMethods(participant, safebox, new List<int?> { 1 }, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task DeleteParticipantContactMethods_noParticipantIdError()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";
            var participant = new Helpers.Participant("participant@example.com");

            try
            {
                var returnedParticipant = await client.DeleteParticipantContactMethods(participant, safebox, new List<int?> { 1 }, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Participant id cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task AddTime_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string addTimeJson = "{\"safebox\":{\"add_time_value\":1,\"add_time_unit\":\"days\"}}";

            string expectedJson = @"{ ""result"":true,
                                      ""message"":""SafeBox duration successfully extended."",
                                      ""new_expiration"":""2017-05-14T18:09:05.662Z""
                                    }";
            jsonClientMock.Setup(x => x.AddTimeAsync(addTimeJson, safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            JsonObjects.AddTimeResponseSuccess response = await client.AddTimeAsync(safebox, 1, Helpers.SecurityEnums.TimeUnit.Days, default(CancellationToken));
            Assert.AreEqual(DateTime.Parse("2017-05-14T18:09:05.662Z").ToUniversalTime(), safebox.Expiration);
            Assert.AreEqual(true, response.Result);
            Assert.AreEqual("SafeBox duration successfully extended.", response.Message);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task AddTime_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.AddTimeAsync(safebox, 1, Helpers.SecurityEnums.TimeUnit.Days, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task CloseSafebox_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string expectedJson = "{ \"result\":true, \"message\": \"SafeBox successfully closed.\" }";
            jsonClientMock.Setup(x => x.CloseSafeboxAsync(safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            JsonObjects.RequestResponse response = await client.CloseSafeboxAsync(safebox, default(CancellationToken));
            Assert.AreEqual(true, response.Result);
            Assert.AreEqual("SafeBox successfully closed.", response.Message);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task CloseSafebox_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.CloseSafeboxAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task DeleteSafeboxContent_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string expectedJson = "{ \"result\":true," +
                                    "\"message\":\"SafeBox content successfully deleted.\" }";
            jsonClientMock.Setup(x => x.DeleteSafeboxContentAsync(safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            JsonObjects.RequestResponse response = await client.DeleteSafeboxContentAsync(safebox, default(CancellationToken));
            Assert.AreEqual(true, response.Result);
            Assert.AreEqual("SafeBox content successfully deleted.", response.Message);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task DeleteSafeboxContent_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.DeleteSafeboxContentAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task MarkAsRead_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string expectedJson = "{ \"result\":true }";
            jsonClientMock.Setup(x => x.MarkAsReadAsync(safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            JsonObjects.RequestResponse response = await client.MarkAsReadAsync(safebox, default(CancellationToken));
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task MarkAsRead_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.MarkAsReadAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task MarkAsUnread_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string expectedJson = "{ \"result\":true }";
            jsonClientMock.Setup(x => x.MarkAsUnreadAsync(safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            JsonObjects.RequestResponse response = await client.MarkAsUnreadAsync(safebox, default(CancellationToken));
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task MarkAsUnread_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.MarkAsUnreadAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task FileUrl_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";
            var attachment = new Helpers.Attachment(new MemoryStream(new byte[0]), "testFile.txt", 0);
            attachment.Guid = "65f53ec1282c454fa98439dbda134093";

            string expectedJson = "{\"url\":\"url\"}";
            jsonClientMock.Setup(x => x.GetFileUrlAsync(safebox.Guid, attachment.Guid, safebox.UserEmail, default(CancellationToken))).ReturnsAsync(expectedJson);

            var response = await client.FileUrlAsync(safebox, attachment, default(CancellationToken));
            Assert.AreEqual("url", response);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task FileUrl_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.FileUrlAsync(safebox, null, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task FileUrl_noDocumentGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";
            var attachment = new Helpers.Attachment(new MemoryStream(new byte[0]), "testFile.txt", 0);

            try
            {
                var response = await client.FileUrlAsync(safebox, attachment, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Document Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task AuditRecordUrl_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string expectedJson = "{\"url\":\"url\"}";
            jsonClientMock.Setup(x => x.GetAuditRecordUrlAsync(safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            var response = await client.AuditRecordUrlAsync(safebox, default(CancellationToken));
            Assert.AreEqual("url", response);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task AuditRecordUrl_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.AuditRecordUrlAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task Safeboxes_success()
        {
            string expectedJson = "{\"count\":100," +
                                    "\"previous_page_url\":\"https://sendsecure.integration.xmedius.com/api/v2/safeboxes?status=unread&search=test&per_page=20&page=1\"," +
                                    "\"previous_page_url\":\"https://sendsecure.integration.xmedius.com/api/v2/safeboxes?status=unread&search=test&per_page=20&page=3\"," +
                                    "\"safeboxes\": [{ \"safebox\":{\"guid\":\"dc6f21e0f02c4112874f8b5653b795e4\" }}," +
                                                    "{ \"safebox\": {\"guid\":\"b4d898ada15f42f293e31905c514607f\" }}]}";

            jsonClientMock.Setup(x => x.GetSafeboxesAsync("in_progress", "search", 1, 1, default(CancellationToken))).ReturnsAsync(expectedJson);

            JsonObjects.SafeboxesResponse safeboxesResponse = await client.SafeboxesAsync(Helpers.Safebox.Status.InProgress, "search", 1, 1, default(CancellationToken));
            Assert.AreEqual(100, safeboxesResponse.Count);
            Assert.AreEqual(2, safeboxesResponse.Safeboxes.Count);
            Assert.AreEqual("dc6f21e0f02c4112874f8b5653b795e4", safeboxesResponse.Safeboxes[0].Guid);
        }

        [TestMethod]
        public async Task SafeboxInfo_success()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string expectedJson = "{ \"safebox\": { \"guid\": \"73af62f766ee459e81f46e4f533085a4\"," +
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

            jsonClientMock.Setup(x => x.GetSafeboxInfoAsync(safebox.Guid, "", default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.Safebox returnedSafebox = await client.SafeboxInfoAsync(safebox, null, default(CancellationToken));
            Assert.AreEqual("73af62f766ee459e81f46e4f533085a4", returnedSafebox.Guid);
            Assert.AreEqual(4, returnedSafebox.SecurityOptions.SecurityCodeLength);
            Assert.AreEqual("7a3c51e00a004917a8f5db807180fcc5", returnedSafebox.Participants[0].Id);
            Assert.AreEqual("5a3df276aaa24e43af5aca9b2204a535", returnedSafebox.Messages[0].Documents[0].Id);
            Assert.AreEqual("john44@example.com", returnedSafebox.EventHistory[0].Metadata.Emails[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task SafeboxInfo_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.SafeboxInfoAsync(safebox, null, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task Messages_success()
        {
            Helpers.Safebox safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string expectedJson = @"{""messages"":[{""note"":""Test""}]}";
            jsonClientMock.Setup(x => x.GetSafeboxMessagesAsync(safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            List<Helpers.Message> messages = await client.MessagesAsync(safebox, default(CancellationToken));
            Assert.AreSame(safebox.Messages, messages);
            Assert.AreEqual("Test", messages[0].Note);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task Messages_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.MessagesAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task Participants_success()
        {
            Helpers.Safebox safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string expectedJson = @"{""participants"":[{""id"":""7a3c51e00a004917a8f5db807180fcc5""}]}";
            jsonClientMock.Setup(x => x.GetSafeboxParticipantsAsync(safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            List<Helpers.Participant> participants = await client.ParticipantsAsync(safebox, default(CancellationToken));
            Assert.AreSame(safebox.Participants, participants);
            Assert.AreEqual("7a3c51e00a004917a8f5db807180fcc5", participants[0].Id);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task Participants_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.ParticipantsAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task EventHistory_success()
        {
            Helpers.Safebox safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string expectedJson = @"{""event_history"":[{""type"":""safebox_created_owner""}]}";
            jsonClientMock.Setup(x => x.GetSafeboxEventHistoryAsync(safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            List<Helpers.EventHistory> history = await client.EventHistoryAsync(safebox, default(CancellationToken));
            Assert.AreSame(safebox.EventHistory, history);
            Assert.AreEqual("safebox_created_owner", history[0].Type);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task EventHistory_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.EventHistoryAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task SecurityOptions_success()
        {
            Helpers.Safebox safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string expectedJson = @"{""security_options"":{""security_code_length"":4}}";
            jsonClientMock.Setup(x => x.GetSafeboxSecurityOptionsAsync(safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.SecurityOptions options = await client.SecurityOptionsAsync(safebox, default(CancellationToken));
            Assert.AreSame(safebox.SecurityOptions, options);
            Assert.AreEqual(4, options.SecurityCodeLength);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task SecurityOptions_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.SecurityOptionsAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task DownloadActivity_success()
        {
            Helpers.Safebox safebox = new Helpers.Safebox("user@example.com");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";

            string expectedJson = @"{""download_activity"":{""guests"":[],""owner"":{""id"":""42220c777c30486e80cd3bbfa7f8e82f""}}}";
            jsonClientMock.Setup(x => x.GetSafeboxDownloadActivityAsync(safebox.Guid, default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.DownloadActivity downloadActivity = await client.DownloadActivityAsync(safebox, default(CancellationToken));
            Assert.AreSame(safebox.DownloadActivity, downloadActivity);
            Assert.AreEqual("42220c777c30486e80cd3bbfa7f8e82f", downloadActivity.Owner.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task DownloadActivity_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");

            try
            {
                var response = await client.DownloadActivityAsync(safebox, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task MarkAsReadMessageAsync_success()
        {
            var safebox = new Helpers.Safebox("user@example.com")
            {
                Guid = "dc6f21e0f02c4112874f8b5653b795e4"
            };
            var message = new Helpers.Message()
            {
                Id = "1234567"
            };

            string expectedJson = "{\"result\":true}";
            jsonClientMock.Setup(x => x.MarkAsReadMessageAsync(safebox.Guid, message.Id, default(CancellationToken))).ReturnsAsync(expectedJson);

            JsonObjects.RequestResponse response = await client.MarkAsReadMessageAsync(safebox, message, default(CancellationToken));
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task MarkAsReadMessageAsync_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            var message = new Helpers.Message()
            {
                Id = "1234567"
            };

            try
            {
                var response = await client.MarkAsReadMessageAsync(safebox, message, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task MarkAsReadMessageAsync_noMessageIdError()
        {
            var safebox = new Helpers.Safebox("user@example.com")
            {
                Guid = "dc6f21e0f02c4112874f8b5653b795e4"
            };
            var message = new Helpers.Message();

            try
            {
                var response = await client.MarkAsReadMessageAsync(safebox, message, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Message Id cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task MarkAsUneadMessageAsync_success()
        {
            var safebox = new Helpers.Safebox("user@example.com")
            {
                Guid = "dc6f21e0f02c4112874f8b5653b795e4"
            };
            var message = new Helpers.Message()
            {
                Id = "1234567"
            };

            string expectedJson = "{\"result\":true}";
            jsonClientMock.Setup(x => x.MarkAsUnreadMessageAsync(safebox.Guid, message.Id, default(CancellationToken))).ReturnsAsync(expectedJson);

            JsonObjects.RequestResponse response = await client.MarkAsUnreadMessageAsync(safebox, message, default(CancellationToken));
            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task MarkAsUnreadMessageAsync_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            var message = new Helpers.Message()
            {
                Id = "1234567"
            };

            try
            {
                var response = await client.MarkAsUnreadMessageAsync(safebox, message, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task MarkAsUnreadMessageAsync_noMessageIdError()
        {
            var safebox = new Helpers.Safebox("user@example.com")
            {
                Guid = "dc6f21e0f02c4112874f8b5653b795e4"
            };
            var message = new Helpers.Message();

            try
            {
                var response = await client.MarkAsUnreadMessageAsync(safebox, message, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Message Id cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task SearchRecipientAsync_success()
        {
            string expectedJson = "{ \"results\": [{" +
                                      "\"id\": 1," +
                                      "\"type\": \"favorite\"," +
                                      "\"first_name\": \"John\"," +
                                      "\"last_name\": \"Doe\"," +
                                      "\"email\": \"john@xmedius.com\"," +
                                      "\"company_name\": \"ACME\"}]}";
            jsonClientMock.Setup(x => x.SearchRecipientAsync("John", default(CancellationToken))).ReturnsAsync(expectedJson);

            JsonObjects.SearchRecipientResponseSuccess response = await client.SearchRecipientAsync("John", default(CancellationToken));
            Assert.AreEqual(1, response.Results.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task SearchRecipientAsync_norSearchTermError()
        {
            try
            {
                var response = await client.SearchRecipientAsync(null, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Search term cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task ReplyAsync_Success()
        {
            string expectedNewFileResponse = "{\"temporary_document_guid\":\"6c58a1335a3142d6b140c06003c45d58\"," +
                                              "\"upload_url\":\"http://fileserver.lvh.me/xmss/DteeDmb-2zfN5WtC111OcWbl96EVtI=\"}";
            string expectedUploadFileResponse = "{\"temporary_document\":{\"document_guid\":\"65f53ec1282c454fa98439dbda134093\"}}";
            string expectedReplyResponse = "{\"result\":true,\"message\":\"SafeBox successfully updated.\"}";

            var safebox = new Helpers.Safebox("user@example.com")
            {
                Guid = "d65979185c1a4bbe85ef8ce3458de55b"
            };
            var reply = new Helpers.Reply("Message")
            {
                Consent = true
            };
            
            var attachment = new Helpers.Attachment(new MemoryStream(new byte[0]), "testFile.txt", 0);
            reply.Attachments.Add(attachment);

            jsonClientMock.Setup(x => x.NewFileAsync("d65979185c1a4bbe85ef8ce3458de55b", safebox.TemporaryDocument(attachment.Size), default(CancellationToken))).ReturnsAsync(expectedNewFileResponse);
            jsonClientMock.Setup(x => x.UploadFileAsync(new Uri("http://fileserver.lvh.me/xmss/DteeDmb-2zfN5WtC111OcWbl96EVtI="), attachment.Stream, attachment.ContentType, attachment.FileName, attachment.Size, default(CancellationToken))).ReturnsAsync(expectedUploadFileResponse);
            jsonClientMock.Setup(x => x.ReplyAsync("d65979185c1a4bbe85ef8ce3458de55b", reply.ToJson(), default(CancellationToken))).ReturnsAsync(expectedReplyResponse);

            JsonObjects.RequestResponse response = await client.ReplyAsync(safebox, reply, default(CancellationToken));

            Assert.AreEqual(true, response.Result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task ReplyAsync_noSafeboxGuidError()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            var reply = new Helpers.Reply("Message");

            try
            {
                var response = await client.ReplyAsync(safebox, reply, default(CancellationToken));
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual("Safebox Guid cannot be null", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task GetConsentGroupMessagesAsync_success()
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
            jsonClientMock.Setup(x => x.GetConsentGroupMessagesAsync(1, default(CancellationToken))).ReturnsAsync(expectedJson);

            Helpers.ConsentMessageGroup response = await client.GetConsentGroupMessagesAsync(1, default(CancellationToken));
            Assert.AreEqual(1, response.Id);
            Assert.AreEqual("Default", response.Name);
            Assert.AreEqual(1, response.ConsentMessages.Count);
            Assert.IsInstanceOfType(response.ConsentMessages[0], typeof(Helpers.ConsentMessage));
            Assert.AreEqual("en", response.ConsentMessages[0].Locale);
            Assert.AreEqual("Lorem ipsum", response.ConsentMessages[0].Value);
        }

    }
}
