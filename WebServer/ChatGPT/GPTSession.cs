using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WebServer.DB;

namespace WebServer.ChatGPT
{
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
        private static List<GPTSession> __sessions = new List<GPTSession>();
        public static GPTSession AddSession(string msg)
        {
            var datable = new ChatSession() { title = msg.Length > 20 ? msg[..20] : msg, chats = "[]" };
            if (__sessions.Count > 0)
            {
                List<ChatSession> sessions = Mongo.Where<ChatSession>(Builders<ChatSession>.Filter.Where(_ => true));
                if (sessions != null && sessions.Count > 0)
                {
                    Mongo.Delete(Builders<ChatSession>.Filter.Eq("_id", sessions[0]._id.ToString()));
                    __sessions.Remove(__sessions.Find(x => x.id == sessions[0]._id.ToString()));
                }
            }
            Mongo.Add(datable);
            GPTSession session = new GPTSession() { id = datable._id.ToString(), title = datable.title, chats = new List<Role>() };
            __sessions.Add(session);
            return session;
        }

        public static GPTSession FindSession(string id)
        {
            return __sessions.Find(x => x.id == id);
        }

        public static List<GPTSession> GetChatList()
        {
            List<ChatSession> sessions = Mongo.Where(Builders<ChatSession>.Filter.Where(_ => true));
            if (sessions == null)
            {
                return new List<GPTSession>();
            }
            for (int i = 0; i < sessions.Count; i++)
            {
                if (__sessions.Find(x => x.id == sessions[i]._id.ToString()) != null)
                {
                    continue;
                }
                __sessions.Add(new GPTSession() { id = sessions[i]._id.ToString(), title = sessions[i].title, chats = JsonConvert.DeserializeObject<List<Role>>(sessions[i].chats) });
            }
            return __sessions;
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
                                dynamic chatResponse = JsonConvert.DeserializeObject<dynamic>(line);
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
            Mongo.Update(Builders<ChatSession>.Filter.Eq("title", title), Builders<ChatSession>.Update.Set("chats", JsonConvert.SerializeObject(chats)));
            return result;
        }
        public async Task<string> GetCodeCompletion(string msg)
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
                request.Content = new StringContent(JsonConvert.SerializeObject(message2), Encoding.UTF8, "application/json");
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
                            dynamic completions = JsonConvert.DeserializeObject<dynamic>(temp);
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
        public void Dispose()
        {
            Mongo.Update(Builders<ChatSession>.Filter.Eq("_id", id), Builders<ChatSession>.Update.Set("chats", chats));
        }
    }

    //public class Completions
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string id { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string @object { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int created { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<Choices> choices { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string model { get; set; }
    //}

    //public class Choices
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string text { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int index { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string logprobs { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string finish_reason { get; set; }
    //}

    //public class Message
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string role { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string content { get; set; }
    //}

    //public class ChoicesItem
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int index { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public Message message { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string finish_reason { get; set; }
    //}

    //public class Usage
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int prompt_tokens { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int completion_tokens { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int total_tokens { get; set; }
    //}

    //public class ChatResponse
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string id { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string @object { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int created { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<ChoicesItem> choices { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public Usage usage { get; set; }
    //}
}
