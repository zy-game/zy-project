using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServer.Gateway
{
    [Route("api/assets")]
    [ApiController]
    public class Files : ControllerBase
    {
        // GET api/<Files>/5
        [HttpGet("{group}")]
        public string Get(string group)
        {
            return "value";
        }

        // POST api/<Files>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<Files>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<Files>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
