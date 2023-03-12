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
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "PUT,POST,GET,DELETE,OPTIONS");
            var result = "";
            result += "<li class=\"layui-timeline-item\">";
            result += "<i class=\"layui-icon layui-timeline-axis\">&#xe63f; </i>";
            result += "<div class=\"layui-timeline-content layui-text\">";
            result += "<h3 class=\"layui-timeline-title\">";
            result += "<a href=\"test.html\">";
            result += "8月18日</a></h3><p> layui 2.0 的一切准备工作似乎都已到位。发布之弦，一触即发。<br>不枉近百个日日夜夜与之为伴。因小而大，因弱而强。<br>无论它能走多远，抑或如何支撑？至少我曾倾注全心，无怨无悔...</p></div></li>";
            return result;
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
