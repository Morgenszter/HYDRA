namespace Hydra.Bridge.Application.Voice;

public interface IHydraVoiceRepository
{
    ValueTask<VoiceStateModel> GetStateAsync(
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<VoiceEventModel> StreamEventsAsync(
        CancellationToken cancellationToken = default);

    ValueTask<VoiceIntentResultModel> SubmitTranscriptAsync(
        string transcript,
        string locale,
        DateTimeOffset capturedAt,
        CancellationToken cancellationToken = default);
}
