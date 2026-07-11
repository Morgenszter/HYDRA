using Hydra.Bridge.Application.AI;
using Microsoft.Extensions.Options;

namespace Hydra.Bridge.Infrastructure.AI;

public sealed class HybridModelRouter(
    OllamaResponsesGateway ollama,
    IOptions<HydraAiOptions> options)
    : IModelRouter
{
    private readonly HydraAiOptions _options = options.Value;

    public async Task<AiGenerationResult> GenerateAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TaskType is AiTaskTypeModel.DeviceControl)
        {
            throw new InvalidOperationException(
                "Device control must pass through the HYDRA command validator.");
        }

        if (await ollama.IsAvailableAsync(cancellationToken))
        {
            return await ollama.GenerateAsync(request, cancellationToken);
        }

        throw new InvalidOperationException(
            "Ollama is unavailable and cloud fallback is disabled.");
    }

    public async Task<AiProviderStatus> GetStatusAsync(
        CancellationToken cancellationToken = default)
    {
        return new AiProviderStatus(
            await ollama.IsAvailableAsync(cancellationToken),
            _options.Ollama.Model,
            _options.OpenAI.Enabled);
    }
}
