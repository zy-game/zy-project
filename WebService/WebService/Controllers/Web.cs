using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class web : ControllerBase
    {
        // GET: api/<Web>
        [HttpGet]
        public string Get()
        {
            List<object> list = new List<object>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(new
                {
                    id =i,
                    title = "test" + i,
                    time = DateTime.Now.Ticks,
                    simple = "hahahhahhaha",
                });
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(list);
        }

        // GET api/<Web>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<Web>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<Web>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<Web>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
