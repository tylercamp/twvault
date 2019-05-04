using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault
{
    public class ASPUtil
    {
        public IHostingEnvironment HostingEnvironment { get; private set; }
        String basePath;

        public ASPUtil(IHostingEnvironment hostingEnvironment, String basePath = "")
        {
            this.HostingEnvironment = hostingEnvironment;
            this.basePath = basePath;
        }

        public String GetFilePath(String relativePath)
        {
            if (String.IsNullOrWhiteSpace(relativePath))
                return null;

            String rootPath = Path.Combine(HostingEnvironment.WebRootPath, basePath);

            String fullPath = Path.Combine(rootPath, relativePath);
            String absolutePath = Path.GetFullPath(fullPath);

            //  Prevent directory traversal
            if (!absolutePath.StartsWith(rootPath))
                return null;

            if (File.Exists(absolutePath))
                return absolutePath;
            else
                return absolutePath;
        }
    }
}
