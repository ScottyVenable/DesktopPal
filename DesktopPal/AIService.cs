using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DesktopPal
{
    public class AIService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BaseUrl = "http://localhost:1234/v1/chat/completions";
        private List<object> _messages = new List<object>();
        private PetState _state;

        public AIService(PetState state)
        {
            _state = state;
            string systemPrompt = $"You are a digital pet named {state.Name}. " +
                                  $"You are currently at level {state.Level}. " +
                                  $"Your hunger is {state.Hunger:F0}%, happiness is {state.Happiness:F0}%. " +
                                  "You live on the user's desktop. Be cute, short-sentenced, and reactive. " +
                                  "You don't know much yet but you are growing.";
            
            _messages.Add(new { role = "system", content = systemPrompt });
        }

        public async Task<string> ChatAsync(string userInput)
        {
            _messages.Add(new { role = "user", content = userInput });

            var requestBody = new
            {
                model = _state.ModelName,
                messages = _messages,
                temperature = 0.7,
                max_tokens = 100
            };

            try
            {
                string json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync(BaseUrl, content);
                response.EnsureSuccessStatusCode();
                
                string responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                string reply = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                
                _messages.Add(new { role = "assistant", content = reply });
                
                // Keep history manageable
                if (_messages.Count > 10) _messages.RemoveAt(1);

                return reply;
            }
            catch (Exception ex)
            {
                return $"*is busy* (Error: {ex.Message}. Make sure LM Studio is running on port 1234)";
            }
        }
    }
}
