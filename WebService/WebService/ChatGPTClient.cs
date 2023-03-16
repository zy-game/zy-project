using Newtonsoft.Json.Linq;
using System.Text;

public class ChatGPTClient
{
    private const string endpoint = "https://api.openai.com/v1/engines/text-moderation-playground/completions";

    private string apiKey = "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";

    private string sessionId = string.Empty;
    private HttpClient client;
    private int maxTokens = 50;
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
            Content = new StringContent($"{{\"prompt\": \"{prompt}\",\"model\": \"text-davinci-003\", \"max_tokens\": {maxTokens},  \"stream\": true}}", Encoding.UTF8, "application/json")
        };

        // 3. 发送请求并获取响应流
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        var responseStream = await response.Content.ReadAsStreamAsync();

        // 4. 处理响应流
        var reader = new StreamReader(responseStream);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();

            Console.WriteLine(line);
            result += line;
        }
        return result;
    }
}
