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
            public Scaffold.User User;
            public Scaffold.UserUploadHistory History;

            public TimeSpan MaxCommandsAge;
            public TimeSpan MaxIncomingsAge;
            public TimeSpan MaxReportsAge;
            public TimeSpan MaxTroopsAge;

            public static ValidateInfo FromMapRestrictions(Scaffold.User user, Scaffold.UserUploadHistory history)
            {
                return new ValidateInfo
                {
                    User = user,
                    History = history,
                    MaxCommandsAge = TimeSpan.FromDays(Configuration.Behavior.Map.MaxDaysSinceCommandUpload),
                    MaxIncomingsAge = TimeSpan.FromDays(Configuration.Behavior.Map.MaxDaysSinceIncomingsUpload),
                    MaxReportsAge = TimeSpan.FromDays(Configuration.Behavior.Map.MaxDaysSinceReportUpload),
                    MaxTroopsAge = TimeSpan.FromDays(Configuration.Behavior.Map.MaxDaysSinceTroopUpload)
                };
            }

            public static ValidateInfo FromTaggingRestrictions(Scaffold.User user, Scaffold.UserUploadHistory history)
            {
                return new ValidateInfo
                {
                    User = user,
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

            if (info.User.IsReadOnly)
                return reasons;

            if (history?.LastUploadedCommandsAt == null ||
                (now - history.LastUploadedCommandsAt.Value > info.MaxCommandsAge))
            {
                reasons.Add("UPLOAD_COMMANDS_REQUIRED");
            }

            if (history?.LastUploadedIncomingsAt == null ||
                (now - history.LastUploadedIncomingsAt.Value > info.MaxIncomingsAge))
            {
                reasons.Add("UPLOAD_INCOMINGS_REQUIRED");
            }

            if (history?.LastUploadedReportsAt == null ||
                (now - history.LastUploadedReportsAt.Value > info.MaxReportsAge))
            {
                reasons.Add("UPLOAD_REPORTS_REQUIRED");
            }

            if (history?.LastUploadedTroopsAt == null ||
                (now - history.LastUploadedTroopsAt.Value > info.MaxTroopsAge))
            {
                reasons.Add("UPLOAD_TROOPS_REQUIRED");
            }

            return reasons;
        }
    }
}
