using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JSON = TW.Vault.Model.JSON;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/Translation")]
    [EnableCors("AllOrigins")]
    public class TranslationController : Controller
    {
        Scaffold.VaultContext context;

        public TranslationController(Scaffold.VaultContext vaultContext)
        {
            this.context = vaultContext;
        }

        [HttpGet]
        public IActionResult GetAllTranslations() => Ok(
            context.TranslationRegistry.Where(r => !r.IsSystemInternal).OrderBy(r => r.Author).ThenBy(r => r.Name).Select(r => new
            {
                Author = r.Author.UrlDecode(), r.AuthorPlayerId, r.Id, Name = r.Name.UrlDecode(), r.LanguageId
            })
        );

        [HttpGet("{translationId}")]
        public IActionResult GetTranslationRegistry(int translationId)
        {
            var registry = context.TranslationRegistry.Include(r => r.Language).Where(r => r.Id == translationId).FirstOrDefault();
            if (registry == null)
                return NotFound();
            else
                return Ok(new { registry.Id, Name = registry.Name.UrlDecode(), Author = registry.Author.UrlDecode(), registry.AuthorPlayerId, registry.LanguageId, Language = registry.Language.Name });
        }

        [HttpGet("{translationId}/contents")]
        public IActionResult GetTranslationContents(int translationId, bool ignoreMissingKeys = false)
        {
            var registry = context
                .TranslationRegistry
                .Include(r => r.Language)
                .Include(r => r.Entries)
                    .ThenInclude(e => e.Key)
                .Where(r => r.Id == translationId).FirstOrDefault();

            if (registry == null)
                return NotFound();

            return Ok(new {
                registry.Id,
                registry.LanguageId,
                Name = registry.Name.UrlDecode(),
                registry.AuthorPlayerId,
                Language = registry.Language.Name,
                Entries = registry.Entries.ToDictionary(
                    e => e.Key.Name,
                    e => e.Value
                )
            });
        }

        [HttpGet("languages")]
        public IActionResult GetLanguages() => Ok(
            context.TranslationLanguage
                .Select(l => new { l.Id, l.Name })
                .ToList()
        );

        [HttpGet("languages/{languageId}")]
        public IActionResult GetTranslations(int languageId)
        {
            var language = context.TranslationLanguage.Include(l => l.Translations).Where(l => l.Id == languageId).FirstOrDefault();
            if (language == null)
                return NotFound();

            return Ok(
                language.Translations
                    .Where(t => !t.IsSystemInternal)
                    .Select(t => new { t.Id, Name = t.Name.UrlDecode(), Author = t.Author.UrlDecode(), t.AuthorPlayerId })
                    .OrderBy(r => r.Author).ThenBy(r => r.Name)
                    .ToList()
            );
        }

        [HttpGet("translation-keys")]
        public IActionResult GetTranslationKeys() => Ok(
            context.TranslationKey.Select(k => new { k.Id, k.Name, k.IsTwNative, k.Group, k.Note }).ToList()
        );

        [HttpGet("default/{serverName}")]
        public IActionResult GetDefaultServerTranslationId(String serverName)
        {
            var world = context.World
                .Include(w => w.DefaultTranslation)
                    .ThenInclude(t => t.Language)
                .Where(w => w.Hostname == serverName)
                .FirstOrDefault();

            if (world == null)
                return NotFound();

            return Ok(new
            {
                translationId = world.DefaultTranslationId,
                languageId = world.DefaultTranslation.Language.Id,
                translationAuthor = world.DefaultTranslation.Author.UrlDecode(),
                translationName = world.DefaultTranslation.Name.UrlDecode(),
                language = world.DefaultTranslation.Language.Name
            });
        }

        [HttpGet("reference")]
        public IActionResult GetReferenceTranslation() => GetTranslationContents(Configuration.Translation.BaseTranslationId);

        [HttpGet("parameters")]
        public IActionResult GetAllTranslationParameters()
        {
            return Ok(
                context.TranslationParameter
                    .Select(p => new { Key = p.Key.Name, p.Name })
                    .ToList()
                    .GroupBy(p => p.Key)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(p => p.Name).ToList()
                    )
            );
        }
    }
}