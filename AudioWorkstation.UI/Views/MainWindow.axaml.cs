using Avalonia.Controls;
using AudioWorkstation.UI.ViewModels;

namespace AudioWorkstation.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}