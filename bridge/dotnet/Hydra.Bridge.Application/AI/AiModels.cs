using Hydra.Contracts.AI.V1;

namespace Hydra.Bridge.Application.AI;

public sealed record AiGenerationRequest(
    string Input,
    string ConversationId,
    AiTaskType TaskType,
    bool AllowCloudFallback);

public sealed record AiGenerationResult(
    string Output,
    string Provider,
    string Model,
    long LatencyMs);

public sealed record AiProviderStatus(
    bool OllamaAvailable,
    string OllamaModel,
    bool CloudEnabled);
