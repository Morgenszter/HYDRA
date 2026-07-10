using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hydra.Desktop.Core.Runtime;

namespace Hydra.Desktop.App.ViewModels;

public partial class MainWindowViewModel(
    IHydraRuntimeService runtimeService,
    IHydraBridgeLifecycleService bridgeLifecycleService) : ObservableObject
{
    [ObservableProperty]
    private string runtimeId = "not-connected";

    [ObservableProperty]
    private string health = "UNKNOWN";

    [ObservableProperty]
    private string capturedAt = "-";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = "Bridge not queried yet.";

    [RelayCommand]
    private async Task StartBridgeAsync()
    {
        IsBusy = true;

        try
        {
            var result = await bridgeLifecycleService.EnsureBridgeAsync(CancellationToken.None);
            StatusMessage = result.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;

        try
        {
            var result = await runtimeService.GetSnapshotAsync(CancellationToken.None);

            if (!result.IsSuccess || result.Snapshot is null)
            {
                RuntimeId = "not-connected";
                Health = "OFFLINE";
                CapturedAt = "-";
                StatusMessage = result.ErrorMessage ?? "Bridge unavailable.";
                return;
            }

            RuntimeId = result.Snapshot.RuntimeId;
            Health = result.Snapshot.Health;
            CapturedAt = result.Snapshot.CapturedAt.ToLocalTime().ToString("u");
            StatusMessage = "Runtime snapshot loaded.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}