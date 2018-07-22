using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TW.Vault.Controllers
{
    [Produces("application/json")]
    [Route("api/Report")]
    public class ReportController : Controller
    {
        // GET: api/Reports
        [HttpGet(Name = "GetReports")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Reports/5
        [HttpGet("{id}", Name = "GetReport")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/Reports
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/Reports/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
