using System.Text;
using System.Text.Json;

namespace PPL3_Banhangonline.Service
{
    public class AIService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public AIService(IHttpClientFactory factory, IConfiguration config)
        {
            _factory = factory;
            _config = config;
        }

        public async Task<string> AskGeminiAsync(string message)
        {
            var client = _factory.CreateClient();
            var apiKey = _config["Gemini:ApiKey"];

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

            var prompt = $"""
            Bạn là chatbot hỗ trợ cho website bán hàng nông sản.
            Hãy trả lời ngắn gọn, dễ hiểu, thân thiện.
            Câu hỏi của khách: {message}
            """;

            var body = new
            {
                contents = new object[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync(
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent",
                content
            );

            var result = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                return $"Lỗi AI: {result}";
            }

            using var doc = JsonDocument.Parse(result);

            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "Không có phản hồi";
        }
    }
}