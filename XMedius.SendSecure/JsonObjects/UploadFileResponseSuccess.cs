using Newtonsoft.Json;

namespace XMedius.SendSecure.JsonObjects
{
    internal class TemporaryDocument
    {
        [JsonProperty(PropertyName = "document_guid", Required = Required.Always)]
        public string DocumentGuid { get; set; }
    }

    internal class UploadFileResponseSuccess
    {
        [JsonProperty(PropertyName = "temporary_document", Required = Required.Always)]
        public TemporaryDocument TemporaryDocument { get; set; }
    }
}
