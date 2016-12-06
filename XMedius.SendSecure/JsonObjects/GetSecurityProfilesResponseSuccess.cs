using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.JsonObjects
{
    internal class GetSecurityProfilesResponseSuccess
    {
        [JsonProperty(PropertyName = "security_profiles", Required = Required.AllowNull)]
        public List<Helpers.SecurityProfile> SecurityProfiles;
    }
}
