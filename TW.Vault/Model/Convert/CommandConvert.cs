using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Model.Convert
{
    public static class CommandConvert
    {
        public static Scaffold.Command ToModel(this JSON.Command command, Scaffold.Command existingCommand, Scaffold.VaultContext context = null) =>
            JsonToModel(command, existingCommand, context);

        public static JSON.Command ToJson(this Scaffold.Command command) =>
            ModelToJson(command);

        public static Scaffold.Command JsonToModel(JSON.Command command, Scaffold.Command existingCommand, Scaffold.VaultContext context = null)
        {
            if (command == null)
            {
                if (existingCommand != null && context != null)
                    context.Remove(existingCommand);

                return null;
            }

            Scaffold.Command result;
            if (existingCommand != null)
            {
                result = existingCommand;
            }
            else
            {
                result = new Scaffold.Command();
                result.CommandId = command.CommandId.Value;
                if (context != null)
                    context.Add(result);
            }

            result.CommandId        = command.CommandId.Value;
            result.SourcePlayerId   = command.SourcePlayerId.Value;
            result.SourceVillageId  = command.SourceVillageId.Value;
            result.TargetPlayerId   = command.TargetPlayerId;
            result.TargetVillageId  = command.TargetVillageId.Value;
            result.LandsAt          = command.LandsAt.Value;
            result.FirstSeenAt      = result.FirstSeenAt == DateTime.MinValue ? DateTime.UtcNow : result.FirstSeenAt;
            result.IsAttack         = command.CommandType == JSON.CommandType.Attack;
            result.IsReturning      = command.IsReturning.Value;

            result.Army             = ArmyConvert.JsonToArmy(command.Troops, result.Army, context);
            result.TroopType        = TroopTypeConvert.TroopTypeToString(command.TroopType);

            if (result.Army != null) result.Army.WorldId = existingCommand.World?.Id ?? existingCommand.WorldId;

            return result;
        }

        public static JSON.Command ModelToJson(Scaffold.Command command)
        {
            var result = new JSON.Command();

            result.CommandId        = command.CommandId;
            result.SourcePlayerId   = command.SourcePlayerId;
            result.SourceVillageId  = command.SourceVillageId;
            result.TargetPlayerId   = command.TargetPlayerId;
            result.TargetVillageId  = command.TargetVillageId;
            result.LandsAt          = command.LandsAt;
            result.CommandType      = command.IsAttack ? JSON.CommandType.Attack : JSON.CommandType.Support;
            result.IsReturning      = command.IsReturning;

            result.TroopType        = TroopTypeConvert.StringToTroopType(command.TroopType);
            result.Troops           = ArmyConvert.ArmyToJson(command.Army);

            return result;
        }
    }
}
