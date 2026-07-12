using System.Windows;
using Hydra.Desktop.Hud.ViewModels;

namespace Hydra.Desktop.Hud;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.RefreshCommand.ExecuteAsync(null);
    }
}
