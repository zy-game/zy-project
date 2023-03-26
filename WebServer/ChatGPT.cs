using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace WebServer
{
    public class SessionDatable
    {
        public string _id;
        public string title;
        public List<Role> chats;
    }
    public class Role
    {
        public string role;
        public string content;
    }
    public class GPTSession : IDisposable
    {

        public string id { get; private set; }
        public string title { get; private set; }
        public List<Role> chats { get; private set; }
        private HttpClient client = new HttpClient();
        private string apiKey = "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";
        private static List<GPTSession> sessions = new List<GPTSession>();
        public static GPTSession AddSession(SessionDatable datable)
        {
            GPTSession session = new GPTSession() { id = datable._id, title = datable.title, chats = datable.chats };
            sessions.Add(session);
            return session;
        }

        public static GPTSession FindSession(string id)
        {
            return sessions.Find(x => x.id == id);
        }

        public static void RemoveSession(string id)
        {
            GPTSession session = FindSession(id);
            if (session == null)
            {
                return;
            }
            Mongo.DeleteDocument<SessionDatable>(Builders<SessionDatable>.Filter.Eq("_id", session.id));
        }
        public async Task<string> Question(string text)
        {
            string result = string.Empty;
            chats.Add(new Role() { role = "user", content = text });
            if (chats.Count > 4)
            {
                chats.Remove(chats.First());
            }
            var message = new
            {
                model = "gpt-3.5-turbo",
                temperature = 1f,
                messages = chats
            };
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
                Headers = { { "Authorization", $"Bearer {apiKey}" } },
                Content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
            };
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            return await reader.ReadToEndAsync();
                        }
                        var resultBuilder = new StringBuilder();
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (line.StartsWith("data: [DONE]") || line.Length < 5)
                            {
                                continue;
                            }
                            try
                            {
                                ChatResponse chatResponse = JsonConvert.DeserializeObject<ChatResponse>(line);
                                if (chatResponse.choices?.Count > 0)
                                {
                                    resultBuilder.Append(chatResponse.choices[0].message.content);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                        result = resultBuilder.ToString();
                    }
                }
            }
            chats.Add(new Role() { role = "assistant", content = result });
            return result;
        }

        public void Dispose()
        {
            Mongo.UpsertDocument<SessionDatable>(Builders<SessionDatable>.Filter.Eq("_id", id), new SessionDatable() { _id = id, title = title, chats = chats });
        }
    }
    [Route("api/chat-gpt")]
    //[Authorize]
    [ApiController]
    public class ChatGPT : ControllerBase
    {
        private string apiKey = "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";
        private static HttpClient client = new HttpClient();

        [HttpGet("list")]
        public string Chats()
        {
            List<SessionDatable> sessions = Mongo.Where<SessionDatable>(Builders<SessionDatable>.Filter.Empty);
            if (sessions == null || sessions.Count == 0)
            {
                return "{}";
            }
            List<object> result = new List<object>();
            string resu = string.Empty;
            for (var i = 0; i < sessions.Count; i++)
            {
                var session = GPTSession.AddSession(sessions[i]);
                resu += "<li class=\"layui-timeline-item\">";
                resu += "<i class=\"layui-icon layui-timeline-axis\"></i>";
                resu += "<div class=\"layui-timeline-content layui-text\">";
                resu += $"<a id=\" {session.id} href = \"javascript:;\" οnclick = \"onInfo({session.id})\">";
                resu += $"<h3 class=\"layui-timeline-title\">{session.title}</h3>";
                resu += "</a>";
                resu += $"<p>{session.chats?.Last()?.content?[..200]}</p>";
                resu += "<ul>";
                resu += "</ul>";
                resu += "</div>";
                resu += "</li>";
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(result);
        }

        [HttpGet("code/{msg}")]
        public async Task<string> Get(string msg)
        {
            int token = 100;
            async Task<string> GetRespnse()
            {
                string result = string.Empty;
                var message2 = new
                {
                    model = "text-davinci-003",
                    prompt = msg,
                    temperature = 1,
                    max_tokens = token,
                    stream = true,
                };
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/completions");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(message2), Encoding.UTF8, "application/json");
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(responseStream))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return await reader.ReadToEndAsync();
                    }
                    var resultBuilder = new StringBuilder();
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line.StartsWith("data: [DONE]") || line.Length < 5)
                        {
                            break;
                        }
                        try
                        {
                            string temp = line.Substring(5);
                            Completions completions = Newtonsoft.Json.JsonConvert.DeserializeObject<Completions>(temp);
                            if (completions.choices != null && completions.choices.Count > 0)
                            {
                                resultBuilder.Append(completions.choices[0].text);
                            }
                            if (completions.choices[0].finish_reason != null && completions.choices[0].finish_reason == "length")
                            {
                                resultBuilder.Clear();
                                token += 100;
                                return await GetRespnse();
                            }
                        }
                        catch (Exception e)
                        {
                            // Handle exception.
                        }
                    }
                    result = resultBuilder.ToString();
                }
                return result;
            }
            return await GetRespnse();
        }

        [HttpGet("chat/{msg}")]
        public async Task<string> ChatCompletions(string msg)
        {
            string session_id = string.Empty;
            if (Request.Headers.ContainsKey("session_id"))
            {
                session_id = Request.Headers["session_id"];
            }
            GPTSession session = GPTSession.FindSession(session_id);
            if (session == null)
            {
                session = GPTSession.AddSession(new SessionDatable() { title = msg[..20], chats = new List<Role>() });
            }
            return await session.Question(msg);
        }
    }
    public class Completions
    {
        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string @object { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int created { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<Choices> choices { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string model { get; set; }
    }

    public class Choices
    {
        /// <summary>
        /// 
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int index { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string logprobs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string finish_reason { get; set; }
    }

    public class Message
    {
        /// <summary>
        /// 
        /// </summary>
        public string role { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string content { get; set; }
    }

    public class ChoicesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int index { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Message message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string finish_reason { get; set; }
    }

    public class Usage
    {
        /// <summary>
        /// 
        /// </summary>
        public int prompt_tokens { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int completion_tokens { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int total_tokens { get; set; }
    }

    public class ChatResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string @object { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int created { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ChoicesItem> choices { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Usage usage { get; set; }
    }
}
