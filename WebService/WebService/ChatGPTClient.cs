using Newtonsoft.Json.Linq;
using System.Text;

public class ChatGPTClient
{
    private const string endpoint = "https://api.openai.com/v1/engines/text-moderation-playground/completions";

    private string apiKey = "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";

    private string sessionId = string.Empty;
    private HttpClient client;
    private int maxTokens = 200;
    public static ChatGPTClient instance { get; } = new ChatGPTClient();
    public ChatGPTClient()
    {
        client = new HttpClient();
    }
    public async Task<string> GenerateTextAsync(string prompt)
    {
        string result = string.Empty;
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.openai.com/v1/completions"),
            Headers =
            {
                { "Authorization", $"Bearer {apiKey}" },
            },

            Content = new StringContent("{\"model\":\"text-davinci-003\",\"prompt\":\"" + prompt + "\",\"max_tokens\":200,\"temperature\":0.75,\"top_p\":1,\"stream\":true,\"logprobs\":null,\"stop\": \"###\"}", Encoding.UTF8, "application/json")
        };

        // 3. 发送请求并获取响应流
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        var responseStream = await response.Content.ReadAsStreamAsync();

        // 4. 处理响应流
        var reader = new StreamReader(responseStream);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line.Length > 5)
            {
                line = line.Substring(5, line.Length - 5);
                try
                {
                    var json = JObject.Parse(line);

                    Console.WriteLine(line);
                    if (json != null)
                    {
                        result += json["choices"][0]["text"];
                    }
                }
                catch
                {

                }
            }
        }
        return result;
    }
}
