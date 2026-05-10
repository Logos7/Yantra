using Avalonia.Controls;
using Yantra.Studio.ViewModels;

namespace Yantra.Studio.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new StudioAppViewModel();
    }
}
