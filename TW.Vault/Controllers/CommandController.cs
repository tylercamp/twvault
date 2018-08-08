using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TW.Vault.Model.Convert;

using Scaffold = TW.Vault.Scaffold_Model;
using JSON = TW.Vault.Model.JSON;
using Microsoft.AspNetCore.Cors;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/Command")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class CommandController : ControllerBase
    {
        public CommandController(Scaffold.VaultContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {
        }

        // GET: api/Command
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var commands = await Paginated(context.Command.IncludeCommandData()).ToListAsync();
            var jsonCommands = commands.Select(CommandConvert.ModelToJson);
            return Ok(jsonCommands);
        }

        // GET: api/Command/5
        [HttpGet("{id}", Name = "Get")]
        public Task<IActionResult> Get(long id)
        {
            return FindOr404<Scaffold.Command>(id, c => CommandConvert.ModelToJson(c));
        }

        [HttpGet("village/target/{villageId}")]
        public async Task<IActionResult> GetByTargetVillage(long villageId)
        {
            var commands = await Paginated(
                    from command in context.Command.IncludeCommandData()
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
                from command in context.Command.IncludeCommandData()
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
                from command in context.Command.IncludeCommandData()
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
                from command in context.Command.IncludeCommandData()
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


                var scaffoldCommands = await (
                        from command in context.Command.IncludeCommandData()
                        where commandIds.Contains(command.CommandId)
                        select command
                    ).ToListAsync();

                var mappedScaffoldCommands = scaffoldCommands.ToDictionary(c => c.CommandId, c => c);

                var tx = BuildTransaction();
                await context.Transaction.AddAsync(tx);

                foreach (var jsonCommand in jsonCommands)
                {
                    var scaffoldCommand = mappedScaffoldCommands.GetValueOrDefault(jsonCommand.CommandId.Value);
                    if (scaffoldCommand == null)
                    {
                        scaffoldCommand = new Scaffold.Command();
                        jsonCommand.ToModel(scaffoldCommand, context);
                        await context.Command.AddAsync(scaffoldCommand);
                    }
                    else
                    {
                        jsonCommand.ToModel(scaffoldCommand, context);
                    }

                    scaffoldCommand.Tx = tx;
                }

                await context.SaveChangesAsync();

                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
