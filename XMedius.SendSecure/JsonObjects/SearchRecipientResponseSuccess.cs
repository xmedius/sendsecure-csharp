using Newtonsoft.Json;
using System.Collections.Generic;

namespace XMedius.SendSecure.JsonObjects
{
    public class Recipient
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int? Id { get; set; }
        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "first_name", Required = Required.AllowNull)]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "last_name", Required = Required.AllowNull)]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "email", Required = Required.Always)]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "company_name", Required = Required.AllowNull)]
        public string CompanyName { get; set; }
    }

    public class SearchRecipientResponseSuccess
    {
        [JsonProperty(PropertyName = "results", Required = Required.AllowNull)]
        public List<Recipient> Results { get; set; }

        public static SearchRecipientResponseSuccess FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SearchRecipientResponseSuccess>(json);
        }
    }
}
