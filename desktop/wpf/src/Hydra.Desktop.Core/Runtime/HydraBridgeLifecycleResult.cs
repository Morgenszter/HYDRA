namespace Hydra.Desktop.Core.Runtime;

public sealed record HydraBridgeLifecycleResult(
    bool IsSuccess,
    string Message)
{
    public static HydraBridgeLifecycleResult Success(string message) => new(true, message);

    public static HydraBridgeLifecycleResult Failure(string message) => new(false, message);
}