using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ST2_2026_Team_2_JobTracker.DesignPatterns.Factory;
using ST2_2026_Team_2_JobTracker.DesignPatterns.Strategy;
using ST2_2026_Team_2_JobTracker.Models.AI;
using ST2_2026_Team_2_JobTracker.Models.Enums;
using ST2_2026_Team_2_JobTracker.Services.AI;
using Xunit;

namespace JobTracker.Tests;

public sealed class AiMatchTests : IDisposable
{
    private readonly JobTrackerFactory _factory = new();
    private readonly HttpClient _client;

    public AiMatchTests() => _client = _factory.CreateSeededClient();

    private async Task<HttpResponseMessage> SubmitAsync(Dictionary<string, string> fields)
    {
        var page = await _client.GetStringAsync("/AiMatch");
        var match = Regex.Match(page, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success);
        fields["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value);
        return await _client.PostAsync("/AiMatch", new FormUrlEncodedContent(fields));
    }

    [Fact]
    public async Task Get_shows_all_three_seeded_offers_and_mode_selection()
    {
        var html = await _client.GetStringAsync("/AiMatch");
        Assert.Contains("AI Job Match", html);
        Assert.Contains("Junior .NET Developer", html);
        Assert.Contains("Junior Python Developer", html);
        Assert.Contains("Frontend React Developer", html);
        Assert.Contains("Rule-based (offline)", html);
        Assert.Contains("/AiMatch", await _client.GetStringAsync("/Dashboard"));
    }

    [Fact]
    public async Task Offline_matching_returns_score_without_any_gemini_key()
    {
        using var response = await SubmitAsync(new()
        {
            ["JobOfferId"] = "1", ["Mode"] = "1",
            ["SkillsAndExperience"] = "I worked with C#, ASP.NET Core, SQL Server and Git."
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Match analysis", html);
        Assert.Contains("Rule-based (offline)", html);
        Assert.Contains("SQL Server", html);
    }

    [Fact]
    public async Task Invalid_post_rejected_and_never_requires_api_key()
    {
        using var response = await SubmitAsync(new()
        {
            ["JobOfferId"] = "0", ["Mode"] = "1", ["SkillsAndExperience"] = "   "
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Please select a job offer", html);
        Assert.Contains("Please enter your skills", html);
        Assert.DoesNotContain("Match analysis", html);
    }

    [Fact]
    public async Task Unknown_offer_returns_friendly_error()
    {
        using var response = await SubmitAsync(new()
        {
            ["JobOfferId"] = "9999", ["Mode"] = "1",
            ["SkillsAndExperience"] = "C# and ASP.NET Core development with SQL Server."
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("job offer no longer exists", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Gemini_mode_with_no_secret_shows_setup_message()
    {
        using var response = await SubmitAsync(new()
        {
            ["JobOfferId"] = "1", ["Mode"] = "0",
            ["SkillsAndExperience"] = "C# and ASP.NET Core development with SQL Server."
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Gemini API key is not configured", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public void Nonexistent_strategy_mode_is_rejected_by_factory()
    {
        var factory = new MatchingStrategyFactory(new IMatchingStrategy[]
        {
            new RuleBasedMatchingStrategy()
        });
        Assert.IsType<RuleBasedMatchingStrategy>(factory.Create(MatchingMode.RuleBased));
        Assert.Throws<ArgumentOutOfRangeException>(() => factory.Create((MatchingMode)999));
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}

public sealed class GeminiClientTests
{
    private const string ExampleResponse = """
        {
          "candidates": [{"content": {"parts": [{"text": "{\"score\":78,\"summary\":\"Good fit\",\"matchingSkills\":[\"C#\"],\"missingSkills\":[\"Docker\"],\"recommendations\":[\"Practice Docker\"]}" }]}}]
        }
        """;

    private static readonly MatchingInput SampleInput = new(
        "Junior .NET Developer", "C#, ASP.NET Core, Docker", "Example Co", "C#, ASP.NET Core");

    [Fact]
    public async Task Valid_response_is_parsed_and_http_request_contains_key_header()
    {
        var handler = new StubHandler(HttpStatusCode.OK, ExampleResponse);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://generativelanguage.googleapis.com/") };
        var api = new GeminiClient(http, Options.Create(new GeminiOptions { ApiKey = "test-only-key" }));

        var result = await api.AnalyzeAsync(SampleInput);

        Assert.Equal(78, result.Score);
        Assert.Equal("Gemini AI", result.Source);
        Assert.Contains("Docker", result.MissingSkills);
        Assert.Equal("test-only-key", handler.ReceivedApiKey);
        Assert.EndsWith("/v1beta/models/gemini-3.8-flash:generateContent", handler.ReceivedUri);
        Assert.DoesNotContain("test-only-key", handler.ReceivedUri);
        using var sent = JsonDocument.Parse(handler.ReceivedBody!);
        Assert.Equal("application/json", sent.RootElement
            .GetProperty("generationConfig").GetProperty("responseMimeType").GetString());
    }

    [Fact]
    public async Task Rate_limit_returns_useful_message_without_leaking_response()
    {
        var handler = new StubHandler(HttpStatusCode.TooManyRequests, "sensitive provider error");
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://generativelanguage.googleapis.com/") };
        var api = new GeminiClient(http, Options.Create(new GeminiOptions { ApiKey = "test-only-key" }));

        var ex = await Assert.ThrowsAsync<GeminiApiException>(() => api.AnalyzeAsync(SampleInput));
        Assert.Contains("limit reached", ex.Message);
        Assert.DoesNotContain("sensitive", ex.Message);
    }

    [Fact]
    public async Task Malformed_json_response_is_handled()
    {
        var handler = new StubHandler(HttpStatusCode.OK, "not json");
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://generativelanguage.googleapis.com/") };
        var api = new GeminiClient(http, Options.Create(new GeminiOptions { ApiKey = "test-only-key" }));

        var ex = await Assert.ThrowsAsync<GeminiApiException>(() => api.AnalyzeAsync(SampleInput));
        Assert.Contains("unreadable JSON", ex.Message);
    }

    private sealed class StubHandler(HttpStatusCode status, string responseBody) : HttpMessageHandler
    {
        public string? ReceivedApiKey { get; private set; }
        public string? ReceivedUri { get; private set; }
        public string? ReceivedBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ReceivedApiKey = request.Headers.GetValues("x-goog-api-key").Single();
            ReceivedUri = request.RequestUri!.ToString();
            ReceivedBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }
}
