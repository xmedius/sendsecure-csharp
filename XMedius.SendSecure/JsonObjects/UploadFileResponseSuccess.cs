using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.JsonObjects
{
    internal class UploadFileResponseSuccess
    {
        internal class TemporaryDocument
        {
            public string document_guid { get; set; }
        }

        public TemporaryDocument temporary_document { get; set; }
    }
}
