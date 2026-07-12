using Hydra.Bridge.Api.Endpoints;
using Hydra.Bridge.Api.Grpc;
using Hydra.Bridge.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(
    options =>
    {
        options.ListenLocalhost(
            5000,
            listenOptions => listenOptions.Protocols = HttpProtocols.Http1AndHttp2);
    });

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

app.MapHydraHudEndpoints();

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
