using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FshareAPI
{
    public class FshareFileOrFolderInfo
    {
        public string Id { get; set;}
        public string LinkCode { get; set;}
        public string Name { get; set;}
        public string Secure { get; set;}
        public string DirectLink { get; set;}
        public string FileOrFolderType { get; set;}
        public string Path { get; set;}
        public long Size { get; set;}
        public long DownloadCount { get; set;}
        public string MimeType { get; set;}
        public string Created { get; set;}
        public string PWD { get; set;}
        public string AllowFollow { get; set; }
        public long NumFollower { get; set; }
    }
}
