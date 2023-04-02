using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebServer.DB;
using System.Diagnostics;

namespace WebServer.ChatGPT
{
    [Route("api/chat-gpt")]
    //[Authorize]
    [ApiController]
    public class ChatGPT : ControllerBase
    {
        [HttpGet("chat/{msg}")]
        public async Task<string> Complettion(string msg)
        {
            string session_id = Request.Headers["session"];
            ChatChannel chat_session = ChatChannel.FindSession(session_id);
            if (chat_session == null)
            {
                chat_session = ChatChannel.AddSession(msg);
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            string response = await chat_session.Question(msg);
            List<object> result = new List<object>();
            for (var i = 0; i < chat_session.chats.Count; i++)
            {
                result.Add(new
                {
                    role = chat_session.chats[i].role == "user" ? "Me" : "Open AI",
                    chat_session.chats[i].content
                });
            }
            stopwatch.Stop();
            Console.WriteLine($"id:{chat_session.id}title:{chat_session.title} question:{msg} response:{response} use time:{(stopwatch.ElapsedMilliseconds / 1000f)}");
            return JsonConvert.SerializeObject(new { uid = chat_session.id, title = chat_session.title, chats = result });
        }

        [HttpGet]
        public string Chats()
        {
            var sessions = ChatChannel.GetChatList();
            if (sessions == null || sessions.Count == 0)
            {
                return "[]";
            }
            List<object> result = new List<object>();
            for (var i = 0; i < sessions.Count; i++)
            {
                var simple = sessions[i].chats?.LastOrDefault();
                result.Add(new
                {
                    uid = sessions[i].id,
                    sessions[i].title,
                    info = simple?.content?.Length > 200 ? simple?.content?[..200] : simple?.content
                });
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpGet("session/{session}")]
        public string GetSessionData(string session)
        {
            ChatChannel gPTSession = ChatChannel.FindSession(session);
            if (gPTSession == null)
            {
                return string.Empty;
            }
            List<object> result = new List<object>();
            for (var i = 0; i < gPTSession.chats.Count; i++)
            {
                result.Add(new
                {
                    role = gPTSession.chats[i].role == "user" ? "Me" : "Open AI",
                    gPTSession.chats[i].content
                });
            }
            return JsonConvert.SerializeObject(new { uid = gPTSession.id, title = gPTSession.title, chats = result });
        }
    }
}
