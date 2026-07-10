using Hydra.Bridge.Api.Grpc;
using Hydra.Bridge.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.AI.json",
    optional: false,
    reloadOnChange: true);

builder.Services.AddGrpc();
builder.Services.AddHydraInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<HydraRuntimeGrpcService>();
app.MapGrpcService<HydraDeviceGrpcService>();
app.MapGrpcService<HydraVoiceGrpcService>();
app.MapGrpcService<HydraAiGrpcService>();

app.MapGet(
    "/",
    () => Results.Ok(
        new
        {
            service = "HYDRA_HOME gRPC Bridge",
            status = "online",
            transport = "gRPC"
        }));

app.MapGet(
    "/health",
    () => Results.Ok(
        new
        {
            status = "healthy",
            utc = DateTimeOffset.UtcNow
        }));

app.Run();
