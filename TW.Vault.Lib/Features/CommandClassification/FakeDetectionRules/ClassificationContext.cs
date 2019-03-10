using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Scaffold;

namespace TW.Vault.Features.CommandClassification.FakeDetectionRules
{
    public class ClassificationContext
    {
        public DateTime CurrentTime;

        public CurrentVillage SourceVillage;
        public List<Command> SentFromSource;
        public List<Command> ReturningToSource;
    }
}
