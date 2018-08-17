using Newtonsoft.Json;

namespace XMedius.SendSecure.JsonObjects
{
    internal class GetTokenResponseError
    {
        public bool Result { get; set; }
        public string Message { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int Code { get; set; }
    }
}
