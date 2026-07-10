namespace Hydra.Bridge.Application.AI;

public interface IModelRouter
{
    Task<AiGenerationResult> GenerateAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default);

    Task<AiProviderStatus> GetStatusAsync(
        CancellationToken cancellationToken = default);
}
