using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopPal
{
    /// <summary>
    /// Manages communication with the local LM Studio server (OpenAI-compatible API).
    /// Maintains a rolling short-term chat history and injects a dynamic system prompt.
    /// </summary>
    public class AIService
    {
        // ── Constants ────────────────────────────────────────────────────────────
        private const string BaseUrl = "http://localhost:1234/v1/chat/completions";
        private const int MaxHistoryMessages = 20; // excludes system prompt
        private const int RequestTimeoutSeconds = 30;

        // ── Fields ───────────────────────────────────────────────────────────────
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds)
        };

        private readonly List<object> _messages = new List<object>();
        private readonly PetState _state;

        // ── Constructor ──────────────────────────────────────────────────────────
        public AIService(PetState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            RebuildSystemPrompt();
            DebugLogger.Info($"AIService initialised for pet '{state.Name}' using model '{state.ModelName}'.");
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Sends <paramref name="userInput"/> to the LM Studio server and returns the
        /// model's reply. Returns a friendly fallback string on any error.
        /// </summary>
        public async Task<string> ChatAsync(string userInput, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                DebugLogger.Warning("ChatAsync called with empty input – skipping.");
                return string.Empty;
            }

            _messages.Add(new { role = "user", content = userInput });
            DebugLogger.Debug($"Sending user message ({userInput.Length} chars) to AI.");

            var requestBody = new
            {
                model = _state.ModelName,
                messages = _messages,
                temperature = 0.7,
                max_tokens = 150
            };

            try
            {
                string json = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await _httpClient.PostAsync(BaseUrl, httpContent, cancellationToken)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                string responseString = await response.Content.ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                string? reply = ParseReply(responseString);
                if (reply is null)
                {
                    DebugLogger.Warning("Received a response but could not parse the reply content.");
                    TrimHistory();
                    return "*confused* (I couldn't understand that response.)";
                }

                _messages.Add(new { role = "assistant", content = reply });
                TrimHistory();

                DebugLogger.Debug($"AI replied ({reply.Length} chars).");
                return reply;
            }
            catch (OperationCanceledException)
            {
                DebugLogger.Warning("ChatAsync was cancelled.");
                TrimHistory();
                return "*distracted* (Response timed out.)";
            }
            catch (HttpRequestException ex)
            {
                DebugLogger.Error("HTTP request to LM Studio failed.", ex);
                TrimHistory();
                return $"*is busy* (Cannot reach LM Studio. Make sure it is running on port 1234. Detail: {ex.Message})";
            }
            catch (JsonException ex)
            {
                DebugLogger.Error("Failed to parse LM Studio JSON response.", ex);
                TrimHistory();
                return "*confused* (Got a strange response from AI.)";
            }
            catch (Exception ex)
            {
                DebugLogger.Error("Unexpected error in ChatAsync.", ex);
                TrimHistory();
                return $"*is busy* (Unexpected error: {ex.Message})";
            }
        }

        /// <summary>
        /// Rebuilds the system prompt from the current <see cref="PetState"/>.
        /// Call after significant stat changes or a level-up.
        /// </summary>
        public void RebuildSystemPrompt()
        {
            // Replace existing system message if present, otherwise prepend.
            if (_messages.Count > 0)
                _messages.RemoveAt(0);

            string systemPrompt =
                $"You are a digital pet named {_state.Name}. " +
                $"You are currently at level {_state.Level}. " +
                $"Your hunger is {_state.Hunger:F0}% and happiness is {_state.Happiness:F0}%. " +
                "You live on the user's desktop. Be cute, short-sentenced, and reactive. " +
                "Never break character. Keep responses under two sentences.";

            _messages.Insert(0, new { role = "system", content = systemPrompt });
            DebugLogger.Debug("System prompt rebuilt.");
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private static string? ParseReply(string responseString)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseString);
                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }
            catch (Exception ex)
            {
                DebugLogger.Error("ParseReply failed.", ex);
                return null;
            }
        }

        /// <summary>
        /// Keeps the conversation history within <see cref="MaxHistoryMessages"/>
        /// by removing the oldest user/assistant pair (indices 1 and 2) while
        /// always preserving the system prompt at index 0.
        /// </summary>
        private void TrimHistory()
        {
            // _messages[0] is always the system prompt.
            while (_messages.Count > MaxHistoryMessages + 1)
            {
                _messages.RemoveAt(1); // oldest non-system message
            }
        }
    }
}

