using Newtonsoft.Json;
using System.Text;
using ZyGame;
using ZyGame.Service;

class ChatService : IHttpSocketService
{
    private HttpClient client = new HttpClient();
    private DBContext<ChatSessionData> db_chat = new DBContext<ChatSessionData>("mongodb://140.143.97.63:27017/x_project");
    public async Task<object> Executedrequest(string path, byte[] bytes)
    {
        switch (path)
        {
            case ChatApi.session:
                return await GetchatSessionList(bytes);
            case ChatApi.question:
                return await GetQuestionResponse(bytes);
        }
        return default;
    }

    private Task<object> GetchatSessionList(byte[] bytes)
    {
        ZyGame.Queryable query = bytes.TryToObject<ZyGame.Queryable>();
        int uid = query.GetField<int>("uid");
        return Task.FromResult<object>(db_chat.Entities.Where(x => x.uid == uid).ToList());
    }

    private async Task<object> GetQuestionResponse(byte[] bytes)
    {
        Question question = bytes.TryToObject<Question>();
        ChatSessionData chatSession = db_chat.Entities.Where(x => x.uid == question.id).FirstOrDefault();
        if (chatSession is null)
        {
            db_chat.Entities.Add(chatSession = new ChatSessionData() { chats = new List<ChatData>() });
        }
        chatSession.chats.Add(new ChatData() { role = "user", content = question.message });
        var temp = chatSession.chats.Count > 2 ? chatSession.chats.GetRange(chatSession.chats.Count - 2, 2) : chatSession.chats;
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
            Headers = { { "Authorization", $"Bearer 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" } },
            Content = new StringContent(JsonConvert.SerializeObject(new { model = "gpt-3.5-turbo", temperature = 1f, messages = temp }), Encoding.UTF8, "application/json")
        };
        ChatData result = new ChatData() { role = "assistant" };
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
                    result.content = resultBuilder.ToString();
                }
            }
        }
        chatSession.chats.Add(result);
        await db_chat.SaveChangesAsync();
        return result;
    }


    class ChatSessionData : DBEntity
    {
        public int uid { get; set; }
        public string name { get; set; }
        public List<ChatData> chats { get; set; }
    }

    class ChatData
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    class Question
    {
        public int id;
        public string message;
    }

    class ChatApi
    {
        public const string session = "/chat/sessions";
        public const string question = "/chat/question";
    }
}