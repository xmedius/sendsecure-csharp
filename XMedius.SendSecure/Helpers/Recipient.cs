using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.Helpers
{
    public class Recipient
    {
        public Recipient(string email)
        {
            Email = email;
            ContactMethods = new List<ContactMethod>();
        }

        [JsonProperty(PropertyName = "email", Required = Required.Always)]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "company_name")]
        public string CompanyName { get; set; }

        [JsonProperty(PropertyName = "contact_methods")]
        public List<ContactMethod> ContactMethods { get; set; }
    }
}
