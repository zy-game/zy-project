using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace WebServer
{
    [Route("api/web")]
    public class Web : ControllerBase
    {
        class Book
        {
            public string id;
            public string title;
            public string tag;
            public string info;
        }

        [HttpGet("search/{msg}")]
        public string Search(string msg)
        {
            return string.Empty;
        }

        [HttpGet("{type}")]
        public string Get(string type)
        {
            string result = string.Empty;
            List<Book> books = new List<Book>();
            switch (type)
            {
                case "none":
                    books = Mongo.Where<Book>(Builders<Book>.Filter.Empty);
                    foreach (Book book in books)
                    {
                        result += "<li class=\"layui-timeline-item\">";
                        result += "<i class=\"layui-icon layui-timeline-axis\">¤š</i>";
                        result += "<div class=\"layui-timeline-content layui-text\">";
                        result += $"<a id=\" {book.id} href = \"javascript:;\" ¦Ïnclick = \"onInfo({book.id})\">";
                        result += $"<h3 class=\"layui-timeline-title\">{book.title}</h3>";
                        result += "</a>";
                        result += $"<p>{book.info[..200]}</p>";
                        result += "<ul>";
                        result += "</ul>";
                        result += "</div>";
                        result += "</li>";
                    }
                    break;
                case "unity":
                    books = Mongo.Where<Book>(Builders<Book>.Filter.Eq("tag", "unity"));
                    foreach (Book book in books)
                    {
                        result += "<li class=\"layui-timeline-item\">";
                        result += "<i class=\"layui-icon layui-timeline-axis\">¤š</i>";
                        result += "<div class=\"layui-timeline-content layui-text\">";
                        result += $"<a id=\" {book.id} href = \"javascript:;\" ¦Ïnclick = \"onInfo({book.id})\">";
                        result += $"<h3 class=\"layui-timeline-title\">{book.title}</h3>";
                        result += "</a>";
                        result += $"<p>{book.info[..200]}</p>";
                        result += "<ul>";
                        result += "</ul>";
                        result += "</div>";
                        result += "</li>";
                    }
                    break;
                case "c#":
                    books = Mongo.Where<Book>(Builders<Book>.Filter.Eq("tag", "c#"));
                    foreach (Book book in books)
                    {
                        result += "<li class=\"layui-timeline-item\">";
                        result += "<i class=\"layui-icon layui-timeline-axis\">¤š</i>";
                        result += "<div class=\"layui-timeline-content layui-text\">";
                        result += $"<a id=\" {book.id} href = \"javascript:;\" ¦Ïnclick = \"onInfo({book.id})\">";
                        result += $"<h3 class=\"layui-timeline-title\">{book.title}</h3>";
                        result += "</a>";
                        result += $"<p>{book.info[..200]}</p>";
                        result += "<ul>";
                        result += "</ul>";
                        result += "</div>";
                        result += "</li>";
                    }
                    break;
            }
            return result;
        }

        [HttpPost]
        public string Post(string data)
        {
            try
            {
                Book book = JsonConvert.DeserializeObject<Book>(data);
                Mongo.UpsertDocument<Book>(Builders<Book>.Filter.Empty, book);
            }
            catch (Exception e)
            {
                Console.WriteLine(Request.Host + ":" + e.Message);
            }
            return "OK";
        }
    }
}