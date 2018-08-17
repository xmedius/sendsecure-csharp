using Newtonsoft.Json;

namespace XMedius.SendSecure.JsonObjects
{
    public class UserToken
    {
        public bool Result { get; set; }
        [JsonProperty(PropertyName = "user_id")]
        public int? UserId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Token { get; set; }
    }
}
