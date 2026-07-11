using Hydra.Bridge.Application.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hydra.Bridge.Infrastructure.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddHydraAi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<HydraAiOptions>()
            .Bind(configuration.GetSection(HydraAiOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(
                    options.Ollama.BaseUrl,
                    UriKind.Absolute,
                    out _),
                "HydraAi:Ollama:BaseUrl must be an absolute URI.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(
                    options.Ollama.Model),
                "HydraAi:Ollama:Model is required.")
            .ValidateOnStart();

        services.AddHttpClient<OllamaResponsesGateway>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<HydraAiOptions>>()
                    .Value;

                client.Timeout = TimeSpan.FromSeconds(
                    options.Ollama.TimeoutSeconds);
            });

        services.AddScoped<IModelRouter, HybridModelRouter>();

        return services;
    }
}
