using System.Windows;
using Hydra.Desktop.App.ViewModels;
using Hydra.Desktop.App.Views;
using Hydra.Desktop.Core.Runtime;
using Hydra.Desktop.Infrastructure.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Hydra.Desktop.App;

public partial class App : Application
{
    private readonly ServiceProvider services;

    public App()
    {
        var collection = new ServiceCollection();
        collection.AddSingleton<IHydraRuntimeService>(_ =>
            new HydraGrpcRuntimeService("https://localhost:5001"));
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