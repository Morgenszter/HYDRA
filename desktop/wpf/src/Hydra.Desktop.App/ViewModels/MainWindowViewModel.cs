using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hydra.Desktop.Core.Runtime;

namespace Hydra.Desktop.App.ViewModels;

public partial class MainWindowViewModel(IHydraRuntimeService runtimeService) : ObservableObject
{
    [ObservableProperty]
    private string runtimeId = "not-connected";

    [ObservableProperty]
    private string health = "UNKNOWN";

    [ObservableProperty]
    private string capturedAt = "-";

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsBusy = true;

        try
        {
            var snapshot = await runtimeService.GetSnapshotAsync(CancellationToken.None);
            RuntimeId = snapshot.RuntimeId;
            Health = snapshot.Health;
            CapturedAt = snapshot.CapturedAt.ToLocalTime().ToString("u");
        }
        finally
        {
            IsBusy = false;
        }
    }
}