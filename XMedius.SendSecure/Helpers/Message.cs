using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace XMedius.SendSecure.Helpers
{
    public class Message
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "note")]
        public string Note { get; set; }
        [JsonProperty(PropertyName = "note_size")]
        public int? NoteSize { get; set; }
        [JsonProperty(PropertyName = "read")]
        public bool? Read { get; set; }
        [JsonProperty(PropertyName = "author_id")]
        public int AuthorId { get; set; }
        [JsonProperty(PropertyName = "author_type")]
        public string AuthorType { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty(PropertyName = "documents")]
        public List<MessageDocument> Documents { get; set; }
    }
}
