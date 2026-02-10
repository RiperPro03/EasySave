using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EasySave.App.Gui.Views;

/// <summary>
/// Jobs view.
/// </summary>
public partial class JobsView : UserControl
{
    public JobsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
