using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DesktopPal;
using Xunit;

namespace DesktopPal.Tests
{
    /// <summary>
    /// Unit tests for <see cref="AIService"/> using a stub <see cref="HttpMessageHandler"/>
    /// to avoid requiring a live LM Studio instance.
    /// </summary>
    public class AIServiceTests
    {
        // ── Fake HTTP handler ────────────────────────────────────────────────────

        /// <summary>
        /// A stub <see cref="HttpMessageHandler"/> that always returns a preset response.
        /// </summary>
        private sealed class FakeHttpHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public FakeHttpHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_response);
        }

        /// <summary>
        /// Builds a minimal OpenAI-compatible JSON response payload.
        /// </summary>
        private static HttpResponseMessage OkResponse(string content) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $$"""
                    {
                      "choices": [
                        {
                          "message": {
                            "role": "assistant",
                            "content": "{{content}}"
                          }
                        }
                      ]
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            };

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates an <see cref="AIService"/> that uses <paramref name="handler"/>
        /// for HTTP calls.
        /// </summary>
        private static AIService BuildService(HttpMessageHandler handler)
        {
            // AIService uses a static HttpClient, so we use reflection to swap the handler
            // for testing purposes.  In production code a factory/DI approach would be used.
            var state = new PetState { Name = "TestPal", Level = 1 };

            // Temporarily replace the private static HttpClient field.
            var field = typeof(AIService).GetField(
                "_httpClient",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            if (field is null)
                throw new InvalidOperationException(
                    "Could not find '_httpClient' field on AIService. " +
                    "Ensure the field name matches.");

            // Save original and swap.
            var original = (HttpClient)field.GetValue(null)!;
            field.SetValue(null, new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) });

            // Return service and a cleanup action.
            var service = new AIService(state);

            // Restore original after test (best-effort – tests should be isolated).
            field.SetValue(null, original);

            return service;
        }

        // ── Tests ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ChatAsync_ValidResponse_ReturnsAIReply()
        {
            var state = new PetState { Name = "TestPal", Level = 1 };
            var handler = new FakeHttpHandler(OkResponse("Hello there!"));

            // Use reflection to inject the fake handler.
            var httpClientField = typeof(AIService).GetField(
                "_httpClient",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

            var original = (HttpClient)httpClientField.GetValue(null)!;
            httpClientField.SetValue(null, new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) });

            try
            {
                var service = new AIService(state);
                string reply = await service.ChatAsync("Hi!");
                Assert.Equal("Hello there!", reply);
            }
            finally
            {
                httpClientField.SetValue(null, original);
            }
        }

        [Fact]
        public async Task ChatAsync_EmptyInput_ReturnsEmptyString()
        {
            var state = new PetState { Name = "TestPal" };
            var service = new AIService(state); // no HTTP needed for empty input

            string reply = await service.ChatAsync(string.Empty);
            Assert.Equal(string.Empty, reply);
        }

        [Fact]
        public async Task ChatAsync_WhitespaceInput_ReturnsEmptyString()
        {
            var state = new PetState { Name = "TestPal" };
            var service = new AIService(state);

            string reply = await service.ChatAsync("   ");
            Assert.Equal(string.Empty, reply);
        }

        [Fact]
        public async Task ChatAsync_HttpError_ReturnsFriendlyFallback()
        {
            var state = new PetState { Name = "TestPal" };

            var errorHandler = new FakeHttpHandler(
                new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var httpClientField = typeof(AIService).GetField(
                "_httpClient",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

            var original = (HttpClient)httpClientField.GetValue(null)!;
            httpClientField.SetValue(null, new HttpClient(errorHandler) { Timeout = TimeSpan.FromSeconds(5) });

            try
            {
                var service = new AIService(state);
                string reply = await service.ChatAsync("Hello?");
                // Should not throw; should return a user-friendly fallback message.
                Assert.False(string.IsNullOrEmpty(reply));
                Assert.Contains("LM Studio", reply, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                httpClientField.SetValue(null, original);
            }
        }

        [Fact]
        public async Task ChatAsync_CancellationRequested_ReturnsFallback()
        {
            var state = new PetState { Name = "TestPal" };

            // Handler that throws TaskCancelledException to simulate timeout.
            var cancellingHandler = new CancellingHttpHandler();

            var httpClientField = typeof(AIService).GetField(
                "_httpClient",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

            var original = (HttpClient)httpClientField.GetValue(null)!;
            httpClientField.SetValue(null, new HttpClient(cancellingHandler) { Timeout = TimeSpan.FromSeconds(5) });

            try
            {
                var service = new AIService(state);
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                string reply = await service.ChatAsync("Hello?", cts.Token);
                Assert.False(string.IsNullOrEmpty(reply));
            }
            finally
            {
                httpClientField.SetValue(null, original);
            }
        }

        [Fact]
        public void RebuildSystemPrompt_UpdatesSystemMessage()
        {
            var state = new PetState { Name = "Fluffy", Level = 3, Hunger = 80, Happiness = 90 };
            var service = new AIService(state);

            // Change state and rebuild.
            state.Name = "Sparkle";
            service.RebuildSystemPrompt();

            // Access _messages via reflection to verify the system prompt changed.
            var msgField = typeof(AIService).GetField(
                "_messages",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var messages = (System.Collections.Generic.List<object>)msgField.GetValue(service)!;
            Assert.NotEmpty(messages);

            // First message is the system prompt; check it references the new name.
            string json = System.Text.Json.JsonSerializer.Serialize(messages[0]);
            Assert.Contains("Sparkle", json);
        }

        // ── Helper types ─────────────────────────────────────────────────────────

        private sealed class CancellingHttpHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new OperationCanceledException(cancellationToken);
            }
        }
    }
}
