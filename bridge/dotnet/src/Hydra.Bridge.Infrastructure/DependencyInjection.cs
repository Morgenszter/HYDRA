using Hydra.Bridge.Application.AI;
using Hydra.Bridge.Application.Devices;
using Hydra.Bridge.Application.Runtime;
using Hydra.Bridge.Application.Voice;
using Hydra.Bridge.Infrastructure.AI;
using Hydra.Bridge.Infrastructure.Devices;
using Hydra.Bridge.Infrastructure.Runtime;
using Hydra.Bridge.Infrastructure.Voice;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hydra.Bridge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHydraInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IHydraRuntimeRepository, InMemoryHydraRuntimeRepository>();
        services.AddSingleton<IHydraDeviceRepository, InMemoryHydraDeviceRepository>();
        services.AddSingleton<IHydraVoiceRepository, InMemoryHydraVoiceRepository>();

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
                options => !string.IsNullOrWhiteSpace(options.Ollama.Model),
                "HydraAi:Ollama:Model is required.")
            .ValidateOnStart();

        services.AddHttpClient<OllamaResponsesGateway>(
            (provider, client) =>
            {
                var options = provider
                    .GetRequiredService<IOptions<HydraAiOptions>>()
                    .Value;

                client.Timeout = TimeSpan.FromSeconds(
                    options.Ollama.TimeoutSeconds);
            });

        services.AddScoped<IModelRouter, HybridModelRouter>();

        return services;
    }
}
