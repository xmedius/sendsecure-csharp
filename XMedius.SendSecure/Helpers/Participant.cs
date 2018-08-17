using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace XMedius.SendSecure.Helpers
{
    [JsonConverter(typeof(JsonObjects.Serializers.ParticipantSerializer))]
    public class Participant : IContact
    {
        public class Creation : Creation<Participant>
        {
            public override string EntryToRemove
            {
                get { return "locked"; }
            }
        }

        public class Edition : Edition<Participant>
        {}

        public class Destruction : Destruction<Participant>
        {
            public override List<ContactMethod> ObjectContactMethods
            {
                get { return ContactObject.GuestOptions.ContactMethods; }
            }
        }

        public Participant(string email, string firstName = null, string lastName = null, string companyName = null)
        {
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            GuestOptions = new GuestOptions
            {
                ContactMethods = new List<ContactMethod>(),
                CompanyName = companyName
            };
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }
        [JsonProperty(PropertyName = "guest_options")]
        public GuestOptions GuestOptions { get; set; }
        [JsonProperty(PropertyName = "message_read_count")]
        public int MessageReadCount { get; set; }
        [JsonProperty(PropertyName = "message_total_count")]
        public int MessageTotalCount { get; set; }

        public T UpdateFor<T>() where T : ContactInteraction<Participant>, new()
        {
            var t = new T()
            {
                ContactObject = this
            };

            t.JObject = new Lazy<JObject>(() =>
            {
                JObject jParticipant = JObject.FromObject(new { participant = t.ContactObject });

                jParticipant["participant"].RemoveEntries(new List<string> { "id", "type", "role", "guest_options", "bounced_email",
                                                                             "failed_login_attempts", "verified", "created_at",
                                                                             "updated_at", "message_read_count", "message_total_count" })
                            ["contact_methods"].RemoveEntries(new List<string> { "verified", "created_at", "updated_at" });
                return jParticipant;
            });

            return t;
        }

        public static Participant FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Participant>(json);
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
