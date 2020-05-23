using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TW.Vault.Scaffold;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Controllers
{
    //  Creation and modification of translations done within a player
    //  context to prevent users from editing each other's translations
    /// directly

    [Produces("application/json")]
    [Route("api/{worldName}/PlayerTranslation")]
    [EnableCors("AllOrigins")]
    [ServiceFilter(typeof(Security.RequireAuthAttribute))]
    public class PlayerTranslationController : BaseController
    {

        [HttpGet]
        public IActionResult GetCurrentTranslation()
        {
            var translation = CurrentTranslation;

            return Ok(new {
                translation.Id,
                translation.LanguageId,
                translation.Name,
                Language = translation.Language.Name,
                Entries = Translation.GetFullTranslation()
            });
        }

        [HttpPost]
        public IActionResult CreateNewTranslation([FromBody] JSON.TranslationRegistry newRegistry)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var language = context.TranslationLanguage.Where(l => l.Id == newRegistry.LanguageId).FirstOrDefault();
            if (language == null)
            {
                return NotFound("Language does not exist with ID " + newRegistry.LanguageId);
            }

            var existingRegistry = context.TranslationRegistry
                .Where(r => r.LanguageId == newRegistry.LanguageId)
                .Where(r => r.Name == newRegistry.Name)
                .Where(r => r.AuthorPlayerId == CurrentPlayerId)
                .FirstOrDefault();

            if (existingRegistry != null)
                return Conflict();

            TranslationValidationResult validationResult;
            if (!ValidateRegistry(newRegistry.Entries, out validationResult))
                return BadRequest(validationResult.FailureReasons);

            var scaffoldRegistry = new Scaffold.TranslationRegistry
            {
                AuthorPlayerId = CurrentPlayerId,
                Author = CurrentSets.Player.Where(p => p.PlayerId == CurrentPlayerId).First().PlayerName,
                LanguageId = newRegistry.LanguageId,
                Name = newRegistry.Name
            };

            context.Add(scaffoldRegistry);
            context.SaveChanges();

            var translationKeys = context.TranslationKey.ToDictionary(k => k.Name, k => k.Id);
            foreach (var (key, value) in newRegistry.Entries.Tupled())
            {
                if (!translationKeys.ContainsKey(key))
                    continue;

                if (String.IsNullOrWhiteSpace(value))
                    continue;

                var newEntry = new Scaffold.TranslationEntry
                {
                    KeyId = translationKeys[key],
                    TranslationId = scaffoldRegistry.Id,
                    Value = value
                };

                context.Add(newEntry);
            }

            context.SaveChanges();

            return Ok(new { newRegistryId = scaffoldRegistry.Id });
        }

        [HttpPut]
        public IActionResult UpdateTranslation([FromBody] JSON.TranslationRegistry jsonRegistry)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var scaffoldRegistry = context
                .TranslationRegistry
                .Include(r => r.Entries)
                .Where(r => r.Id == jsonRegistry.Id)
                .Where(r => r.AuthorPlayerId == CurrentPlayerId)
                .FirstOrDefault();

            if (scaffoldRegistry == null)
                return NotFound();

            var translationKeys = context.TranslationKey.ToDictionary(k => k.Name, k => k.Id);
            var translationKeysInverted = translationKeys.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            TranslationValidationResult validationResult;
            if (!ValidateRegistry(jsonRegistry.Entries, out validationResult))
                return BadRequest(validationResult.FailureReasons);

            var existingKeyIds = scaffoldRegistry.Entries.Select(e => e.KeyId).ToList();

            var addedKeyNames = jsonRegistry.Entries.Keys
                .Where(k => translationKeys.ContainsKey(k))
                .Where(k => !existingKeyIds.Contains(translationKeys[k]))
                .ToList();

            var removedKeyNames = translationKeys
                .Where((kvp) => !jsonRegistry.Entries.ContainsKey(kvp.Key))
                .Where((kvp) => existingKeyIds.Contains(kvp.Value))
                .Select(kvp => kvp.Key)
                .ToList();

            scaffoldRegistry.Name = jsonRegistry.Name;

            // Delete removed entries
            foreach (var removedEntry in scaffoldRegistry.Entries)
            {
                if (!removedKeyNames.Contains(translationKeysInverted[removedEntry.KeyId]))
                    continue;

                context.Remove(removedEntry);
            }

            context.SaveChanges();

            // Create new entries
            foreach (var addedKeyName in addedKeyNames)
            {
                var keyId = translationKeys[addedKeyName];

                var scaffoldEntry = new Scaffold.TranslationEntry
                {
                    KeyId = keyId,
                    TranslationId = scaffoldRegistry.Id,
                    Value = jsonRegistry.Entries[addedKeyName]
                };

                context.Add(scaffoldEntry);
            }

            context.SaveChanges();

            // Modify existing entries
            var existingScaffoldEntries = scaffoldRegistry.Entries.ToDictionary(e => e.KeyId, e => e);

            foreach (var (modifiedKeyName, value) in jsonRegistry.Entries.Tupled())
            {
                if (addedKeyNames.Contains(modifiedKeyName) || removedKeyNames.Contains(modifiedKeyName))
                    continue;

                if (!translationKeys.ContainsKey(modifiedKeyName))
                    continue;

                var keyId = translationKeys[modifiedKeyName];
                var oldValue = existingScaffoldEntries[keyId].Value;
                if (oldValue != value)
                    existingScaffoldEntries[keyId].Value = value;
            }

            context.SaveChanges();

            return Ok();
        }

        [HttpDelete("{translationId}")]
        public IActionResult DeleteTranslation(short translationId)
        {
            var registry = context.TranslationRegistry.Include(r => r.Entries).FirstOrDefault(r => r.Id == translationId);
            if (registry == null)
                return NotFound();

            if (registry.AuthorPlayerId != CurrentPlayerId)
                return Unauthorized();

            // Don't allow deleting translations that are used as defaults
            var worldsUsingDefault = context.World.Where(w => w.DefaultTranslationId == translationId).Count();
            if (worldsUsingDefault > 0 || Configuration.Translation.BaseTranslationId == translationId)
                return Conflict();

            context.RemoveRange(registry.Entries);
            context.SaveChanges();
            context.Remove(registry);
            context.SaveChanges();

            return Ok();
        }

        private class TranslationValidationResult
        {
            public Dictionary<String, String> FailureReasons { get; set; } = new Dictionary<String, string>();
        }

        private static readonly Regex ParameterExtractor = new Regex(@"\{([^\}]+)\}");

        public PlayerTranslationController(VaultContext context, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory) : base(context, scopeFactory, loggerFactory)
        {
        }

        private bool ValidateRegistry(Dictionary<String, String> checkedEntries, out TranslationValidationResult validationResult)
        {
            var parameters =
                context.TranslationParameter
                    .Select(p => new { Key = p.Key.Name, p.Name })
                .ToList()
                .GroupBy(p => p.Key)
                .ToDictionary(g => g.Key, g => g.Select(p => p.Name).ToList());

            var result = new TranslationValidationResult();

            foreach (var (key, value) in checkedEntries.Tupled())
            {
                if (!parameters.ContainsKey(key))
                    continue;

                var keyParameters = parameters[key];
                var discoveredParameters = ParameterExtractor
                    .Matches(value)
                    ?.SelectMany(m => m.Groups.Select<Group, String>(g => g.Value))
                ?? new String[] { };

                var missingParameters = keyParameters.Except(discoveredParameters);
                if (missingParameters.Any())
                {
                    var message = $"Missing parameters: {String.Join(", ", missingParameters.Select(p => $"\"{p}\""))}";
                    result.FailureReasons.Add(key, message);
                }
            }

            if (result.FailureReasons.Count > 0)
            {
                validationResult = result;
                return false;
            }
            else
            {
                validationResult = null;
                return true;
            }
        }
    }
}