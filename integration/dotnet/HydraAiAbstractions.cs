namespace Hydra.Bridge.Application.AI;

public enum HydraModelProvider { Auto, Ollama, OpenAI }
public enum HydraRoutingPolicy { LocalOnly, CloudOnly, LocalFirst, CloudFirst, Classified }
public enum HydraDataClassification { Public, Internal, PrivateLocal, Secret }

public sealed record HydraAiRequest(
    string RequestId,
    string Input,
    string? Instructions,
    HydraModelProvider Provider,
    HydraRoutingPolicy RoutingPolicy,
    HydraDataClassification Classification,
    string? Model);

public sealed record HydraAiResponse(
    string RequestId,
    string Provider,
    string Model,
    string OutputText);

public interface IHydraModelGateway
{
    string ProviderId { get; }
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
    Task<HydraAiResponse> GenerateAsync(HydraAiRequest request, CancellationToken cancellationToken);
}

public interface IHydraAiRouter
{
    Task<HydraAiResponse> GenerateAsync(HydraAiRequest request, CancellationToken cancellationToken);
}
