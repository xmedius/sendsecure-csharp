using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.Helpers
{
    public class Safebox
    {
        public Safebox(string userEmail)
        {
            UserEmail = userEmail;
            Attachments = new List<Attachment>();
            Recipients = new List<Recipient>();
        }

        public string UserEmail { get; set; }

        public string Guid { get; set; }
        public string PublicEncryptionKey { get; set; }
        public string UploadUrl { get; set; }

        public string Subject { get; set; }
        public string Message { get; set; }

        public SecurityProfile SecurityProfile { get; set; }

        public List<Recipient> Recipients { get; }

        public List<Attachment> Attachments { get; }

    }
}
