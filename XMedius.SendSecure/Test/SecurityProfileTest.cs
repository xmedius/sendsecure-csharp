using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XMedius.SendSecure.Test
{
    [TestClass]
    public class SecurityProfileTest
    {
        [TestMethod]
        public void SecurityProfile_FromJson()
        {
            var json = @"{ ""id"": 7,
                           ""name"": ""email-only"",
                           ""description"": null,
                           ""created_at"": ""2016-08-29T14:52:26.085Z"",
                           ""updated_at"": ""2016-08-29T14:52:26.085Z"",
                           ""allowed_login_attempts"": {
                               ""value"": 3,
                               ""modifiable"": false
                           },
                           ""allow_remember_me"": {
                              ""value"": true,
                              ""modifiable"": false
                           },
                           ""allow_sms"": {
                              ""value"": true,
                              ""modifiable"": false
                           },
                           ""allow_voice"": {
                              ""value"": true,
                              ""modifiable"": false
                           },
                           ""allow_email"": {
                              ""value"": true,
                              ""modifiable"": false
                           },
                           ""code_time_limit"": {
                              ""value"": ""5"",
                              ""modifiable"": false
                           },
                           ""code_length"":{
                              ""value"": 4,
                              ""modifiable"": false
                           },
                           ""auto_extend_value"":{
                              ""value"": 3,
                              ""modifiable"": false
                           },
                           ""auto_extend_unit"":{
                              ""value"": ""days"",
                              ""modifiable"": false
                           },
                           ""two_factor_required"":{
                              ""value"": true,
                              ""modifiable"": false
                           },
                           ""encrypt_attachments"":{
                              ""value"": true,
                              ""modifiable"": false
                           },
                           ""encrypt_message"":{
                              ""value"": true,
                              ""modifiable"": false
                           },
                           ""expiration_value"":{
                              ""value"": 1,
                              ""modifiable"": false
                           },
                           ""expiration_unit"":{
                              ""value"": ""months"",
                              ""modifiable"": false
                           },
                           ""reply_enabled"":{
                              ""value"": true,
                              ""modifiable"": false
                           },
                           ""group_replies"":{
                              ""value"": true,
                              ""modifiable"": false
                           },
                           ""double_encryption"":{
                              ""value"": true,
                              ""modifiable"": true
                           },
                           ""retention_period_type"":{
                              ""value"": ""do_not_discard"",
                              ""modifiable"": false
                           },
                           ""retention_period_value"":{
                              ""value"": null,
                              ""modifiable"": false
                           },
                           ""retention_period_unit"":{
                              ""value"": null,
                              ""modifiable"": false
                           }
                         }";

            var securityProfile = Helpers.SecurityProfile.FromJson(json);
            Assert.AreEqual(7, securityProfile.Id);
            Assert.IsInstanceOfType(securityProfile.AllowedLoginAttempts, typeof(Helpers.SecurityProfile.ValueT<int?>));
            Assert.AreEqual(3, securityProfile.AllowedLoginAttempts.Value);
            Assert.AreEqual(false, securityProfile.AllowedLoginAttempts.Modifiable);
            Assert.IsInstanceOfType(securityProfile.AllowRememberMe, typeof(Helpers.SecurityProfile.ValueT<bool?>));
            Assert.IsInstanceOfType(securityProfile.AllowSms, typeof(Helpers.SecurityProfile.ValueT<bool?>));
            Assert.IsInstanceOfType(securityProfile.AllowVoice, typeof(Helpers.SecurityProfile.ValueT<bool?>));
            Assert.IsInstanceOfType(securityProfile.AllowEmail, typeof(Helpers.SecurityProfile.ValueT<bool?>));
            Assert.IsInstanceOfType(securityProfile.CodeTimeLimit, typeof(Helpers.SecurityProfile.ValueT<int?>));
            Assert.IsInstanceOfType(securityProfile.CodeLength, typeof(Helpers.SecurityProfile.ValueT<int?>));
            Assert.IsInstanceOfType(securityProfile.AutoExtendUnit, typeof(Helpers.SecurityProfile.ValueT<Helpers.SecurityEnums.TimeUnit>));
            Assert.IsInstanceOfType(securityProfile.AutoExtendValue, typeof(Helpers.SecurityProfile.ValueT<int?>));
            Assert.IsInstanceOfType(securityProfile.TwoFactorRequired, typeof(Helpers.SecurityProfile.ValueT<bool?>));
            Assert.IsInstanceOfType(securityProfile.EncryptAttachments, typeof(Helpers.SecurityProfile.ValueT<bool?>));
            Assert.IsInstanceOfType(securityProfile.EncryptMessage, typeof(Helpers.SecurityProfile.ValueT<bool?>));
            Assert.IsInstanceOfType(securityProfile.ExpirationUnit, typeof(Helpers.SecurityProfile.ValueT<Helpers.SecurityEnums.TimeUnit>));
            Assert.IsInstanceOfType(securityProfile.ExpirationValue, typeof(Helpers.SecurityProfile.ValueT<int>));
            Assert.IsInstanceOfType(securityProfile.ReplyEnabled, typeof(Helpers.SecurityProfile.ValueT<bool?>));
            Assert.IsInstanceOfType(securityProfile.GroupReplies, typeof(Helpers.SecurityProfile.ValueT<bool?>));
            Assert.IsInstanceOfType(securityProfile.RetentionPeriodType, typeof(Helpers.SecurityProfile.ValueT<Helpers.SecurityEnums.RetentionPeriod>));
            Assert.IsInstanceOfType(securityProfile.RetentionPeriodValue, typeof(Helpers.SecurityProfile.ValueT<int?>));
            Assert.IsInstanceOfType(securityProfile.RetentionPeriodUnit, typeof(Helpers.SecurityProfile.ValueT<Helpers.SecurityEnums.TimeUnit?>));
        }
    }
}
