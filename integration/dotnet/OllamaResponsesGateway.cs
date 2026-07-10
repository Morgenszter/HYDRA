using System.Net.Http.Json;
using System.Text.Json;
using Hydra.Bridge.Application.AI;

namespace Hydra.Bridge.Infrastructure.AI;

public sealed class OllamaResponsesGateway(HttpClient httpClient, OllamaOptions options)
    : IHydraModelGateway
{
    public string ProviderId => "ollama";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync("v1/models", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<HydraAiResponse> GenerateAsync(
        HydraAiRequest request,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = request.Model ?? options.DefaultModel,
            input = request.Input,
            instructions = request.Instructions,
            stream = false
        };

        using var response = await httpClient.PostAsJsonAsync("v1/responses", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        var root = document.RootElement;
        var outputText = ExtractOutputText(root);
        var responseId = root.TryGetProperty("id", out var id) ? id.GetString() : null;
        var model = root.TryGetProperty("model", out var modelElement)
            ? modelElement.GetString()
            : payload.model;

        return new HydraAiResponse(
            request.RequestId,
            ProviderId,
            model ?? payload.model,
            outputText ?? string.Empty);
    }

    private static string? ExtractOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var direct))
            return direct.GetString();

        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var part in content.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text))
                    return text.GetString();
            }
        }

        return null;
    }
}

public sealed class OllamaOptions
{
    public Uri BaseAddress { get; init; } = new("http://127.0.0.1:11434/");
    public string DefaultModel { get; init; } = "CHANGE_ME";
    public int TimeoutSeconds { get; init; } = 120;
}
