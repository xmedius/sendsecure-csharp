using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace XMedius.SendSecure.Helpers
{
    [JsonObject(Title = "favorite")]
    public class Favorite : IContact
    {
        public class Creation : Creation<Favorite>
        {
            public override string EntryToRemove
            {
                get { return "order_number"; }
            }
        }

        public class Edition : Edition<Favorite>
        {}

        public class Destruction : Destruction<Favorite>
        {
            public override List<ContactMethod> ObjectContactMethods
            {
                get { return ContactObject.ContactMethods; }
            }
        }

        public Favorite(string email, string firstName = null, string lastName = null, string companyName = null)
        {
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            CompanyName = companyName;
            ContactMethods = new List<ContactMethod>();
        }

        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }
        [JsonProperty(PropertyName = "email", Required = Required.Always)]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "company_name")]
        public string CompanyName { get; set; }
        [JsonProperty(PropertyName = "order_number")]
        public int? OrderNumber { get; set; }
        [JsonProperty(PropertyName = "contact_methods")]
        public List<ContactMethod> ContactMethods { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime? UpdatedAt { get; set; }


        public T UpdateFor<T>() where T : ContactInteraction<Favorite>, new()
        {
            var t = new T()
            {
                ContactObject = this
            };

            t.JObject = new Lazy<JObject>(() =>
            {
                JObject jFavorite = JObject.FromObject(new { favorite = t.ContactObject }, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });

                jFavorite["favorite"].RemoveEntries(new List<string> { "id", "created_at", "updated_at" })
                         ["contact_methods"].RemoveEntries(new List<string> { "verified", "created_at", "updated_at" });
                return jFavorite;
            });

            return t;
        }

        public static Favorite FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Favorite>(json);
        }

        public void Update(string json)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };

            JsonConvert.PopulateObject(json, this, jsonSerializerSettings);
        }
    }
}
