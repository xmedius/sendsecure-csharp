using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;

namespace XMedius.SendSecure.Test
{
    [TestClass]
    public class SafeboxTest
    {
        [TestMethod]
        public void Safebox_ToJson()
        {
            var safebox = new Helpers.Safebox("user@example.com", 1, "test_message", "test_subject");
            safebox.Guid = "dc6f21e0f02c4112874f8b5653b795e4";
            var participant1 = new Helpers.Participant("participant1@example.com", "Test", "Participant1", "Test Company");
            participant1.GuestOptions.ContactMethods.Add(new Helpers.ContactMethod { DestinationType = Helpers.ContactMethod.DestinationTypeT.HomePhone, Destination = "5145550001" });
            safebox.Participants.Add(participant1);
            safebox.Participants.Add(new Helpers.Participant("participant2@example.com", "Test", "Participant2", "Test Company"));
            safebox.SecurityOptions.EncryptMessage = true;
            safebox.SecurityOptions.DoubleEncryption = false;
            safebox.PublicEncryptionKey = "Public Encryption Key";
            safebox.Attachments.Add(new Helpers.Attachment(new MemoryStream(new byte[0]), "testFile.txt", 0));
            safebox.Attachments[0].Guid = "97334293-23c2-4c94-8cee-369ddfabb678";

            string expectedJson = @"{""safebox"":{
                                        ""guid"":""dc6f21e0f02c4112874f8b5653b795e4"",
                                        ""subject"":""test_subject"",
                                        ""notification_language"":""en"",
                                        ""message"":""test_message"",
                                        ""security_profile_id"":1,
                                        ""public_encryption_key"":""Public Encryption Key"",
                                        ""user_email"":""user@example.com"",
                                        ""recipients"":[
                                            {""email"":""participant1@example.com"",
                                            ""first_name"":""Test"",
                                            ""last_name"":""Participant1"",
                                            ""company_name"":""Test Company"",
                                            ""contact_methods"":[
                                                {""destination_type"":""home_phone"",
                                                 ""destination"":""5145550001""}
                                            ]},
                                            {""email"":""participant2@example.com"",
                                            ""first_name"":""Test"",
                                            ""last_name"":""Participant2"",
                                            ""company_name"":""Test Company"",
                                            ""contact_methods"":[]}
                                        ],
                                        ""encrypt_message"":true,
                                        ""double_encryption"":false,
                                        ""document_ids"":[""97334293-23c2-4c94-8cee-369ddfabb678""] 
                                        }
                                    }";
            Assert.AreEqual(JToken.Parse(expectedJson).ToString(Newtonsoft.Json.Formatting.None), safebox.ToJson());
        }

        [TestMethod]
        public void Safebox_FromJson()
        {
            var json = @"{  ""guid"":""dc6f21e0f02c4112874f8b5653b795e4"",
                            ""user_id"":1,
                            ""enterprise_id"":1,
                            ""subject"":""test_subject"",
                            ""notification_language"":""en"",
                            ""status"":""in_progress"",
                            ""security_profile_name"":""Security Profile"",
                            ""unread_count"":0,
                            ""double_encryption_status"":""disabled"",
                            ""audit_record_pdf"":""audit_record_pdf"",
                            ""secure_link"":""secure_link"",
                            ""secure_link_title"":""secure link title"",
                            ""email_notification_enabled"":true,
                            ""created_at"":""2017-05-24T14:45:35.062Z"",
                            ""updated_at"":""2017-05-24T14:45:35.062Z"",
                            ""assigned_at"":""2017-05-24T14:45:35.062Z"",
                            ""latest_activity"":""2017-05-24T14:45:35.062Z"",
                            ""expiration"":""2017-05-31T14:45:35.038Z"",
                            ""closed_at"":null,
                            ""content_deleted_at"":null
                        }";
            Helpers.Safebox safebox = Helpers.Safebox.FromJson(json);
            Assert.AreEqual("dc6f21e0f02c4112874f8b5653b795e4", safebox.Guid);
            Assert.IsNull(safebox.UserEmail);
        }

        [TestMethod]
        public void Safebox_Update()
        {
            var safebox = new Helpers.Safebox("user@example.com");
            var json = @"{""guid"":""dc6f21e0f02c4112874f8b5653b795e4"", ""public_encryption_key"":""public_encryption_key"", ""upload_url"":""upload_url""}";
            safebox.Update(json);
            Assert.AreEqual("dc6f21e0f02c4112874f8b5653b795e4", safebox.Guid);
            Assert.AreEqual("public_encryption_key", safebox.PublicEncryptionKey);
            Assert.AreEqual("upload_url", safebox.UploadUrl);
            Assert.AreEqual("user@example.com", safebox.UserEmail);
        }
    }
}