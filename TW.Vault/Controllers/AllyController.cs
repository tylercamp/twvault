using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TW.Vault.Scaffold_Model;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/Ally")]
    public class AllyController : ControllerBase
    {
        public AllyController(VaultContext context) : base(context)
        {
        }

        // GET: api/Ally
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await Paginated(context.Ally).ToListAsync());
        }

        // GET: api/Ally/5
        [HttpGet("{id}", Name = "Get")]
        public Task<IActionResult> Get(int id)
        {
            return FindOr404<Ally>(id);
        }
    }
}
