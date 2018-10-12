﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Features.Simulation;
using TW.Vault.Model;
using TW.Vault.Model.JSON;

namespace TW.Vault.Features.Planning.Requirements
{
    public class MinimumOffenseRequirement : ICommandRequirements
    {
        public int MinimumOffense { get; set; }

        public bool MeetsRequirement(Coordinate source, Coordinate target, Army army)
        {
            return BattleSimulator.AttackPower(army).Total >= MinimumOffense;
        }
    }
}