using Newtonsoft.Json;
using System;

namespace XMedius.SendSecure.Helpers
{
    public class DownloadActivityDocument
    {
        [JsonProperty(PropertyName = "downloaded_bytes")]
        public ulong? DownloadBytes { get; set; }
        [JsonProperty(PropertyName = "downloaded_date")]
        public DateTime? DownloadDate { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
