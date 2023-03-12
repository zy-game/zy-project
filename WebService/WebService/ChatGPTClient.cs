using Newtonsoft.Json.Linq;
using System.Text;

public class ChatGPTClient
{
    private const string endpoint = "https://api.openai.com/v1/engines/text-moderation-playground/completions";

    private readonly string apiKey;

    public ChatGPTClient(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<string> GenerateTextAsync(string prompt)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            string reqult = prompt;
            while (true)
            {
                var requestBody = new
                {
                    prompt = reqult,
                    temperature = 0.7,
                    max_tokens = 50,
                    top_p = 1,
                    frequency_penalty = 0,
                    presence_penalty = 0,
                    stop = "##END##"
                };
                var requestContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, requestContent, CancellationToken.None);
                if (!response.IsSuccessStatusCode)
                {
                    return "error:" + response.StatusCode.ToString();
                }
                string responseContent = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseContent);
                reqult += json["choices"][0]["text"];
                //                  reqult += responseContent;
                //                  {
                //                      "id": "cmpl-uqkvlQyYK7bGYrRHQ0eXlWi7",
                //"object": "text_completion",
                //"created": 1589478378,
                //"model": "text-davinci-003",
                //"choices": [
                //  {
                //                          "text": "\n\nThis is indeed a test",
                //    "index": 0,
                //    "logprobs": null,
                //    "finish_reason": "length"
                //  }
                //],
                //"usage": {
                //                          "prompt_tokens": 5,
                //  "completion_tokens": 7,
                //  "total_tokens": 12
                //}
                //                  }
                // Check for termination condition here, e.g. a specific response from the API
                string state = json["choices"][0]["finish_reason"].ToString();
                if (state == "stop")
                {
                    break;
                }
                Thread.Sleep(100);
            }
            return reqult;
        }
    }
}