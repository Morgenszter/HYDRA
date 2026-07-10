using Hydra.Bridge.Application.AI;
using Microsoft.Extensions.Options;

namespace Hydra.Bridge.Infrastructure.AI;

public sealed class HybridModelRouter(
    OllamaResponsesGateway ollamaGateway,
    IOptions<HydraAiOptions> options)
    : IModelRouter
{
    private readonly HydraAiOptions _options = options.Value;

    public async Task<AiGenerationResult> GenerateAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var localAvailable = await ollamaGateway.IsAvailableAsync(
            cancellationToken);

        if (localAvailable)
        {
            return await ollamaGateway.GenerateAsync(
                request,
                cancellationToken);
        }

        throw new InvalidOperationException(
            "Ollama is unavailable and no approved cloud gateway is configured.");
    }

    public async Task<AiProviderStatus> GetStatusAsync(
        CancellationToken cancellationToken = default)
    {
        var ollamaAvailable = await ollamaGateway.IsAvailableAsync(
            cancellationToken);

        return new AiProviderStatus(
            OllamaAvailable: ollamaAvailable,
            OllamaModel: _options.Ollama.Model,
            CloudEnabled: _options.OpenAI.Enabled);
    }
}
