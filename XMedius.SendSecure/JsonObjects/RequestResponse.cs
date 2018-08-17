using Newtonsoft.Json;

namespace XMedius.SendSecure.JsonObjects
{
    public class RequestResponse
    {
        public bool? Result { get; set; }
        public string Message { get; set; }

        public static RequestResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<RequestResponse>(json);
        }
    }
}
