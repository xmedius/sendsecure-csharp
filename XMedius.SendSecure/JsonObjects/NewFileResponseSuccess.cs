using Newtonsoft.Json;

namespace XMedius.SendSecure.JsonObjects
{
    public class NewFileResponseSuccess
    {
        [JsonProperty(PropertyName = "temporary_document_guid", Required = Required.Always)]
        public string TemporaryDocumentGuid { get; set; }
        [JsonProperty(PropertyName = "upload_url", Required = Required.Always)]
        public string UploadUrl { get; set; }

        public static NewFileResponseSuccess FromJson(string json)
        {
            return JsonConvert.DeserializeObject<NewFileResponseSuccess>(json);
        }
    }
}
