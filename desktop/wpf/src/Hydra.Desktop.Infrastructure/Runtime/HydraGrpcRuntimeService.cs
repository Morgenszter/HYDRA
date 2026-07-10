using Grpc.Net.Client;
using Hydra.Contracts.Runtime.V1;
using Hydra.Desktop.Core.Runtime;

namespace Hydra.Desktop.Infrastructure.Runtime;

public sealed class HydraGrpcRuntimeService : IHydraRuntimeService, IDisposable
{
    private readonly GrpcChannel channel;
    private readonly HydraRuntimeService.HydraRuntimeServiceClient client;
    private readonly HydraDesktopOptions options;

    public HydraGrpcRuntimeService(HydraDesktopOptions options)
    {
        this.options = options;
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        channel = GrpcChannel.ForAddress(options.BridgeEndpoint);
        client = new HydraRuntimeService.HydraRuntimeServiceClient(channel);
    }

    public async Task<HydraRuntimeResult> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetRuntimeSnapshotAsync(
                new GetRuntimeSnapshotRequest { ClientId = options.ClientId },
                cancellationToken: cancellationToken);

            var snapshot = new RuntimeSnapshotModel(
                response.RuntimeId,
                response.Health.ToString(),
                response.CapturedAt.ToDateTimeOffset());

            return HydraRuntimeResult.Success(snapshot);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HydraRuntimeResult.Failure("Runtime request was cancelled.");
        }
        catch (Exception error)
        {
            return HydraRuntimeResult.Failure($"Bridge unavailable at {options.BridgeEndpoint}: {error.Message}");
        }
    }

    public void Dispose()
    {
        channel.Dispose();
    }
}