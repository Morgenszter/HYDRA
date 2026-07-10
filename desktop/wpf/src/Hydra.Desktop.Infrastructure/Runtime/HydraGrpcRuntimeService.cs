using Grpc.Net.Client;
using Hydra.Contracts.Runtime.V1;
using Hydra.Desktop.Core.Runtime;

namespace Hydra.Desktop.Infrastructure.Runtime;

public sealed class HydraGrpcRuntimeService : IHydraRuntimeService, IDisposable
{
    private readonly GrpcChannel channel;
    private readonly HydraRuntimeService.HydraRuntimeServiceClient client;

    public HydraGrpcRuntimeService(string endpoint)
    {
        channel = GrpcChannel.ForAddress(endpoint);
        client = new HydraRuntimeService.HydraRuntimeServiceClient(channel);
    }

    public async Task<RuntimeSnapshotModel> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetRuntimeSnapshotAsync(
            new GetRuntimeSnapshotRequest { ClientId = "hydra-wpf-desktop" },
            cancellationToken: cancellationToken);

        return new RuntimeSnapshotModel(
            response.RuntimeId,
            response.Health.ToString(),
            response.CapturedAt.ToDateTimeOffset());
    }

    public void Dispose()
    {
        channel.Dispose();
    }
}