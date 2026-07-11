using System.Windows;
using Hydra.Desktop.Hud.Services;
using Hydra.Desktop.Hud.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hydra.Desktop.Hud;

public partial class App : Application
{
    private readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.AddHttpClient<HydraRuntimeClient>(client =>
            {
                client.BaseAddress = new Uri("http://127.0.0.1:5000");
                client.Timeout = TimeSpan.FromSeconds(5);
            });
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        })
        .Build();

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();
        _host.Services.GetRequiredService<MainWindow>().Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}