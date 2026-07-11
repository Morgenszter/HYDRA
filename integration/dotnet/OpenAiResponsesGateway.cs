using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Hydra.Bridge.Application.AI;

namespace Hydra.Bridge.Infrastructure.AI;

public sealed class OpenAiResponsesGateway(HttpClient httpClient, OpenAiOptions options)
    : IHydraModelGateway
{
    public string ProviderId => "openai";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey)) return false;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "v1/models");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
            using var response = await httpClient.SendAsync(request, cancellationToken);
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
        if (request.Classification is HydraDataClassification.PrivateLocal or HydraDataClassification.Secret)
            throw new InvalidOperationException("Local-private or secret data cannot be sent to OpenAI.");

        var payload = new
        {
            model = request.Model ?? options.DefaultModel,
            input = request.Input,
            instructions = request.Instructions
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "v1/responses")
        {
            Content = JsonContent.Create(payload)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        using var response = await httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        var root = document.RootElement;
        var outputText = root.TryGetProperty("output_text", out var text) ? text.GetString() : null;
        var model = root.TryGetProperty("model", out var modelElement)
            ? modelElement.GetString()
            : payload.model;

        return new HydraAiResponse(
            request.RequestId,
            ProviderId,
            model ?? payload.model,
            outputText ?? string.Empty);
    }
}

public sealed class OpenAiOptions
{
    public Uri BaseAddress { get; init; } = new("https://api.openai.com/");
    public string ApiKey { get; init; } = string.Empty;
    public string DefaultModel { get; init; } = "CHANGE_ME";
    public int TimeoutSeconds { get; init; } = 120;
}
