using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TW.Vault.App
{
    public static class ScriptObfuscation
    {
        private static ILogger logger = Log.ForContext(typeof(ScriptObfuscation));

        private static String[] defaultObfuscationParams = new[]
        {
            "--compact true",
            "--transform-object-keys true",
            "--string-array-threshold 1",
            "--string-array-encoding 'rc4'",
            "--self-defending true",
            "--dead-code-injection-threshold 0.2",
            "--dead-code-injection true",
            "--control-flow-flattening true",
            "--numbers-to-expressions true",
        };

        private static String ShellName => Environment.OSVersion.Platform == PlatformID.Win32NT ? "cmd" : "/bin/bash";
        private static String ToShellParams(String command) => (Environment.OSVersion.Platform == PlatformID.Win32NT ? "/C" : "-c") + $" \"{command}\"";

        public static bool Run(String inputFile, String targetFile)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = ShellName;
            psi.Arguments = ToShellParams($"javascript-obfuscator {inputFile} -o {targetFile} {Configuration.Instance.GetSection("Security")["ObfuscatorParams"] ?? String.Join(" ", defaultObfuscationParams)}");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            var process = new Process();
            process.StartInfo = psi;

            List<String> stdout = new List<string>();
            process.OutputDataReceived += (sender, e) => { lock (stdout) { stdout.Add(e.Data); } };
            process.ErrorDataReceived += (sender, e) => { lock (stdout) { stdout.Add(e.Data); } };

            if (!process.Start())
            {
                logger.Warning("Failed to execute obfuscation command: {shellName} {params}", ShellName, psi.Arguments);
                return false;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            while (!process.HasExited)
                Thread.Sleep(100);

            logger.Information("Finished running javascript-obfuscator:\n{logs}", String.Join("", stdout));

            if (process.ExitCode != 0)
            {
                logger.Warning("javascript-obfuscator failed with exit code {exitCode}", process.ExitCode);
                logger.Warning("Ran shell with params: {params}", psi.Arguments);
                logger.Warning("Process output:\n{logLines}", String.Join("\n", stdout));
                return false;
            }

            logger.Information("javascript-obfuscator completed successfully");

            return true;
        }
    }
}
