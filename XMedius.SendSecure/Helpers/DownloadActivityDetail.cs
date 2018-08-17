using Newtonsoft.Json;
using System.Collections.Generic;

namespace XMedius.SendSecure.Helpers
{
    public class DownloadActivityDetail
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "documents")]
        public List<DownloadActivityDocument> Documents { get; set; }
    }
}
