using Newtonsoft.Json;
using System.Collections.Generic;

namespace XMedius.SendSecure.Helpers
{
    public class DownloadActivity
    {
        [JsonProperty(PropertyName = "guests")]
        public List<DownloadActivityDetail> Guests { get; set; }
        [JsonProperty(PropertyName = "owner")]
        public DownloadActivityDetail Owner { get; set; }
    }
}
