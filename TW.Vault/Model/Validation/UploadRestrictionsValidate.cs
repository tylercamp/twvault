using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.Validation
{
    public static class UploadRestrictionsValidate
    {
        public struct ValidateInfo
        {
            public Scaffold.UserUploadHistory History;

            public TimeSpan MaxCommandsAge;
            public TimeSpan MaxIncomingsAge;
            public TimeSpan MaxReportsAge;
            public TimeSpan MaxTroopsAge;

            public static ValidateInfo FromMapRestrictions(Scaffold.UserUploadHistory history)
            {
                return new ValidateInfo
                {
                    History = history,
                    MaxCommandsAge = TimeSpan.FromDays(Configuration.Behavior.Map.MaxDaysSinceCommandUpload),
                    MaxIncomingsAge = TimeSpan.FromDays(Configuration.Behavior.Map.MaxDaysSinceIncomingsUpload),
                    MaxReportsAge = TimeSpan.FromDays(Configuration.Behavior.Map.MaxDaysSinceReportUpload),
                    MaxTroopsAge = TimeSpan.FromDays(Configuration.Behavior.Map.MaxDaysSinceTroopUpload)
                };
            }

            public static ValidateInfo FromTaggingRestrictions(Scaffold.UserUploadHistory history)
            {
                return new ValidateInfo
                {
                    History = history,
                    MaxCommandsAge = TimeSpan.FromDays(Configuration.Behavior.Tagging.MaxDaysSinceCommandUpload),
                    MaxIncomingsAge = TimeSpan.FromDays(Configuration.Behavior.Tagging.MaxDaysSinceIncomingsUpload),
                    MaxReportsAge = TimeSpan.FromDays(Configuration.Behavior.Tagging.MaxDaysSinceReportUpload),
                    MaxTroopsAge = TimeSpan.FromDays(Configuration.Behavior.Tagging.MaxDaysSinceTroopUpload)
                };
            }
        }

        public static List<String> GetNeedsUpdateReasons(DateTime currentTime, ValidateInfo info)
        {
            var now = DateTime.UtcNow;

            var history = info.History;

            List<String> reasons = new List<String>();
            if (history?.LastUploadedCommandsAt == null ||
                (now - history.LastUploadedCommandsAt.Value > info.MaxCommandsAge))
            {
                reasons.Add("commands");
            }

            if (history?.LastUploadedIncomingsAt == null ||
                (now - history.LastUploadedIncomingsAt.Value > info.MaxIncomingsAge))
            {
                reasons.Add("incomings");
            }

            if (history?.LastUploadedReportsAt == null ||
                (now - history.LastUploadedReportsAt.Value > info.MaxReportsAge))
            {
                reasons.Add("reports");
            }

            if (history?.LastUploadedTroopsAt == null ||
                (now - history.LastUploadedTroopsAt.Value > info.MaxTroopsAge))
            {
                reasons.Add("troops");
            }

            return reasons;
        }
    }
}
