using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace XMedius.SendSecure.Test
{
    [TestClass]
    public class ParticipantTest
    {
        private static readonly string participantJson = @"{  ""id"":""7a3c51e00a004917a8f5db807180fcc5"",
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
                                                                       ""updated_at"":""2017-05-24T14:45:35.062Z"",
                                                                       ""_destroy"":true }
                                                                  ]}}";

        private static readonly Helpers.Participant participant = Helpers.Participant.FromJson(participantJson);

        [TestMethod]
        public void Participant_ToJson_OnCreation()
        {
            string expectedJson = @"{""participant"":{
                                        ""email"":""participant@example.com"",
                                        ""first_name"":""Test"",
                                        ""last_name"":""Participant"",
                                        ""company_name"":""Test Company"",
                                        ""contact_methods"":[
                                            {""destination_type"":""home_phone"",
                                                ""destination"":""5145550001""}
                                        ]}}";

            Assert.AreEqual(JToken.Parse(expectedJson).ToString(Newtonsoft.Json.Formatting.None), participant.UpdateFor<Helpers.Participant.Creation>().ToJson());
        }

        [TestMethod]
        public void Participant_ToJson_OnEdition()
        {
            string expectedJson = @"{""participant"":{
                                        ""email"":""participant@example.com"",
                                        ""first_name"":""Test"",
                                        ""last_name"":""Participant"",
                                        ""company_name"":""Test Company"",
                                        ""locked"":false,
                                        ""contact_methods"":[
                                            {   ""destination_type"":""home_phone"",
                                                ""destination"":""5145550001"",
                                                ""id"":1 }
                                        ]}}";

            Assert.AreEqual(JToken.Parse(expectedJson).ToString(Newtonsoft.Json.Formatting.None), participant.UpdateFor<Helpers.Participant.Edition>().ToJson());
        }

        [TestMethod]
        public void Participant_ToJson_OnContactDestruction()
        {
            string expectedJson = @"{""participant"":{
                                        ""email"":""participant@example.com"",
                                        ""first_name"":""Test"",
                                        ""last_name"":""Participant"",
                                        ""company_name"":""Test Company"",
                                        ""locked"":false,
                                        ""contact_methods"":[
                                            {   ""destination_type"":""home_phone"",
                                                ""destination"":""5145550001"",
                                                ""id"":1,
                                                ""_destroy"":true }
                                        ]}}";

            Assert.AreEqual(JToken.Parse(expectedJson).ToString(Newtonsoft.Json.Formatting.None), participant.UpdateFor<Helpers.Participant.Destruction>().OfFollowingContacts(new List<int?> { 1 }).ToJson());
        }

        [TestMethod]
        public void Participant_FromJson()
        {
            var json = @"{  ""id"":""7a3c51e00a004917a8f5db807180fcc5"",
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

            Helpers.Participant participant = Helpers.Participant.FromJson(json);
            Assert.AreEqual("7a3c51e00a004917a8f5db807180fcc5", participant.Id);
            Assert.IsInstanceOfType(participant.GuestOptions, typeof(Helpers.GuestOptions));
            Assert.AreEqual(1, participant.GuestOptions.ContactMethods.Count);
            Assert.IsInstanceOfType(participant.GuestOptions.ContactMethods[0], typeof(Helpers.ContactMethod));
            Assert.AreEqual(1, participant.GuestOptions.ContactMethods[0].Id);
        }

        [TestMethod]
        public void Participant_Update()
        {
            var json = @"{  ""id"":""7a3c51e00a004917a8f5db807180fcc5"",
                            ""first_name"":""Test"",
                            ""last_name"":""Participant"",
                            ""email"":""participant1@example.com"",
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

            var participant = new Helpers.Participant("participant@example.com", "Test", "Participant", "Test Company");

            participant.Update(json);
            Assert.AreEqual("7a3c51e00a004917a8f5db807180fcc5", participant.Id);
            Assert.AreEqual("participant1@example.com", participant.Email);
            Assert.IsInstanceOfType(participant.GuestOptions, typeof(Helpers.GuestOptions));
            Assert.AreEqual(1, participant.GuestOptions.ContactMethods.Count);
            Assert.IsInstanceOfType(participant.GuestOptions.ContactMethods[0], typeof(Helpers.ContactMethod));
            Assert.AreEqual(1, participant.GuestOptions.ContactMethods[0].Id);
        }
    }
}
