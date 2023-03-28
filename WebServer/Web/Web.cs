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

        [HttpGet("{type}")]
        public string Get(string type)
        {
            Console.WriteLine(Response.HttpContext.Connection.RemoteIpAddress.ToString() + " => " + type);
            string result = string.Empty;
            List<Book> books = new List<Book>();
            switch (type)
            {
                case "none":
                    books = Mongo.Where(Builders<Book>.Filter.Empty);
                    foreach (Book book in books)
                    {
                        result += "<li class=\"layui-timeline-item\">";
                        result += "<i class=\"layui-icon layui-timeline-axis\">¤š</i>";
                        result += "<div class=\"layui-timeline-content layui-text\">";
                        result += $"<a id=\" {book.id} href = \"javascript:;\" ¦Ïnclick = \"onInfo({book.id})\">";
                        result += $"<h3 class=\"layui-timeline-title\">{book.title}</h3>";
                        result += "</a>";
                        result += $"<p>{(book.info.Length > 200 ? book.info[..200] : book.info)}</p>";
                        result += "<ul>";
                        result += "</ul>";
                        result += "</div>";
                        result += "</li>";
                    }
                    break;
                case "unity":
                    books = Mongo.Where(Builders<Book>.Filter.Eq("tag", "unity"));
                    foreach (Book book in books)
                    {
                        result += "<li class=\"layui-timeline-item\">";
                        result += "<i class=\"layui-icon layui-timeline-axis\">¤š</i>";
                        result += "<div class=\"layui-timeline-content layui-text\">";
                        result += $"<a id=\" {book.id} href = \"javascript:;\" ¦Ïnclick = \"onInfo({book.id})\">";
                        result += $"<h3 class=\"layui-timeline-title\">{book.title}</h3>";
                        result += "</a>";
                        result += $"<p>{(book.info.Length > 200 ? book.info[..200] : book.info)}</p>";
                        result += "<ul>";
                        result += "</ul>";
                        result += "</div>";
                        result += "</li>";
                    }
                    break;
                case "c#":
                    books = Mongo.Where(Builders<Book>.Filter.Eq("tag", "c#"));
                    foreach (Book book in books)
                    {
                        result += "<li class=\"layui-timeline-item\">";
                        result += "<i class=\"layui-icon layui-timeline-axis\">¤š</i>";
                        result += "<div class=\"layui-timeline-content layui-text\">";
                        result += $"<a id=\" {book.id} href = \"javascript:;\" ¦Ïnclick = \"onInfo({book.id})\">";
                        result += $"<h3 class=\"layui-timeline-title\">{book.title}</h3>";
                        result += "</a>";
                        result += $"<p>{(book.info.Length > 200 ? book.info[..200] : book.info)}</p>";
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
            }
            catch (Exception e)
            {
                Console.WriteLine(Request.Host + ":" + e.Message);
            }
            return "OK";
        }
    }
}