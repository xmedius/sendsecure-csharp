using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace XMedius.SendSecure.Helpers
{
    public class ContactMethod
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum DestinationTypeT
        {
            [EnumMember(Value = "home_phone")] HomePhone,
            [EnumMember(Value = "cell_phone")] CellPhone,
            [EnumMember(Value = "office_phone")] OfficePhone,
            [EnumMember(Value = "other_phone")] OtherPhone
        };

        [JsonProperty(PropertyName = "destination_type", Required = Required.Always)]
        public DestinationTypeT? DestinationType { get; set; }
        [JsonProperty(PropertyName = "destination", Required = Required.Always)]
        public string Destination { get; set; }
        [JsonProperty(PropertyName = "verified")]
        public bool? Verified { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime? UpdatedAt { get; set; }
        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }
        [JsonProperty(PropertyName = "_destroy")]
        public bool? DestroyContact { get; set; }
    }
}
