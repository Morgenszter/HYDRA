using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Hydra.Bridge.Application.AI;
using Microsoft.Extensions.Options;

namespace Hydra.Bridge.Infrastructure.AI;

public sealed class OllamaResponsesGateway(
    HttpClient httpClient,
    IOptions<HydraAiOptions> options)
{
    private readonly HydraAiOptions _options = options.Value;

    public async Task<bool> IsAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new Uri(
                new Uri(_options.Ollama.BaseUrl),
                "api/version");

            using var response = await httpClient.GetAsync(
                endpoint,
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
            _options.Ollama.Model,
            request.Input,
            false);

        using var response = await httpClient.PostAsJsonAsync(
            endpoint,
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<OllamaGenerateResponse>(
                cancellationToken: cancellationToken);

        stopwatch.Stop();

        return new AiGenerationResult(
            result?.Response ?? string.Empty,
            "Ollama",
            _options.Ollama.Model,
            stopwatch.ElapsedMilliseconds);
    }

    private sealed record OllamaGenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string? Response);
}
