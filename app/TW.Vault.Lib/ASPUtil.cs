using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault
{
    public class ASPUtil
    {
        public IWebHostEnvironment HostingEnvironment { get; private set; }
        String basePath;

        public ASPUtil(IWebHostEnvironment hostingEnvironment, String basePath = "")
        {
            this.HostingEnvironment = hostingEnvironment;
            this.basePath = basePath;
        }

        public bool UseProductionScripts => HostingEnvironment.IsProduction() || Configuration.Security.ForceEnableObfuscatedScripts;
        public String ObfuscationPathRoot => Path.Combine(HostingEnvironment.ContentRootPath, Configuration.Initialization.ScriptCompilationOutputPath);

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

        public String GetObfuscatedPath(String fileName) => Path.Combine(ObfuscationPathRoot, fileName);
    }
}
