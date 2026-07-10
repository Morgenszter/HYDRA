using System.Windows;
using Hydra.Desktop.App.ViewModels;
using Hydra.Desktop.App.Views;
using Hydra.Desktop.Core.Runtime;
using Hydra.Desktop.Infrastructure.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hydra.Desktop.App;

public partial class App : Application
{
    private readonly ServiceProvider services;

    public App()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var options = configuration.GetSection("HydraDesktop").Get<HydraDesktopOptions>()
            ?? new HydraDesktopOptions();
        var lifecycleOptions = configuration.GetSection("HydraBridgeLifecycle").Get<HydraBridgeLifecycleOptions>()
            ?? new HydraBridgeLifecycleOptions();

        var collection = new ServiceCollection();
        collection.AddSingleton(options);
        collection.AddSingleton(lifecycleOptions);
        collection.AddSingleton<IHydraRuntimeService, HydraGrpcRuntimeService>();
        collection.AddSingleton<IHydraBridgeLifecycleService, HydraBridgeLifecycleService>();
        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<MainWindow>();
        services = collection.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        services.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        services.Dispose();
        base.OnExit(e);
    }
}