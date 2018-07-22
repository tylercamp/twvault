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
    [Route("api/Village")]
    public class VillageController : ControllerBase
    {
        public VillageController(VaultContext context) : base(context)
        {
        }


        // GET: api/Villages
        [HttpGet(Name = "GetVillages")]
        public async Task<IActionResult> Get()
        {
            return Ok(await Paginated(context.Village).ToListAsync());
        }

        // GET: api/Villages/5
        [HttpGet("{id}", Name = "GetVillage")]
        public Task<IActionResult> Get(int id)
        {
            return FindOr404<Village>(id);
        }
    }
}
