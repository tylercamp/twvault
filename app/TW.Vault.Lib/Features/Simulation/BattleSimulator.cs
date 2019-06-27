using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model.Convert;
using TW.Vault.Model.JSON;
using TW.Vault.Model.Native;

namespace TW.Vault.Features.Simulation
{
    // https://forum.tribalwars.net/index.php?threads/battle-formula.183466/
    // https://forum.tribalwars.net/index.php?threads/defence-calculation.132254/
    // https://forum.tribalwars.net/index.php?threads/battle-mechanics.235359/
    // https://forum.tribalwars.net/index.php?threads/guide-rams.100537/

    // TODO - Wall levels! Wall buffs are considered, but ram effects on wall buffs are not

    public class BattleSimulator
    {
        private static Army DefaultNukeArmy { get; } = new Army
        {
            { TroopType.Axe,        6900 },
            { TroopType.Light,      3000 },
            { TroopType.Ram,        220 },
            { TroopType.Catapult,   75 }
        };

        private static bool IsArmyEmpty(Army army) => army == null || army.Count == 0 || army.Values.All(c => c == 0);

        private static int MaxSimulationIterations = 75;

        public static int TotalAttackPower(Army attackingArmy) => attackingArmy.Count == 0 ? 0 : attackingArmy.Sum(kvp => kvp.Value * ArmyStats.AttackPower[kvp.Key]);
        public static int TotalAttackPower(Army attackingArmy, UnitType unitType)
        {
            var troopsOfType = attackingArmy.Where(kvp => ArmyStats.UnitType[kvp.Key] == unitType).ToList();
            if (troopsOfType.Count == 0)
                return 0;

            return troopsOfType.Sum(kvp => ArmyStats.AttackPower[kvp.Key] * kvp.Value);
        }

        public static UnitPower AttackPower(Army attackingArmy)
        {
            UnitPower result = new UnitPower();
            foreach (var kvp in attackingArmy)
            {
                switch (ArmyStats.UnitType[kvp.Key])
                {
                    case UnitType.Infantry: result.Infantry += ArmyStats.AttackPower[kvp.Key] * kvp.Value; break;
                    case UnitType.Cavalry: result.Cavalry += ArmyStats.AttackPower[kvp.Key] * kvp.Value; break;
                    case UnitType.Archer: result.Archer += ArmyStats.AttackPower[kvp.Key] * kvp.Value; break;
                }
            }
            return result;
        }

        public static int TotalDefensePower(Army defendingArmy) => defendingArmy?.Count > 0 ? defendingArmy.Sum(kvp => kvp.Value * ArmyStats.DefensePower[kvp.Key].Total) : 0;

        public static int TotalDefensePower(Army defendingArmy, UnitType unitType)
        {
            if (defendingArmy == null || defendingArmy.Count == 0)
                return 0;

            int totalDefense = 0;

            foreach (var kvp in defendingArmy)
            {
                switch (unitType)
                {
                    case UnitType.Infantry: totalDefense += kvp.Value * ArmyStats.DefensePower[kvp.Key].Infantry; break;
                    case UnitType.Cavalry: totalDefense += kvp.Value * ArmyStats.DefensePower[kvp.Key].Cavalry; break;
                    case UnitType.Archer: totalDefense += kvp.Value * ArmyStats.DefensePower[kvp.Key].Archer; break;
                }
            }

            return totalDefense;
        }

        public static UnitPower DefensePower(Army defendingArmy)
        {
            UnitPower result = new UnitPower();
            if (defendingArmy == null)
                return result;

            foreach (var kvp in defendingArmy)
            {
                result.Infantry += ArmyStats.DefensePower[kvp.Key].Infantry * kvp.Value;
                result.Cavalry += ArmyStats.DefensePower[kvp.Key].Cavalry * kvp.Value;
                result.Archer += ArmyStats.DefensePower[kvp.Key].Archer * kvp.Value;
            }

            return result;
        }

        public static bool DoRamsLowerWallPreemptively(int numRams, int wallLevel)
        {
            int lowerLevelAmount = LowerWallPreemptiveLevelAmount(numRams, wallLevel);
            int ramsNeeded = (int)Math.Round(2 * Math.Pow(1.09, wallLevel) + 4 * Math.Pow(1.09, wallLevel) * (lowerLevelAmount - 1) + 0.5);

            return ramsNeeded <= numRams;
        }

        public static int LowerWallPreemptiveLevelAmount(int numRams, int wallLevel)
        {
            return (int)Math.Clamp(Math.Round(
                    numRams * (-0.5 - 2 * Math.Pow(1.09, wallLevel)) / (4 * Math.Pow(1.09, wallLevel)) + 1
                ), wallLevel / 2, wallLevel);
        }

