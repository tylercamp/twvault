using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TW.Vault.Features
{
    public class ScriptCompiler
    {
        private static Regex RequireRegex = new Regex(@"^([ \t]*).*\/\/\#\s*REQUIRE\s+([^\n\r]+)", RegexOptions.Multiline | RegexOptions.Compiled);

        public delegate String ResolveDependencyDelegate(String fileName);
        public delegate void CircularDependencyDelegate(IEnumerable<String> dependencyChain);

        public ResolveDependencyDelegate DependencyResolver;
        public event CircularDependencyDelegate OnCircularDependency;

        public String Compile(String scriptName) => CompileWithDependencies(scriptName, new List<String>(), new List<string>());

        private String CompileWithDependencies(String scriptName, List<String> resolvedDependencies, List<String> workingDependencies)
        {
            String scriptContents = DependencyResolver(scriptName);
            if (scriptContents == null)
                return null;

            bool canContinue = true;

            String compilationResult = RequireRegex.Replace(scriptContents, match =>
            {
                if (!canContinue)
                    return "";

                var spacing = match.Groups[1].Value;
                var dependentFileName = match.Groups[2].Value;
                if (resolvedDependencies.Contains(dependentFileName))
                    return "";

                if (workingDependencies.Contains(dependentFileName))
                {
                    OnCircularDependency?.Invoke(workingDependencies.Concat(new[] { dependentFileName }).ToArray());
                    canContinue = false;
                    return "";
                }

                workingDependencies.Add(dependentFileName);
                String compiledDependency = CompileWithDependencies(dependentFileName, resolvedDependencies, workingDependencies);
                workingDependencies.Remove(dependentFileName);

                if (compiledDependency == null)
                {
                    canContinue = false;
                    return "";
                }

                resolvedDependencies.Add(dependentFileName);

                var scriptLines = compiledDependency.Split('\n');
                var formattedDependency = new StringBuilder();

                foreach (String line in scriptLines)
                {
                    formattedDependency.Append(spacing);
                    formattedDependency.AppendLine(line.Trim('\r', '\n'));
                }

                return formattedDependency.ToString();
            });

            return compilationResult;
        }
    }
}
