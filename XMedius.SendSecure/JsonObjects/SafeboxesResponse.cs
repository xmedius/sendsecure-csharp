using Newtonsoft.Json;
using System.Collections.Generic;

namespace XMedius.SendSecure.JsonObjects
{
    public class SafeboxesResponse
    {
        public int? Count { get; set; }
        public string Previous_page_url { get; set; }
        public string Next_page_url { get; set; }
        public List<Helpers.Safebox> Safeboxes { get; set; }

        public static SafeboxesResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SafeboxesResponse>(json);
        }
    }
}
