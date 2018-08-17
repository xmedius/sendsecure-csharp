using System;
using Newtonsoft.Json;

namespace XMedius.SendSecure.JsonObjects
{
    public class AddTimeResponseSuccess
    {
        [JsonProperty(PropertyName = "result", Required = Required.Always)]
        public bool Result { get; set; }
        [JsonProperty(PropertyName = "message", Required = Required.Always)]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "new_expiration", Required = Required.Always)]
        public DateTime NewExpiration { get; set; }
    }
}
