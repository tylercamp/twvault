using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TW.Vault.Model.Convert;

using JSON = TW.Vault.Model.JSON;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using TW.Vault.Model.Validation;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/{worldName}/Command")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class CommandController : BaseController
    {
        public CommandController(Scaffold.VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        // GET: api/Command
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var commands = await Paginated(context.Command.IncludeCommandData()).FromWorld(CurrentWorldId).ToListAsync();
            var jsonCommands = commands.Select(CommandConvert.ModelToJson);
            return Ok(jsonCommands);
        }

        // GET: api/Command/5
        [HttpGet("{id}", Name = "Get")]
        public Task<IActionResult> Get(long id)
        {
            return SelectOr404<Scaffold.Command>(
                q => q.Where(c => c.CommandId == id).FromWorld(CurrentWorldId),
                c => CommandConvert.ModelToJson(c)
            );
        }

        [HttpGet("village/target/{villageId}")]
        public async Task<IActionResult> GetByTargetVillage(long villageId)
        {
            var commands = await Paginated(
                    from command in context.Command.IncludeCommandData().FromWorld(CurrentWorldId)
                    where command.TargetVillageId == villageId
                    select command
                ).ToListAsync();

            var jsonCommands = commands.Select(CommandConvert.ModelToJson);
            return Ok(jsonCommands);
        }

        [HttpGet("village/source/{villageId}")]
        public async Task<IActionResult> GetBySourceVillage(long villageId)
        {
            var commands = await Paginated(
                from command in context.Command.IncludeCommandData().FromWorld(CurrentWorldId)
                where command.SourceVillageId == villageId
                select command
            ).ToListAsync();

            var jsonCommands = commands.Select(CommandConvert.ModelToJson);
            return Ok(jsonCommands);
        }

        [HttpGet("player/target/{playerId}")]
        public async Task<IActionResult> GetByTargetPlayer(long playerId)
        {
            var commands = await Paginated(
                from command in context.Command.IncludeCommandData().FromWorld(CurrentWorldId)
                where command.TargetPlayerId == playerId
                select command
            ).ToListAsync();

            var jsonCommands = commands.Select(CommandConvert.ModelToJson);
            return Ok(jsonCommands);
        }

        [HttpGet("player/source/{playerId}")]
        public async Task<IActionResult> GetBySourcePlayer(long playerId)
        {
            var commands = await Paginated(
                from command in context.Command.IncludeCommandData().FromWorld(CurrentWorldId)
                where command.SourcePlayerId == playerId
                select command
            ).ToListAsync();

            var jsonCommands = commands.Select(CommandConvert.ModelToJson);
            return Ok(jsonCommands);
        }
        
        // POST: api/Command
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]JSON.ManyCommands jsonCommands)
        {
            if (ModelState.IsValid)
            {
                var mappedCommands = jsonCommands.ToDictionary(c => c.CommandId, c => c);
                var commandIds = jsonCommands.Select(c => c.CommandId).ToList();


                var scaffoldCommands = await Profile("Get existing scaffold commands", () => (
                        from command in context.Command.IncludeCommandData().FromWorld(CurrentWorldId)
                        where commandIds.Contains(command.CommandId)
                        select command
                    ).ToListAsync()
                );

                var mappedScaffoldCommands = scaffoldCommands.ToDictionary(c => c.CommandId, c => c);

                var tx = BuildTransaction();
                context.Transaction.Add(tx);

                Profile("Generate scaffold commands", () =>
                {
                    foreach (var jsonCommand in jsonCommands)
                    {
                        if (!Configuration.Security.AllowCommandArrivalBeforeServerTime
                                && jsonCommand.LandsAt.HasValue
                                && jsonCommand.LandsAt.Value < CurrentServerTime)
                        {
                            context.InvalidDataRecord.Add(MakeInvalidDataRecord(
                                JsonConvert.SerializeObject(jsonCommand),
                                "Command.landsAt is earlier than current server time"
                            ));
                            continue;
                        }

                        if (!Configuration.Security.ReportIgnoreExpectedPopulationBounds
                                && !ArmyValidate.MeetsPopulationRestrictions(jsonCommand.Troops))
                        {
                            context.InvalidDataRecord.Add(MakeInvalidDataRecord(
                                JsonConvert.SerializeObject(jsonCommand),
                                "Troops in command exceed possible village population"
                            ));
                            continue;
                        }

                        var scaffoldCommand = mappedScaffoldCommands.GetValueOrDefault(jsonCommand.CommandId.Value);
                        if (scaffoldCommand == null)
                        {
                            scaffoldCommand = new Scaffold.Command();
                            scaffoldCommand.World = CurrentWorld;
                            jsonCommand.ToModel(scaffoldCommand, context);
                            context.Command.Add(scaffoldCommand);
                        }
                        else
                        {
                            var existingJsonCommand = CommandConvert.ModelToJson(scaffoldCommand);
                            if (existingJsonCommand.IsReturning == jsonCommand.IsReturning && existingJsonCommand != jsonCommand)
                            {
                                context.ConflictingDataRecord.Add(new Scaffold.ConflictingDataRecord
                                {
                                    OldTxId = scaffoldCommand.TxId.Value,
                                    ConflictingTx = tx
                                });
                            }

                            jsonCommand.ToModel(scaffoldCommand, context);
                        }

                        scaffoldCommand.Tx = tx;
                    }
                });

                await Profile("Save changes", () => context.SaveChangesAsync());

                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
