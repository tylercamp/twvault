using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/Translation")]
    public class TranslationController : Controller
    {
        Scaffold.VaultContext context;

        public TranslationController(Scaffold.VaultContext vaultContext)
        {
            this.context = vaultContext;
        }

        [HttpGet]
        public IActionResult GetAllTranslations() => Ok(
            context.TranslationRegistry.Select(r => new
            {
                r.Author, r.AuthorPlayerId, r.Id, r.Name, r.LanguageId
            })
        );

        [HttpGet("{translationId}")]
        public IActionResult GetTranslationRegistry(int translationId)
        {
            var registry = context.TranslationRegistry.Include(r => r.Language).Where(r => r.Id == translationId).FirstOrDefault();
            if (registry == null)
                return NotFound();
            else
                return Ok(new { registry.Id, registry.Name, registry.Author, registry.AuthorPlayerId, registry.LanguageId, Language = registry.Language.Name });
        }

        [HttpGet("{translationId}/contents")]
        public IActionResult GetTranslationContents(int translationId)
        {
            var registry = context
                .TranslationRegistry
                .Include(r => r.Entries)
                    .ThenInclude(e => e.Key)
                .Where(r => r.Id == translationId).FirstOrDefault();

            if (registry == null)
                return NotFound();

            return Ok(new {
                registry.Id,
                registry.LanguageId,
                registry.Name,
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
                    .Select(t => new { t.Id, t.Name, t.Author, t.AuthorPlayerId })
                    .ToList()
            );
        }
    }
}