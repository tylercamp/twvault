using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TW.Vault.Features.Simulation;
using TW.Vault.Model.JSON;

namespace TW.Testing
{
    class BattleTestSample
    {
        public String Label { get; set; }
        public bool ArchersEnabled { get; set; }
        public BattleTestState Start { get; set; }
        public BattleTestState Expect { get; set; }
    }

    class BattleTestState
    {
        public Army Attacker { get; set; }
        public Army Defender { get; set; }
        public int Wall { get; set; }
    }

    class BattleTestResult
    {
        public bool Passed { get; set; }
        public float ErrorPercent { get; set; }
        public BattleTestSample Sample { get; set; }
        public BattleTestState ActualResult { get; set; }
        public int Index { get; set; }
    }

    class BattleTester
    {
        List<BattleTestSample> testSamples;

        public float MaxErrorPercent { get; set; } = 0.05f;

        public BattleTester(String testDataLocation)
        {
            var testDataFile = File.ReadAllText(testDataLocation);
            testSamples = JsonConvert.DeserializeObject<List<BattleTestSample>>(testDataFile);
        }

        public List<BattleTestResult> Test()
        {
            var result = new List<BattleTestResult>();

            var simulator = new BattleSimulator();

            foreach (var sample in testSamples)
            {
                var simulationResult = simulator.SimulateAttack(new Army(sample.Start.Attacker), new Army(sample.Start.Defender), sample.Start.Wall, sample.ArchersEnabled);

                var attackerError = sample.Expect.Attacker.Sum(kvp => Math.Abs(kvp.Value - simulationResult.AttackingArmy[kvp.Key]) / (float)(1 + (sample.Start.Attacker[kvp.Key] - kvp.Value)));
                var defenderError = sample.Expect.Defender.Sum(kvp => Math.Abs(kvp.Value - simulationResult.DefendingArmy[kvp.Key]) / (float)(1 + (sample.Start.Defender[kvp.Key] - kvp.Value)));

                var lossesError = attackerError + defenderError;
                var wallError = Math.Abs(sample.Expect.Wall - simulationResult.NewWallLevel) / (1 + sample.Expect.Wall);

                var finalError = lossesError * 0.75f + wallError * 0.25f;

                result.Add(new BattleTestResult
                {
                    Passed = finalError < MaxErrorPercent,
                    ErrorPercent = finalError,
                    Sample = sample,
                    Index = result.Count,
                    ActualResult = new BattleTestState
                    {
                        Wall = simulationResult.NewWallLevel,
                        Attacker = simulationResult.AttackingArmy,
                        Defender = simulationResult.DefendingArmy
                    }
                });
            }

            return result;
        }
    }
}
