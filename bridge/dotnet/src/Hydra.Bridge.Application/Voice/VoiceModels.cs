namespace Hydra.Bridge.Application.Voice;

public enum VoiceModeModel
{
    Unspecified,
    Disabled,
    Passive,
    Armed,
    Processing
}

public enum VoiceEventTypeModel
{
    Unspecified,
    Transcript,
    Intent,
    Error,
    StateChanged
}

public sealed record VoiceStateModel(
    VoiceModeModel Mode,
    string ActiveProfile,
    string LastTranscript,
    string LastIntent,
    DateTimeOffset UpdatedAt);

public sealed record VoiceEventModel(
    string EventId,
    VoiceEventTypeModel EventType,
    string Transcript,
    string Intent,
    DateTimeOffset OccurredAt);

public sealed record VoiceIntentResultModel(
    string Intent,
    float Confidence,
    bool Accepted,
    string Reason);
