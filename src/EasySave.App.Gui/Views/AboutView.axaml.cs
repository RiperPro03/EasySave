using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.App.Gui.Views;

/// <summary>
/// About view.
/// </summary>
public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
