using Newtonsoft.Json;
using System.Collections.Generic;

namespace XMedius.SendSecure.Helpers
{
    public class Reply
    {
        public Reply(string message)
        {
            Message = message;
            Attachments = new List<Attachment>();
            DocumentIds = new List<string>();
        }

        [JsonProperty(PropertyName = "consent")]
        public bool? Consent { get; set; } //Optional 
        [JsonProperty(PropertyName = "document_ids")]
        public List<string> DocumentIds { get; set; }
        [JsonIgnore]
        public List<Attachment> Attachments { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        public string ToJson()
        {
            dynamic replyWrapper = new
            {
                safebox = this
            };

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,

            };

            return JsonConvert.SerializeObject(replyWrapper, Formatting.None, settings);
        }
    }
}