        public static int WallLevelAfterDowngradeAttackerWon(int ramsRemaining, int ramsLost, int wallLevel)
        {
            int levelsLost = (int)Math.Round(((ramsRemaining + 1 - ramsLost / 2.0) - 1 * Math.Pow(1.09, wallLevel)) / (2 * Math.Pow(1.09, wallLevel)) + 1);

            return Math.Max(0, wallLevel - levelsLost);
        }

        public static int WallLevelAfterDowngradeAttackerLost(int numRams, int wallLevel, float defenderLossRatio)
        {
            int effectiveRams = (int)(Math.Round(numRams * defenderLossRatio));

            int levelsDowngraded = (int)Math.Clamp(Math.Round((effectiveRams - 2 * Math.Pow(1.09, wallLevel)) / (4 * Math.Pow(1.09, wallLevel)) + 1), 0, wallLevel);
            if (levelsDowngraded == 0)
                return wallLevel;

            int ramsRequired = (int)Math.Round(Math.Max(0, (2 * Math.Pow(1.09, wallLevel) + 4 * Math.Pow(1.09, wallLevel) * (levelsDowngraded - 1))));
            if (effectiveRams >= ramsRequired)
                return wallLevel - levelsDowngraded;
            else
                return wallLevel;
        }

        public BattleResult SimulateAttack(Army attackingArmy, Army defendingArmy, int wallLevel, bool useArchers, int moralePercent = 100)
        {
            if (useArchers)
                return SimulateAttackWithArchers(attackingArmy, defendingArmy, wallLevel, moralePercent);
            else
                return SimulateAttackWithoutArchers(attackingArmy, defendingArmy, wallLevel, moralePercent);
        }

        public BattleResult SimulateAttackWithoutArchers(Army attackingArmy, Army defendingArmy, int wallLevel, int moralePercent = 100)
        {
            defendingArmy = defendingArmy ?? new Army();

            float morale = moralePercent / 100.0f;

            int numRams = attackingArmy.GetValueOrDefault(TroopType.Ram, 0);
            int effectiveWallLevel = LowerWallPreemptiveLevelAmount(numRams, wallLevel);
            if (!DoRamsLowerWallPreemptively(numRams, wallLevel))
                effectiveWallLevel = wallLevel;

            double totalInfantryAttack = TotalAttackPower(attackingArmy, UnitType.Infantry) * morale;
            double totalCavalryAttack = TotalAttackPower(attackingArmy, UnitType.Cavalry) * morale;
            double totalAttack = totalInfantryAttack + totalCavalryAttack;

            double totalInfantryDefense = (totalInfantryAttack / totalAttack) * TotalDefensePower(defendingArmy, UnitType.Infantry);
            double totalCavalryDefense = (totalCavalryAttack / totalAttack) * TotalDefensePower(defendingArmy, UnitType.Cavalry);
            double totalDefense = totalInfantryDefense + totalCavalryDefense;
            totalDefense *= ArmyStats.WallDefenseBuff[effectiveWallLevel];
            totalDefense += ArmyStats.WallBonusDefense[effectiveWallLevel];

            double winnerPower = totalAttack > totalDefense ? totalAttack : totalDefense;
            double loserPower = totalAttack > totalDefense ? totalDefense : totalAttack;
            double winnerLossRatio = (double)Math.Pow(loserPower / winnerPower, 0.5) / (winnerPower / loserPower);

            if (totalAttack > totalDefense)
            {
                return new BattleResult
                {
                    AttackingArmy = attackingArmy * (1 - winnerLossRatio),
                    DefendingArmy = Army.Empty,
                    NewWallLevel = WallLevelAfterDowngradeAttackerWon((int)(numRams * winnerLossRatio), (int)(numRams * (1 - winnerLossRatio)), wallLevel)
                };
            }
            else
            {
                float defenderLossRatio = TotalDefensePower(defendingArmy * winnerLossRatio) / (float)TotalDefensePower(defendingArmy);
                return new BattleResult
                {
                    AttackingArmy = Army.Empty,
                    DefendingArmy = defendingArmy * (1 - winnerLossRatio),
                    NewWallLevel = WallLevelAfterDowngradeAttackerLost(numRams, wallLevel, defenderLossRatio)
                };
            }
        }

