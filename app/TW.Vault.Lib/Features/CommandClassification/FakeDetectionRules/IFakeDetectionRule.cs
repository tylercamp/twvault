using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Lib.Features.CommandClassification.FakeDetectionRules
{
    interface IFakeDetectionRule
    {
        FakeClassification Classify(ClassificationContext context);
    }
}
