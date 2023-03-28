using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebServer.DB;

namespace WebServer.ChatGPT
{
    [Route("api/chat-gpt")]
    //[Authorize]
    [ApiController]
    public class ChatGPTService : ControllerBase
    {
        private string apiKey = "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";
        private static HttpClient client = new HttpClient();

        [HttpGet("{msg}")]
        public async Task<string> Complettion(string msg)
        {
            GPTSession gPTSession = GPTSession.FindSession(Request.Headers["session"]);
            if (gPTSession == null)
            {
                gPTSession = GPTSession.AddSession(msg);
            }
            return await gPTSession.Question(msg);
        }

        [HttpGet("list")]
        public string Chats()
        {
            var sessions = GPTSession.GetChatList();
            if (sessions == null || sessions.Count == 0)
            {
                return "{}";
            }
            List<object> result = new List<object>();
            string resu = string.Empty;
            for (var i = 0; i < sessions.Count; i++)
            {
                resu += $"<li class=\"layui-timeline-item\" id = \"{sessions[i].id}\">";
                resu += "<i class=\"layui-icon layui-timeline-axis\"></i>";
                resu += "<div class=\"layui-timeline-content layui-text\">";
                resu += $"<a href = \"javascript:;\" οnclick = \"onInfo()\">";
                resu += $"<h3 class=\"layui-timeline-title\">{sessions[i].title}</h3>";
                resu += "</a>";
                var simple = sessions[i].chats?.LastOrDefault();
                resu += $"<p>{(simple?.content?.Length > 200 ? simple?.content?[..200] : simple?.content)}</p>";
                resu += "<ul>";
                resu += "</ul>";
                resu += "</div>";
                resu += "</li>";
            }
            return resu;
        }

        [HttpGet("session/{session}")]
        public string GetSessionData(string session)
        {
            GPTSession gPTSession = GPTSession.FindSession(session);
            if (gPTSession == null)
            {
                return string.Empty;
            }
            string resu = string.Empty;
            for (var i = 0; i < gPTSession.chats.Count; i++)
            {
                resu += $"<li class=\"layui-timeline-item\" id = \"{gPTSession.id}\">";
                resu += "<i class=\"layui-icon layui-timeline-axis\"></i>";
                resu += "<div class=\"layui-timeline-content layui-text\">";
                resu += $"<h3 class=\"layui-timeline-title\">{(gPTSession.chats[i].role == "user" ? "ME" : "Open AI")}</h3>";
                resu += $"<p>{gPTSession.chats[i].content}</p>";
                resu += "<ul>";
                resu += "</ul>";
                resu += "</div>";
                resu += "</li>";
            }
            return resu;
        }
    }
}
