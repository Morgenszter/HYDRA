using Hydra.Bridge.Application.AI;

namespace Hydra.Bridge.Infrastructure.AI;

public sealed class HybridModelRouter(
    IEnumerable<IHydraModelGateway> gateways) : IHydraAiRouter
{
    private readonly IReadOnlyDictionary<string, IHydraModelGateway> providers = gateways
        .ToDictionary(x => x.ProviderId, StringComparer.OrdinalIgnoreCase);

    public async Task<HydraAiResponse> GenerateAsync(
        HydraAiRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Classification == HydraDataClassification.Secret)
            throw new InvalidOperationException("Secret data is not allowed in model prompts.");

        var order = ResolveOrder(request);
        Exception? lastError = null;

        foreach (var providerId in order)
        {
            if (!providers.TryGetValue(providerId, out var provider)) continue;
            if (!await provider.IsAvailableAsync(cancellationToken)) continue;

            try
            {
                return await provider.GenerateAsync(request, cancellationToken);
            }
            catch (Exception error) when (request.RoutingPolicy is HydraRoutingPolicy.LocalFirst or HydraRoutingPolicy.CloudFirst)
            {
                lastError = error;
            }
        }

        throw new InvalidOperationException("No model provider completed the request.", lastError);
    }

    private static string[] ResolveOrder(HydraAiRequest request)
    {
        if (request.Classification == HydraDataClassification.PrivateLocal)
            return ["ollama"];

        return request.Provider switch
        {
            HydraModelProvider.Ollama => ["ollama"],
            HydraModelProvider.OpenAI => ["openai"],
            _ => request.RoutingPolicy switch
            {
                HydraRoutingPolicy.LocalOnly => ["ollama"],
                HydraRoutingPolicy.CloudOnly => ["openai"],
                HydraRoutingPolicy.CloudFirst => ["openai", "ollama"],
                _ => ["ollama", "openai"]
            }
        };
    }
}
