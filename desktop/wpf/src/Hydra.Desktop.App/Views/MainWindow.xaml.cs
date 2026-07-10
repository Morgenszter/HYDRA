using System.Windows;
using Hydra.Desktop.App.ViewModels;

namespace Hydra.Desktop.App.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}