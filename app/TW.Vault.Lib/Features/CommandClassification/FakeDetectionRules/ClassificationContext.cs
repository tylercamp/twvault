using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Lib.Scaffold;

namespace TW.Vault.Lib.Features.CommandClassification.FakeDetectionRules
{
    public class ClassificationContext
    {
        public DateTime CurrentTime;

        public CurrentVillage SourceVillage;
        public List<Command> SentFromSource;
        public List<Command> ReturningToSource;
    }
}
