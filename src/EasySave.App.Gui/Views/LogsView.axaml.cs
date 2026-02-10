using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.App.Gui.Views;

/// <summary>
/// Logs view.
/// </summary>
public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
