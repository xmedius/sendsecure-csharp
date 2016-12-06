using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
        public DestinationTypeT DestinationType { get; set; }
        [JsonProperty(PropertyName = "destination", Required = Required.Always)]
        public string Destination { get; set; }
    }
}
