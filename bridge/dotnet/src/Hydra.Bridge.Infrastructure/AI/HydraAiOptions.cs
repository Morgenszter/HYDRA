namespace Hydra.Bridge.Infrastructure.AI;

public sealed class HydraAiOptions
{
    public const string SectionName = "HydraAi";

    public string RoutingMode { get; init; } = "LocalFirst";
    public OllamaOptions Ollama { get; init; } = new();
    public OpenAiOptions OpenAI { get; init; } = new();
}

public sealed class OllamaOptions
{
    public string BaseUrl { get; init; } = "http://127.0.0.1:11434/";
    public string Model { get; init; } = "qwen3:8b";
    public int TimeoutSeconds { get; init; } = 120;
}

public sealed class OpenAiOptions
{
    public bool Enabled { get; init; }
    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";
    public string Model { get; init; } = string.Empty;
    public string ApiKeyEnvironmentVariable { get; init; } = "OPENAI_API_KEY";
}
