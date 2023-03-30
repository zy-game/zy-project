using System.Net;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WebServer.DB;

namespace WebServer.Web
{
    [Route("api/web")]
    public class WebService : ControllerBase
    {
        [HttpGet("search/{msg}")]
        public string Search(string msg)
        {
            return string.Empty;
        }
        [HttpGet("info/{id}")]
        public string GetBookData(string id)
        {
            FilterDefinition<DBBook> filter = Builders<DBBook>.Filter.Eq("_id", id);
            var data = Mongo<DBBook>.instance.Where(filter).FirstOrDefault();
            if (data == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return string.Empty;
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(new { data.id, data.title, text = data.info });
        }

        [HttpGet("{type}")]
        public string Get(string type)
        {
            List<object> result = new List<object>();
            FilterDefinition<DBBook> filter = Builders<DBBook>.Filter.Empty;
            switch (type)
            {
                case "unity":
                    filter = Builders<DBBook>.Filter.Eq("tag", "unity");

                    break;
                case "csharp":
                    filter = Builders<DBBook>.Filter.Eq("tag", "csharp");
                    break;
            }
            var books = Mongo<DBBook>.instance.Where(filter);
            foreach (DBBook book in books)
            {
                result.Add(new
                {
                    uid = book.id,
                    book.title,
                    info = book.info.Length > 200 ? book.info[..200] : book.info
                });
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public string Post([FromBody] string data)
        {
            try
            {
                DBBook book = JsonConvert.DeserializeObject<DBBook>(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(Request.Host + ":" + e.Message);
            }
            return "OK";
        }
    }
}