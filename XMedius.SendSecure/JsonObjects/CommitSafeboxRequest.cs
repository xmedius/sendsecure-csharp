using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.JsonObjects
{
    internal class CommitSafeboxRequest
    {
        internal class SafeBox
        {
            public string guid { get; set; }

            public List<Helpers.Recipient> recipients { get; set; }

            public string subject { get; set; }
            public string message { get; set; }

            public List<string> document_ids { get; set; }

            public int security_profile_id { get; set; }

            public bool reply_enabled { get; set; }
            public bool group_replies { get; set; }
            public int expiration_value { get; set; }
            public Helpers.SecurityProfile.TimeUnit expiration_unit { get; set; }
            public Helpers.SecurityProfile.RetentionPeriod retention_period_type { get; set; }
            public int? retention_period_value { get; set; }
            public Helpers.SecurityProfile.TimeUnit? retention_period_unit { get; set; }
            public bool encrypt_message { get; set; }
            public bool double_encryption { get; set; }

            public string public_encryption_key { get; set; }

            public string notification_language { get; set; }
        }

        public SafeBox safebox { get; set; }
    }
}