        public BattleResult SimulateAttackWithArchers(Army attackingArmy, Army defendingArmy, int wallLevel, int moralePercent = 100)
        {
            defendingArmy = defendingArmy ?? new Army();

            var originalAttackingArmy = new Army(attackingArmy);
            var originalDefendingArmy = new Army(defendingArmy);
            float morale = moralePercent / 100.0f;

            attackingArmy = new Army(attackingArmy);
            defendingArmy = new Army(defendingArmy);

            int numRams = attackingArmy.GetValueOrDefault(TroopType.Ram, 0);
            int effectiveWallLevel = LowerWallPreemptiveLevelAmount(numRams, wallLevel);
            if (!DoRamsLowerWallPreemptively(numRams, wallLevel))
                effectiveWallLevel = wallLevel;

            var unitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>().ToArray();

            for (int i = 0; i < MaxSimulationIterations && !IsArmyEmpty(attackingArmy) && !IsArmyEmpty(defendingArmy); i++)
            {
                var attackPerTroopType = attackingArmy.ToDictionary(kvp => kvp.Key, kvp => morale * kvp.Value * ArmyStats.AttackPower[kvp.Key]);
                var attackByUnitType = unitTypes.ToDictionary(t => t, t => morale * attackingArmy.Where(kvp => ArmyStats.UnitType[kvp.Key] == t).Sum((kvp) => kvp.Value * ArmyStats.AttackPower[kvp.Key]));

                var totalAttack = attackPerTroopType.Values.Sum();

                var percentPerTroopType = attackPerTroopType.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / totalAttack);
                var percentPerBuildType = attackByUnitType.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / totalAttack);

                var attackerLosses = new List<Army>();
                var defenderLosses = new List<Army>();

                foreach (var unitType in unitTypes)
                {
                    float defenderPercent = percentPerBuildType[unitType];
                    var currentDefender = defendingArmy * defenderPercent;
                    var currentAttacker = attackingArmy.OfType(unitType);

                    float attackerPower = TotalAttackPower(currentAttacker);
                    if (attackerPower <= 0)
                        continue;

                    float defenderPower = TotalDefensePower(currentDefender, unitType);
                    defenderPower *= ArmyStats.WallDefenseBuff[effectiveWallLevel];
                    defenderPower += ArmyStats.WallBonusDefense[effectiveWallLevel] * defenderPercent;

                    float winnerPower = attackerPower > defenderPower ? attackerPower : defenderPower;
                    float loserPower = attackerPower > defenderPower ? defenderPower : attackerPower;

                    float winnerLossRatio = (float)Math.Pow(loserPower / winnerPower, 0.5) / (winnerPower / loserPower);

                    if (attackerPower > defenderPower)
                    {
                        defenderLosses.Add(currentDefender);
                        attackerLosses.Add(currentAttacker * winnerLossRatio);
                    }
                    else
                    {
                        attackerLosses.Add(currentAttacker);
                        defenderLosses.Add(currentDefender * winnerLossRatio);
                    }
                }

                foreach (var loss in attackerLosses)
                    attackingArmy -= loss;

                foreach (var loss in defenderLosses)
                    defendingArmy -= loss;
            }

            if (TotalAttackPower(attackingArmy) < TotalDefensePower(defendingArmy))
            {
                //  Defender won
                wallLevel = WallLevelAfterDowngradeAttackerLost(numRams, wallLevel, TotalDefensePower(defendingArmy) / TotalDefensePower(originalDefendingArmy));
            }
            else
            {
                //  Attacker won
                int originalRams = numRams;
                int newRams = attackingArmy.GetValueOrDefault(TroopType.Ram, 0);
                wallLevel = WallLevelAfterDowngradeAttackerWon(newRams, originalRams - newRams, wallLevel);
            }

            return new BattleResult
            {
                AttackingArmy = attackingArmy,
                DefendingArmy = defendingArmy,
                NewWallLevel = wallLevel
            };
        }




        public struct NukeEstimationResult
        {
            public int NukesRequired;
            public float LastNukeLossesPercent;
        }

        public NukeEstimationResult EstimateRequiredNukes(Army defendingArmy, int wallLevel, bool useArchers, int moralePercent)
        {
            defendingArmy = defendingArmy ?? new Army();
            var activeDefendingArmy = new Army(defendingArmy);
            int numNukes = 0;
            float lastNukeLossRatio = 0;
            for (int i = 0; i < MaxSimulationIterations && !IsArmyEmpty(activeDefendingArmy) && numNukes < 50; i++)
            {
                var battleResult = SimulateAttack(DefaultNukeArmy, activeDefendingArmy, wallLevel, useArchers, moralePercent);
                wallLevel = battleResult.NewWallLevel;
                activeDefendingArmy = battleResult.DefendingArmy;
                ++numNukes;

                lastNukeLossRatio = 1.0f - ArmyStats.CalculateTotalPopulation(battleResult.AttackingArmy) / (float)ArmyStats.CalculateTotalPopulation(DefaultNukeArmy);
            }

            return new NukeEstimationResult
            {
                NukesRequired = numNukes,
                LastNukeLossesPercent = lastNukeLossRatio * 100
            };
        }
    }
}
