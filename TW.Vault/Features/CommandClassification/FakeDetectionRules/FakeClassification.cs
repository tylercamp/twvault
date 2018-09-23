using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Features.CommandClassification.FakeDetectionRules
{
    [Flags]
    public enum FakeClassification
    {
        None = 0,

        Fake     = 0b0000_0001,
        Nuke     = 0b0000_0010,
        Unknown  = 0b0000_0100,

        Definite = 0b0001_0000,
        Possible = 0b0010_0000,

        TypeMask       = 0b0000_1111, // Used to get the classification type flag
        ConfidenceMask = 0b1111_0000  // Used to get the confidence flag
    }
}
