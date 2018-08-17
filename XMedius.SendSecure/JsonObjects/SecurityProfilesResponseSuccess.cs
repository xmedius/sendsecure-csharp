using Newtonsoft.Json;
using System.Collections.Generic;

namespace XMedius.SendSecure.JsonObjects
{
    internal class SecurityProfilesResponseSuccess
    {
        [JsonProperty(PropertyName = "security_profiles", Required = Required.AllowNull)]
        public List<Helpers.SecurityProfile> SecurityProfiles { get; set; }
    }
}
