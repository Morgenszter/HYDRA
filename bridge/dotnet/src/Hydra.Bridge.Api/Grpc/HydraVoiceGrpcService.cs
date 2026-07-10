using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hydra.Bridge.Application.Voice;
using Hydra.Contracts.Voice.V1;

namespace Hydra.Bridge.Api.Grpc;

public sealed class HydraVoiceGrpcService(
    IHydraVoiceRepository repository)
    : HydraVoiceService.HydraVoiceServiceBase
{
    public override async Task<VoiceState> GetVoiceState(
        GetVoiceStateRequest request,
        ServerCallContext context)
    {
        return Map(
            await repository.GetStateAsync(
                context.CancellationToken));
    }

    public override async Task StreamVoiceEvents(
        StreamVoiceEventsRequest request,
        IServerStreamWriter<VoiceEvent> responseStream,
        ServerCallContext context)
    {
        await foreach (var item in repository.StreamEventsAsync(
            context.CancellationToken))
        {
            await responseStream.WriteAsync(Map(item));
        }
    }

    public override async Task<VoiceIntentResult> SubmitTranscript(
        SubmitTranscriptRequest request,
        ServerCallContext context)
    {
        var capturedAt = request.CapturedAt?.ToDateTimeOffset()
            ?? DateTimeOffset.UtcNow;

        var result = await repository.SubmitTranscriptAsync(
            request.Transcript,
            request.Locale,
            capturedAt,
            context.CancellationToken);

        return new VoiceIntentResult
        {
            Intent = result.Intent,
            Confidence = result.Confidence,
            Accepted = result.Accepted,
            Reason = result.Reason
        };
    }

    private static VoiceState Map(VoiceStateModel model) => new()
    {
        Mode = model.Mode switch
        {
            VoiceModeModel.Disabled => VoiceMode.Disabled,
            VoiceModeModel.Passive => VoiceMode.Passive,
            VoiceModeModel.Armed => VoiceMode.Armed,
            VoiceModeModel.Processing => VoiceMode.Processing,
            _ => VoiceMode.Unspecified
        },
        ActiveProfile = model.ActiveProfile,
        LastTranscript = model.LastTranscript,
        LastIntent = model.LastIntent,
        UpdatedAt = Timestamp.FromDateTimeOffset(model.UpdatedAt)
    };

    private static VoiceEvent Map(VoiceEventModel model) => new()
    {
        EventId = model.EventId,
        EventType = model.EventType switch
        {
            VoiceEventTypeModel.Transcript => VoiceEventType.Transcript,
            VoiceEventTypeModel.Intent => VoiceEventType.Intent,
            VoiceEventTypeModel.Error => VoiceEventType.Error,
            VoiceEventTypeModel.StateChanged => VoiceEventType.StateChanged,
            _ => VoiceEventType.Unspecified
        },
        Transcript = model.Transcript,
        Intent = model.Intent,
        OccurredAt = Timestamp.FromDateTimeOffset(model.OccurredAt)
    };
}
