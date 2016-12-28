using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using Moq;
using Moq.Protected;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Newtonsoft.Json;

namespace XMedius.SendSecure.Test
{
    [TestClass]
    public class ClientTest
    {
        private static Mock<HttpClientHandler> mockHandler;
        private static readonly Uri PortalUrlUriSuccess = new Uri("https://portal.xmedius.com/services/testsuccess/portal/host");
        private static readonly Uri PortalUrlUriSuccess2 = new Uri("https://portal.xmedius.com/services/testurisuccess/portal/host");
        private static readonly Uri UserTokenUriSuccess = new Uri("https://portal.xmedius.com/api/user_token");

        private static readonly Uri PortalUrlUriNotFound = new Uri("https://portal.xmedius.com/services/testnotfound/portal/host");

        private static readonly string TOKEN = "USER|29401642-b24f-4986-af3d-67af2e3f893c";

        [TestInitialize()]
        public void Initialize()
        {
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
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonConvert.SerializeObject(new JsonObjects.GetTokenResponseSuccess
                {
                    Result = true,
                    Token = TOKEN
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

        }

        [TestMethod]
        public async Task GetUserTokenAsyncTest_Success()
        {
            Utils.HttpUtil.HttpClient = new HttpClient(mockHandler.Object);

            string token = await Client.GetUserTokenAsync("testsuccess", "testuser", "testpass", "test", "test");

            Assert.AreEqual(TOKEN, token);
        }

        [TestMethod]
        [ExpectedException(typeof(Exceptions.SendSecureException))]
        public async Task GetUserTokenAsyncTest_UrlNotFound()
        {
            Utils.HttpUtil.HttpClient = new HttpClient(mockHandler.Object);

            try
            {
                string token = await Client.GetUserTokenAsync("testnotfound", "testuser", "testpass", "test", "test");
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
                string token = await Client.GetUserTokenAsync("testurisuccess", "testuser", "testpass", "test", "test");
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
                string token = await Client.GetUserTokenAsync("testsuccess", "wronguser", "testpass", "test", "test");
            }
            catch (Exceptions.SendSecureException e)
            {
                Assert.AreEqual(102, e.Code);
                throw;
            }
        }
    }
}
