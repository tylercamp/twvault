using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.JSON
{
    public class Army : Dictionary<String, int> { }
    public class BuildingLevels : Dictionary<String, short> { }
    public class ManyCommands : List<Command> { }
}
