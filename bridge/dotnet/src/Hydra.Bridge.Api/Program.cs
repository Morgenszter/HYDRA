using Hydra.Bridge.Api.Services;
using Hydra.Bridge.Application.Runtime;
using Hydra.Bridge.Infrastructure.Runtime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddSingleton<IHydraRuntimeRepository, InMemoryHydraRuntimeRepository>();

var app = builder.Build();

app.MapGrpcService<HydraRuntimeGrpcService>();
app.MapGet("/", () => "HYDRA_HOME gRPC bridge is online. Use a gRPC client.");

app.Run();