namespace Hydra.Bridge.Application.AI;

public interface IAiModelGateway
{
    string ProviderName { get; }

    Task<bool> IsAvailableAsync(
        CancellationToken cancellationToken = default);

    Task<AiGenerationResult> GenerateAsync(
        AiGenerationRequest request,
        CancellationToken cancellationToken = default);
}
