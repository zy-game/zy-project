using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServer.Gateway
{
    [Route("api/message")]
    [ApiController]
    public class Message : ControllerBase
    {
        // GET api/<Message>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<Message>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<Message>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }
    }
}
