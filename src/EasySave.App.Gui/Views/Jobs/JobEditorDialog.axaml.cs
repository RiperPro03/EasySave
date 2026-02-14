using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using EasySave.App.Gui.ViewModels;

namespace EasySave.App.Gui.Views;

public partial class JobEditorDialog : Window
{
    public JobEditorDialog()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private async void OnBrowseSource(object? sender, RoutedEventArgs e)
    {
        var path = await BrowseForFolder("Select Source Directory");
        if (!string.IsNullOrEmpty(path) && DataContext is JobEditorViewModel vm)
        {
            vm.SourcePath = path;
        }
    }

    private async void OnBrowseTarget(object? sender, RoutedEventArgs e)
    {
        var path = await BrowseForFolder("Select Target Directory");
        if (!string.IsNullOrEmpty(path) && DataContext is JobEditorViewModel vm)
        {
            vm.TargetPath = path;
        }
    }

    private async Task<string?> BrowseForFolder(string title)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
            return null;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
