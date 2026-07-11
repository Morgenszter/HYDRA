namespace Hydra.Bridge.Application.AI;

public enum AiTaskTypeModel
{
    Unspecified,
    GeneralChat,
    VoiceIntent,
    DeviceControl,
    CodeArchitecture
}

public sealed record AiGenerationRequest(
    string Input,
    string ConversationId,
    AiTaskTypeModel TaskType,
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
