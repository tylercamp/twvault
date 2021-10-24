using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Lib.Scaffold;

namespace TW.Vault.Lib.Features.CommandClassification.FakeDetectionRules
{
    //  This might be unused since it's very strict
    public class NoTravelingTroopsRule : IFakeDetectionRule
    {
        public FakeClassification Classify(ClassificationContext context)
        {
            var traveling = context.SourceVillage.ArmyTraveling;
            return traveling == null || ArmyIsOld(traveling, context.CurrentTime)
                ? FakeClassification.Unknown | FakeClassification.Definite
                : FakeClassification.None;
        }

        bool ArmyIsOld(CurrentArmy army, DateTime serverTime) => (serverTime - army.LastUpdated.Value).TotalDays >= 1;
    }
}
