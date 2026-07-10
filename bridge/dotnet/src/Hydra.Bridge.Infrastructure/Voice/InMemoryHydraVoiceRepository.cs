using System.Runtime.CompilerServices;
using Hydra.Bridge.Application.Voice;

namespace Hydra.Bridge.Infrastructure.Voice;

public sealed class InMemoryHydraVoiceRepository : IHydraVoiceRepository
{
    private readonly object _sync = new();
    private VoiceStateModel _state = new(
        VoiceModeModel.Passive,
        "OMEGON",
        string.Empty,
        string.Empty,
        DateTimeOffset.UtcNow);

    public ValueTask<VoiceStateModel> GetStateAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return ValueTask.FromResult(_state);
        }
    }

    public async IAsyncEnumerable<VoiceEventModel> StreamEventsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            VoiceStateModel snapshot;
            lock (_sync)
            {
                snapshot = _state;
            }

            yield return new VoiceEventModel(
                Guid.NewGuid().ToString("N"),
                VoiceEventTypeModel.StateChanged,
                snapshot.LastTranscript,
                snapshot.LastIntent,
                DateTimeOffset.UtcNow);
        }
    }

    public ValueTask<VoiceIntentResultModel> SubmitTranscriptAsync(
        string transcript,
        string locale,
        DateTimeOffset capturedAt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(transcript))
        {
            return ValueTask.FromResult(
                new VoiceIntentResultModel(
                    string.Empty,
                    0,
                    false,
                    "Transcript is empty."));
        }

        var normalized = transcript.Trim().ToUpperInvariant();
        var intent = normalized switch
        {
            var value when value.Contains("GRZEJNIK") => "heater.control",
            var value when value.Contains("ŚWIATŁ") => "light.control",
            var value when value.Contains("STATUS") => "runtime.status",
            var value when value.Contains("OMEGON") => "voice.wake",
            _ => "general.chat"
        };

        lock (_sync)
        {
            _state = _state with
            {
                Mode = VoiceModeModel.Passive,
                LastTranscript = transcript,
                LastIntent = intent,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        return ValueTask.FromResult(
            new VoiceIntentResultModel(
                intent,
                0.90f,
                true,
                "Intent accepted by rule-based bridge parser."));
    }
}
