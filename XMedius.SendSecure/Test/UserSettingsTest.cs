using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XMedius.SendSecure.Test
{

    [TestClass]
    public class UserSettingsTest
    {
        [TestMethod]
        public void UserSettings_fromJson()
        {
            var json = @"{ ""created_at"": ""2017-05-26T19:27:27.798Z"",
                           ""updated_at"": ""2017-05-24T14:45:35.062Z"",
                           ""mask_note"": false,
                           ""open_first_transaction"": false,
                           ""mark_as_read"": true,
                           ""mark_as_read_delay"": 5,
                           ""remember_key"": true,
                           ""default_filter"": ""everything"",
                           ""recipient_language"": ""en"",
                           ""secure_link"": {
                               ""enabled"": true,
                               ""url"": ""https://sendsecure.integration.xmedius.com/r/612328d944b842c68418375ffdc87b3f"",
                               ""security_profile_id"": 1 }
                       }";

            var userSettings = Helpers.UserSettings.FromJson(json);
            Assert.AreEqual(false, userSettings.MaskNote);
            Assert.AreEqual(false, userSettings.OpenFirstTransaction);
            Assert.AreEqual(true, userSettings.MarkAsRead);
            Assert.AreEqual(5, userSettings.MarkAsReadDelay);
            Assert.AreEqual(true, userSettings.RememberKey);
            Assert.AreEqual("everything", userSettings.DefaultFilter);
            Assert.AreEqual("en", userSettings.RecipientLanguage);
            Assert.IsInstanceOfType(userSettings.SecureLink, typeof(Helpers.PersonnalSecureLink));
            Assert.AreEqual(true, userSettings.SecureLink.Enabled);
            Assert.AreEqual("https://sendsecure.integration.xmedius.com/r/612328d944b842c68418375ffdc87b3f", userSettings.SecureLink.Url);
            Assert.AreEqual(1, userSettings.SecureLink.SecurityProfileId);
        }
    }
}
