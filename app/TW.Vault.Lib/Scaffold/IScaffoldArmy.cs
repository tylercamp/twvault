using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Lib.Scaffold
{
    public interface IScaffoldArmy
    {
        int? Spear { get; set; }
        int? Sword { get; set; }
        int? Axe { get; set; }
        int? Archer { get; set; }
        int? Spy { get; set; }
        int? Light { get; set; }
        int? Marcher { get; set; }
        int? Heavy { get; set; }
        int? Ram { get; set; }
        int? Catapult { get; set; }
        int? Knight { get; set; }
        int? Snob { get; set; }
        int? Militia { get; set; }
    }

    public static class ScaffoldArmyExtensions
    {
        public static String ScaffoldToString(this IScaffoldArmy army)
        {
            var parts = new List<String>();
            var zeroParts = new List<String>();
            var totalCount = 0;

            void AddPart(String label, int? count)
            {
                if (count != null)
                {
                    if (count.Value == 0) zeroParts.Add(label);
                    else
                    {
                        totalCount += count.Value;
                        parts.Add($"{count} {label}");
                    }
                }
            }

            AddPart("spear", army.Spear);
            AddPart("sword", army.Sword);
            AddPart("axe", army.Axe);
            AddPart("archer", army.Archer);
            AddPart("spy", army.Spy);
            AddPart("light", army.Light);
            AddPart("marcher", army.Marcher);
            AddPart("heavy", army.Heavy);
            AddPart("ram", army.Ram);
            AddPart("catapult", army.Catapult);
            AddPart("knight", army.Knight);
            AddPart("noble", army.Snob);
            AddPart("militia", army.Militia);

            if (parts.Count == 0 && zeroParts.Count == 0)
            {
                return "<All-Null Scaffold Army>";
            }
            else
            {
                String filledSummary = null;
                String zeroSummary = null;
                if (parts.Count > 0) filledSummary = $"{totalCount} total: {String.Join(", ", parts)}";
                if (zeroParts.Count > 0) zeroSummary = $"0 {String.Join("/", zeroParts)}";

                if (filledSummary != null && zeroSummary != null)
                {
                    return $"{filledSummary}; {zeroSummary}";
                }
                else return filledSummary ?? zeroSummary;
            }
        }
    }
}
