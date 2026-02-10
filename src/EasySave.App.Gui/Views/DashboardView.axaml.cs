using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.App.Gui.Views;

/// <summary>
/// Dashboard view.
/// </summary>
public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
