using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Scaffold;

namespace TW.Vault.Features.CommandClassification.FakeDetectionRules
{
    public class DefensiveVillageRule : IFakeDetectionRule
    {
        public FakeClassification Classify(ClassificationContext context)
        {
            var village = context.SourceVillage;
            if (village.ArmyStationed == null && village.ArmyOwned == null)
                return FakeClassification.None;

            var isDefensiveNullable = IsDefensiveVillage(context.CurrentTime, village.ArmyStationed, village.ArmyOwned);
            if (isDefensiveNullable == null)
                return FakeClassification.None;

            return FakeClassification.Possible | (
                isDefensiveNullable.Value
                    ? FakeClassification.Fake
                    : FakeClassification.Nuke
            );
        }

        bool? IsDefensiveVillage(DateTime currentTime, CurrentArmy stationedArmy, CurrentArmy ownedArmy)
        {
            if (ownedArmy != null && ownedArmy.IsRecent(currentTime) && !ownedArmy.IsOffensive())
                return true;

            if (stationedArmy != null && ownedArmy.IsRecent(currentTime) && !stationedArmy.IsOffensive())
                return true;

            return null;
        }
    }
}
