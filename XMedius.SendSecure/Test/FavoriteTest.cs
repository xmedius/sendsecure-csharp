using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace XMedius.SendSecure.Test
{
    [TestClass]
    public class FavoriteTest
    {
        private static readonly string favoriteJson = @"{ ""id"":123,
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
                                                              ""updated_at"":""2017-05-24T14:45:35.062Z"",
                                                              ""_destroy"": true }]}";

        private static readonly Helpers.Favorite favorite = Helpers.Favorite.FromJson(favoriteJson);

        [TestMethod]
        public void Favorite_FromJson()
        {
            Assert.AreEqual(123, favorite.Id);
            Assert.AreEqual("Test", favorite.FirstName);
            Assert.AreEqual("Favorite", favorite.LastName);
            Assert.AreEqual("favorite@example.com", favorite.Email);
            Assert.AreEqual("Test Company", favorite.CompanyName);
            Assert.AreEqual(1, favorite.OrderNumber);
            Assert.AreEqual(1, favorite.ContactMethods.Count);
            Assert.IsInstanceOfType(favorite.ContactMethods[0], typeof(Helpers.ContactMethod));
            Assert.AreEqual(1, favorite.ContactMethods[0].Id);
            Assert.AreEqual(Helpers.ContactMethod.DestinationTypeT.HomePhone, favorite.ContactMethods[0].DestinationType);
            Assert.AreEqual("5145550001", favorite.ContactMethods[0].Destination);

        }

        [TestMethod]
        public void Favorite_ToJson_OnCreation()
        {
            string expectedJson = @"{""favorite"":{
                                        ""email"":""favorite@example.com"",
                                        ""first_name"":""Test"",
                                        ""last_name"":""Favorite"",
                                        ""company_name"":""Test Company"",
                                        ""contact_methods"":[
                                            { ""destination_type"":""home_phone"",
                                              ""destination"":""5145550001""}
                                        ]}}";
            Assert.AreEqual(JToken.Parse(expectedJson).ToString(Newtonsoft.Json.Formatting.None), favorite.UpdateFor<Helpers.Favorite.Creation>().ToJson());
        }

        [TestMethod]
        public void Favorite_ToJson_OnEdition()
        {
            string expectedJson = @"{""favorite"":{
                                        ""email"":""favorite@example.com"",
                                        ""first_name"":""Test"",
                                        ""last_name"":""Favorite"",
                                        ""company_name"":""Test Company"",
                                        ""order_number"":1,
                                        ""contact_methods"":[
                                            { ""destination_type"":""home_phone"",
                                              ""destination"":""5145550001"",
                                              ""id"":1 }]}}";

            Assert.AreEqual(JToken.Parse(expectedJson).ToString(Newtonsoft.Json.Formatting.None), favorite.UpdateFor<Helpers.Favorite.Edition>().ToJson());
        }

        [TestMethod]
        public void Favorite_ToJson_OnContactDestruction()
        {
            string expectedJson = @"{""favorite"":{
                                        ""email"":""favorite@example.com"",
                                        ""first_name"":""Test"",
                                        ""last_name"":""Favorite"",
                                        ""company_name"":""Test Company"",
                                        ""order_number"":1,
                                        ""contact_methods"":[
                                            { ""destination_type"":""home_phone"",
                                              ""destination"":""5145550001"",
                                              ""id"":1,
                                              ""_destroy"":true }]}}";
            var favorite = Helpers.Favorite.FromJson(favoriteJson);
            Assert.AreEqual(JToken.Parse(expectedJson).ToString(Newtonsoft.Json.Formatting.None), favorite.UpdateFor<Helpers.Favorite.Destruction>().OfFollowingContacts(new List<int?> { 1 }).ToJson());
        }

        [TestMethod]
        public void Favorite_Update()
        {
            var newFavorite = new Helpers.Favorite("favorite123@example.com", "TestUpdate", "Favorite", "Test Company");

            favorite.Update(favoriteJson);
            Assert.AreEqual(123, favorite.Id);
            Assert.AreEqual("favorite@example.com", favorite.Email);
            Assert.AreEqual(1, favorite.ContactMethods.Count);
            Assert.IsInstanceOfType(favorite.ContactMethods[0], typeof(Helpers.ContactMethod));
            Assert.AreEqual(1, favorite.ContactMethods[0].Id);
        }
    }
}
