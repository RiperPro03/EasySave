using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.App.Gui.Views;

/// <summary>
/// Settings view.
/// </summary>
public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
