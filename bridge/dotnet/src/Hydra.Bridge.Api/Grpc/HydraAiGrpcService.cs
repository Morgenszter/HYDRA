using Grpc.Core;
using Hydra.Bridge.Application.AI;
using Hydra.Contracts.AI.V1;

namespace Hydra.Bridge.Api.Grpc;

public sealed class HydraAiGrpcService(
    IModelRouter modelRouter,
    ILogger<HydraAiGrpcService> logger)
    : HydraAiService.HydraAiServiceBase
{
    public override async Task<GenerateResponse> Generate(
        GenerateRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Input))
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    "Input cannot be empty."));
        }

        try
        {
            var result = await modelRouter.GenerateAsync(
                new AiGenerationRequest(
                    request.Input,
                    request.ConversationId,
                    Map(request.TaskType),
                    request.AllowCloudFallback),
                context.CancellationToken);

            logger.LogInformation(
                "AI response generated using {Provider}/{Model} in {LatencyMs} ms.",
                result.Provider,
                result.Model,
                result.LatencyMs);

            return new GenerateResponse
            {
                Output = result.Output,
                Provider = result.Provider,
                Model = result.Model,
                LatencyMs = result.LatencyMs
            };
        }
        catch (InvalidOperationException exception)
        {
            throw new RpcException(
                new Status(
                    StatusCode.FailedPrecondition,
                    exception.Message));
        }
        catch (HttpRequestException exception)
        {
            throw new RpcException(
                new Status(
                    StatusCode.Unavailable,
                    exception.Message));
        }
    }

    public override async Task<GetProviderStatusResponse> GetProviderStatus(
        GetProviderStatusRequest request,
        ServerCallContext context)
    {
        var status = await modelRouter.GetStatusAsync(
            context.CancellationToken);

        return new GetProviderStatusResponse
        {
            OllamaAvailable = status.OllamaAvailable,
            OllamaModel = status.OllamaModel,
            CloudEnabled = status.CloudEnabled
        };
    }

    private static AiTaskTypeModel Map(AiTaskType value) => value switch
    {
        AiTaskType.GeneralChat => AiTaskTypeModel.GeneralChat,
        AiTaskType.VoiceIntent => AiTaskTypeModel.VoiceIntent,
        AiTaskType.DeviceControl => AiTaskTypeModel.DeviceControl,
        AiTaskType.CodeArchitecture => AiTaskTypeModel.CodeArchitecture,
        _ => AiTaskTypeModel.Unspecified
    };
}
