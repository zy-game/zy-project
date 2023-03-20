using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServer
{
    [Route("api/chat-gpt")]
    [Authorize]
    [ApiController]
    public class ChatGPT : ControllerBase
    {
        private string apiKey = "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";
        private static HttpClient client = new HttpClient();

        public List<object> Chats()
        { 
            Mongo.
        }



        [HttpGet("code/{msg}")]
        public async Task<string> Get(string msg)
        {
            return await GetCompletionResponse(msg, 100, 1);
        }

        async Task<string> GetCompletionResponse(string msg, int token, float temperature)
        {
            string result = string.Empty;
            var message2 = new
            {
                model = "text-davinci-003",
                prompt = msg,
                temperature = 1,
                max_tokens = token,
                top_p = 1,
                n = 1,
                stream = true,
                stop = "######"
            };
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.openai.com/v1/completions"),
                Headers =
                {
                    { "Authorization", $"Bearer {apiKey}" },
                },

                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(message2), Encoding.UTF8, "application/json")
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
                        while (!reader.EndOfStream)
                        {
                            line = await reader.ReadLineAsync();
                            if (line.StartsWith("data: [DONE]"))
                            {
                                break;
                            }
                            if (line.Length < 5)
                            {
                                continue;
                            }
                            try
                            {
                                Console.WriteLine(line);
                                string temp = line.Substring(5);
                                Completions completions = Newtonsoft.Json.JsonConvert.DeserializeObject<Completions>(temp);
                                if (completions.choices != null && completions.choices.Count > 0)
                                {
                                    resultBuilder.Append(completions.choices[0].text);
                                }
                                if (completions.choices[0].finish_reason != null && completions.choices[0].finish_reason == "length")
                                {
                                    resultBuilder.Clear();
                                    return await GetCompletionResponse(msg, token + 100, temperature);
                                }
                            }
                            catch (Exception e)
                            {

                            }
                        }

                        result = resultBuilder.ToString();
                    }

                }
            }
            return result;
        }

        public static List<string> history = new List<string>();

        [HttpGet("chat/{msg}")]
        public async Task<string> ChatCompletions(string msg)
        {
            string result = string.Empty;

            var message = new
            {
                model = "gpt-3.5-turbo",
                temperature = 1f,
                messages = new List<object>()
            };
            message.messages.Add(new { role = "system", content = "你是一个有用的助手" });
            message.messages.Add(new { role = "assistant", content = "好的" });
            history.Add(msg);
            for (int i = 0; i < history.Count; i++)
            {
                if (i % 2 == 0)
                {
                    message.messages.Add(new { role = "user", content = history[i] });
                }
                else
                {
                    message.messages.Add(new { role = "assistant", content = history[i] });
                }
            }
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(message));
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
                Headers =
                {
                    { "Authorization", $"Bearer {apiKey}" },
                },
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
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
                        while (!reader.EndOfStream)
                        {
                            line = await reader.ReadLineAsync();
                            if (line.StartsWith("data: [DONE]"))
                            {
                                break;
                            }
                            if (line.Length < 5)
                            {
                                continue;
                            }
                            if (line.Length < 5)
                            {
                                continue;
                            }
                            try
                            {
                                ChatResponse chatResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatResponse>(line);
                                if (chatResponse.choices != null && chatResponse.choices.Count > 0)
                                {
                                    resultBuilder.Append(chatResponse.choices[0].message.content);
                                }
                            }
                            catch (Exception e)
                            {

                            }
                        }
                        result = resultBuilder.ToString();
                    }

                }
            }

            history.Add(result);

            return result;
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
}
