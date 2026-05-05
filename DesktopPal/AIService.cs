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
    /// LM Studio connection status surfaced to the UI.
    /// </summary>
    public enum AIServiceStatus
    {
        /// <summary>Not yet contacted; treat as healthy until proven otherwise.</summary>
        Unknown,
        /// <summary>Last call succeeded.</summary>
        Available,
        /// <summary>Last call failed due to network / timeout / server unreachable.</summary>
        Unavailable,
        /// <summary>Last call returned a malformed or non-success response.</summary>
        Error,
    }

    public class AIService
    {
        private const string LogSource = "AIService";
        private const string BaseUrl = "http://localhost:1234/v1/chat/completions";
        private const string ModelName = "google/gemma-3-4b";
        private const int RequestTimeoutSeconds = 10;
        private const int MaxHistoryMessages = 10;

        // Single shared HttpClient with a *short* per-request timeout applied via CancellationToken.
        // (HttpClient.Timeout is not used because we want per-call cancellation + observable status.)
        private static readonly HttpClient _client = new HttpClient
        {
            // Defensive default in case a caller forgets to pass a token.
            Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds + 5)
        };

        private readonly List<object> _messages = new List<object>();
        private readonly PetState _state;
        private AIServiceStatus _status = AIServiceStatus.Unknown;

        /// <summary>Raised on the calling thread when <see cref="Status"/> changes.</summary>
        public event Action<AIServiceStatus>? StatusChanged;

        public AIServiceStatus Status => _status;

        /// <summary>Human-readable detail about the last failure, if any.</summary>
        public string? LastErrorMessage { get; private set; }

        public AIService(PetState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            string systemPrompt = $"You are a digital pet named {state.Name}. " +
                                  $"You are currently at level {state.Level}. " +
                                  $"Your hunger is {state.Hunger:F0}%, happiness is {state.Happiness:F0}%. " +
                                  "You live on the user's desktop. Be cute, short-sentenced, and reactive. " +
                                  "You don't know much yet but you are growing.";

            _messages.Add(new { role = "system", content = systemPrompt });
        }

        public async Task<string> ChatAsync(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return OfflineBrain.GetRandomPhrase("Idle");
            }

            _messages.Add(new { role = "user", content = userInput });

            var requestBody = new
            {
                model = ModelName,
                messages = _messages,
                temperature = 0.7,
                max_tokens = 100
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(RequestTimeoutSeconds));

            try
            {
                string json = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await _client.PostAsync(BaseUrl, content, cts.Token).ConfigureAwait(true);
                if (!response.IsSuccessStatusCode)
                {
                    string detail = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
                    return HandleFailure(AIServiceStatus.Error, detail, null);
                }

                string responseString = await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(true);
                using var doc = JsonDocument.Parse(responseString);

                string? reply = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(reply))
                {
                    return HandleFailure(AIServiceStatus.Error, "Empty completion content.", null);
                }

                _messages.Add(new { role = "assistant", content = reply });

                // Keep history manageable — preserve the system prompt at index 0.
                if (_messages.Count > MaxHistoryMessages) _messages.RemoveAt(1);

                SetStatus(AIServiceStatus.Available, null);
                return reply;
            }
            catch (OperationCanceledException ex)
            {
                return HandleFailure(AIServiceStatus.Unavailable,
                    $"Request timed out after {RequestTimeoutSeconds}s.", ex);
            }
            catch (HttpRequestException ex)
            {
                return HandleFailure(AIServiceStatus.Unavailable, "LM Studio is unreachable.", ex);
            }
            catch (JsonException ex)
            {
                return HandleFailure(AIServiceStatus.Error, "Malformed response from LM Studio.", ex);
            }
            catch (Exception ex)
            {
                return HandleFailure(AIServiceStatus.Error, "Unexpected AI failure.", ex);
            }
        }

        private string HandleFailure(AIServiceStatus newStatus, string detail, Exception? ex)
        {
            // Drop the user message we optimistically appended so retries don't accumulate dangling turns.
            if (_messages.Count > 1) _messages.RemoveAt(_messages.Count - 1);

            Logging.Warn(LogSource, detail, ex);
            SetStatus(newStatus, detail);

            // Degraded mode: hand back an OfflineBrain phrase rather than a raw error string.
            return OfflineBrain.GetRandomPhrase("Idle");
        }

        private void SetStatus(AIServiceStatus status, string? detail)
        {
            LastErrorMessage = detail;
            if (_status == status) return;
            _status = status;
            try
            {
                StatusChanged?.Invoke(status);
            }
            catch (Exception ex)
            {
                Logging.Warn(LogSource, "StatusChanged handler threw.", ex);
            }
        }
    }
}
