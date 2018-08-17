using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XMedius.SendSecure.Test
{
    [TestClass]
    public class EnterpriseSettingsTest
    {
        [TestMethod]
        public void EnterpriseSettings_fromJson()
        {
            var json = @"{ ""created_at"": ""2016-09-08T18:54:43.018Z"",
                           ""updated_at"": ""2017-03-23T16:12:09.411Z"",
                           ""default_security_profile_id"": 14,
                           ""pdf_language"": ""en"",
                           ""use_pdfa_audit_records"": false,
                           ""international_dialing_plan"": ""ca"",
                           ""extension_filter"": {
                               ""mode"": ""forbid"",
                               ""list"": [""bat"",""bin""]
                           },
                           ""virus_scan_enabled"": false,
                           ""max_file_size_value"": null,
                           ""max_file_size_unit"": null,
                           ""include_users_in_autocomplete"": true,
                           ""include_favorites_in_autocomplete"": true,
                           ""users_public_url"": true
                          }";

            var enterpriseSettings = Helpers.EnterpriseSettings.FromJson(json);
            Assert.AreEqual(14, enterpriseSettings.DefaultSecurityProfileId);
            Assert.AreEqual("en", enterpriseSettings.PdfLanguage);
            Assert.AreEqual(false, enterpriseSettings.UsePdfaAuditRecords);
            Assert.AreEqual("ca", enterpriseSettings.InternationalDialingPlan);
            Assert.IsInstanceOfType(enterpriseSettings.ExtensionFilter, typeof(Helpers.ExtensionFilter));
            Assert.AreEqual(false, enterpriseSettings.VirusScanEnabled);
            Assert.IsNull(enterpriseSettings.MaxFileSizeValue);
            Assert.IsNull(enterpriseSettings.MaxFileSizeUnit);
            Assert.AreEqual(true, enterpriseSettings.IncludeUsersInAutocomplete);
            Assert.AreEqual(true, enterpriseSettings.IncludeFavoritesInAutocomplete);
            Assert.AreEqual(true, enterpriseSettings.UsersPublicUrl);
        }
    }
}
