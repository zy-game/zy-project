using System.Net;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerFramework;
using WebServer.DB;

namespace WebServer.Web
{
    [Route("api/web")]
    public class WebService : ControllerBase
    {
        [HttpGet("search/{msg}")]
        public string Search(string msg)
        {
            FilterDefinition<DBBook> filter = Builders<DBBook>.Filter.Empty;
            var books = Server.DBService.Where(filter);
            List<object> result = new List<object>();
            foreach (DBBook book in books)
            {
                if (book.info.Contains(msg) || book.title.Contains(msg))
                {
                    result.Add(new
                    {
                        uid = book.id,
                        book.title,
                        info = book.info.Length > 200 ? book.info[..200] : book.info
                    });
                }
            }
            return JsonConvert.SerializeObject(result);
        }



        [HttpGet("info/{id}")]
        public string GetBookData(string id)
        {
            FilterDefinition<DBBook> filter = Builders<DBBook>.Filter.Eq("_id", id);
            var data = Server.DBService.Where(filter).FirstOrDefault();
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
            var books = Server.DBService.Where(filter);
            foreach (DBBook book in books)
            {
                result.Add(new
                {
                    uid = book.id,
                    book.title,
                    info = book.info.Length > 200 ? book.info[..200] : book.info
                });
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> Post([FromForm] string model)
        {
            string data = await new StreamReader(Request.Body).ReadToEndAsync();
            Console.WriteLine(data);
            List<object> chats = new List<object>();
            try
            {
                JObject request = JObject.Parse(data);
                string value = request["value"].ToString();
                value = string.IsNullOrEmpty(value) == false && value.Length > 200 ? value[..200] : value;
                Server.DBService.Insert(new DBCommand() { title = request["message"].ToString(), text = value, time = DateTime.Now.ToString("G") });
                List<DBCommand> commands = Server.DBService.Where(Builders<DBCommand>.Filter.Where((x) => true));
                for (int i = commands.Count > 10 ? commands.Count - 10 : 0; i < commands.Count; i++)
                {
                    var item = commands[i];
                    chats.Add(new
                    {
                        role = item.title,
                        content = item.text + "\n\n Executed By " + item.time
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(Request.Host + ":" + e);
            }
            return JsonConvert.SerializeObject(chats);
        }

        [HttpGet("cmds")]
        public string GetCommandList()
        {
            List<object> chats = new List<object>();
            try
            {
                List<DBCommand> commands = Server.DBService.Where(Builders<DBCommand>.Filter.Where((x) => true));
                for (int i = commands.Count > 10 ? commands.Count - 10 : 0; i < commands.Count; i++)
                {
                    var item = commands[i];
                    chats.Add(new
                    {
                        role = item.title,
                        content = item.text + "\n\n Executed By " + item.time
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(Request.Host + ":" + e);
            }
            return JsonConvert.SerializeObject(chats);
        }
    }
}