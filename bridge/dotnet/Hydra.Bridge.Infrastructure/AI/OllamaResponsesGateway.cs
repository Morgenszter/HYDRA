using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Hydra.Bridge.Application.AI;
using Microsoft.Extensions.Options;

namespace Hydra.Bridge.Infrastructure.AI;

public sealed class OllamaResponsesGateway(
    HttpClient httpClient,
    IOptions<HydraAiOptions> options)
    : IAiModelGateway
{
    private readonly HydraAiOptions _options = options.Value;

    public string ProviderName => "Ollama";

    public async Task<bool> IsAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var versionEndpoint = new Uri(
                new Uri(_options.Ollama.BaseUrl),
                "api/version");

            using var response = await httpClient.GetAsync(
                versionEndpoint,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    public async Task<AiGenerationResult> GenerateAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Input);

        var stopwatch = Stopwatch.StartNew();

        var endpoint = new Uri(
            new Uri(_options.Ollama.BaseUrl),
            "api/generate");

        var payload = new OllamaGenerateRequest(
            Model: _options.Ollama.Model,
            Prompt: request.Input,
            Stream: false);

        using var response = await httpClient.PostAsJsonAsync(
            endpoint,
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(
            cancellationToken: cancellationToken);

        stopwatch.Stop();

        return new AiGenerationResult(
            Output: result?.Response ?? string.Empty,
            Provider: ProviderName,
            Model: _options.Ollama.Model,
            LatencyMs: stopwatch.ElapsedMilliseconds);
    }

    private sealed record OllamaGenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string? Response);
}
