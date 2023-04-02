using MongoDB.Driver;
using Newtonsoft.Json;
using ServerFramework;
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
    public class ChatChannel : IDisposable
    {
        private static List<ChatChannel> __sessions;

        private static void EnsureInitializedSessionList()
        {
            if (__sessions != null)
            {
                return;
            }
            __sessions = new List<ChatChannel>();
            List<DBSession> sessions = Server.DBService.Where(Builders<DBSession>.Filter.Where(_ => true));
            if (sessions == null)
            {
                return;
            }
            for (int i = 0; i < sessions.Count; i++)
            {
                __sessions.Add(new ChatChannel() { id = sessions[i]._id.ToString(), title = sessions[i].title, chats = JsonConvert.DeserializeObject<List<Role>>(sessions[i].chats) });
            }
        }

        public static ChatChannel AddSession(string msg)
        {
            EnsureInitializedSessionList();
            var datable = new DBSession() { title = msg.Length > 20 ? msg[..20] : msg, chats = "[]" };
            if (__sessions != null && __sessions.Count > 10)
            {
                ChatChannel channel = __sessions.FirstOrDefault();
                if (channel != null)
                {
                    Server.DBService.Delete(Builders<DBSession>.Filter.Eq("_id", channel.id));
                    __sessions.Remove(channel);
                }
            }
            Server.DBService.Insert(datable);
            ChatChannel session = new ChatChannel() { id = datable._id.ToString(), title = datable.title, chats = new List<Role>() };
            __sessions.Add(session);
            return session;
        }

        public static ChatChannel FindSession(string id)
        {
            EnsureInitializedSessionList();
            if (string.IsNullOrEmpty(id))
            {
                return default;
            }
            return __sessions.Find(x => x.id == id);
        }

        public static List<ChatChannel> GetChatList()
        {
            EnsureInitializedSessionList();
            return __sessions;
        }



        public string id { get; private set; }
        public string title { get; private set; }
        public List<Role> chats { get; private set; }
        private HttpClient client = new HttpClient();
        private string apiKey = "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";

        public async Task<string> Question(string text)
        {
            string result = string.Empty;
            chats.Add(new Role() { role = "user", content = text });
            var temp = chats.Count > 2 ? chats.GetRange(chats.Count - 2, 2) : chats;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
                Headers = { { "Authorization", $"Bearer {apiKey}" } },
                Content = new StringContent(JsonConvert.SerializeObject(new { model = "gpt-3.5-turbo", temperature = 1f, messages = temp }), Encoding.UTF8, "application/json")
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
            List<Role> roles = new List<Role>();
            if (chats.Count > 4)
            {
                roles.AddRange(chats.GetRange(chats.Count - 4, 4));
            }
            else
            {
                roles.AddRange(chats);
            }
            var findFilter = Builders<DBSession>.Filter.Eq("title", title);
            var updateFilter = Builders<DBSession>.Update.Set("chats", JsonConvert.SerializeObject(roles));
            Server.DBService.Update(findFilter, updateFilter);
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
                            Console.WriteLine(e);
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
        }
    }
}
