using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMedius.SendSecure.Helpers
{
    public class Attachment
    {
        public string Guid { get; set; }

        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }

        internal Stream Stream;

        public Attachment(string path, string content_type = "application/octet-stream")
        {
            Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            var fileInfo = new FileInfo(path);
            FileName = fileInfo.Name;
            ContentType = content_type;
            Size = fileInfo.Length;
        }

        public Attachment(Stream stream, string filename, long size, string content_type = "application/octet-stream")
        {
            Stream = stream;
            FileName = filename;
            ContentType = content_type;
            Size = size;
        }
    }
}
