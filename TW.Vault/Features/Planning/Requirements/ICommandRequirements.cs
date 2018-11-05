using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TW.Vault.Model;

namespace TW.Vault.Features.Planning.Requirements
{
    //  Can be ie - Minimum def, minimum off, at least X troops, can kill army Y, etc
    public interface ICommandRequirements
    {
        bool MeetsRequirement(decimal worldSpeed, decimal travelSpeed, Coordinate source, Coordinate target, Model.JSON.Army army);
    }
}
