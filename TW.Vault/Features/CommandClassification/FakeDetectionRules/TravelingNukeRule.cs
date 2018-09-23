using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Features.CommandClassification.FakeDetectionRules
{
    public class TravelingNukeRule : IFakeDetectionRule
    {
        public FakeClassification Classify(ClassificationContext context)
        {
            var village = context.SourceVillage;
            if (village.ArmyTraveling == null || !village.ArmyTraveling.IsRecent(context.CurrentTime))
                return FakeClassification.None;

            return village.ArmyTraveling.IsOffensive()
                ? FakeClassification.Nuke | FakeClassification.Possible
                : FakeClassification.Fake | FakeClassification.Possible;
        }
    }
}
