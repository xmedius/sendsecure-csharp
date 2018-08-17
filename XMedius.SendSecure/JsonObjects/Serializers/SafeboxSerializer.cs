using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace XMedius.SendSecure.JsonObjects.Serializers
{
    public class SafeboxSerializer : JsonConverter
    {
        static int readCount = 1;
        int ReadCount { get { return readCount; } set { readCount = value; } }
        static int writeCount = 1;
        int WriteCount { get { return writeCount; } set { writeCount = value; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            WriteCount++;

            JsonSerializer s = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            JObject jo = JObject.FromObject(value, s);
            Helpers.Safebox safebox = (Helpers.Safebox)value;

            jo.Add("recipients", jo["participants"]);
            jo.Merge(JObject.FromObject(safebox.SecurityOptions, s));

            jo.Remove("participants");
            JArray documentIds = new JArray();
            foreach (Helpers.Attachment attachment in safebox.Attachments)
            {
                documentIds.Add(attachment.Guid);
            }
            jo.Add("document_ids", documentIds);

            jo.WriteTo(writer);

            WriteCount--;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            ReadCount++;

            Object o = serializer.Deserialize(reader, objectType);

            ReadCount--;

            return o;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Helpers.Safebox);
        }

        public override bool CanWrite { get { return WriteCount == 1; } }

        public override bool CanRead { get { return ReadCount == 1; } }
    }
}
