using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.Helpers
{
    public class SafeboxResponse
    {
        [JsonProperty(PropertyName = "guid", Required = Required.Always)]
        public string Guid { get; set; }
        [JsonProperty(PropertyName = "preview_url", Required = Required.Always)]
        public string PreviewUrl { get; set; }
        [JsonProperty(PropertyName = "encryption_key", Required = Required.AllowNull)]
        public string EncryptionKey { get; set; }
    }
}
