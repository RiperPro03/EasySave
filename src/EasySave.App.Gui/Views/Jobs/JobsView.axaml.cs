using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EasySave.App.Gui.ViewModels;
using EasySave.Core.Models;

namespace EasySave.App.Gui.Views;

/// <summary>
/// Jobs view.
/// </summary>
public partial class JobsView : UserControl
{
    private JobsViewModel? _viewModel;

    public JobsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
        _viewModel.CreateRequested -= OnCreateRequested;
        _viewModel.EditRequested -= OnEditRequested;
        }

        _viewModel = DataContext as JobsViewModel;

        if (_viewModel != null)
        {
            _viewModel.CreateRequested += OnCreateRequested;
            _viewModel.EditRequested += OnEditRequested;
        }
    }

    private async void OnCreateRequested()
    {
        if (_viewModel == null)
            return;

        var editor = JobEditorViewModel.CreateNew();
        var saved = await ShowEditorDialog(editor);
        if (saved)
        {
            _viewModel.CreateFromEditor(editor);
        }
    }

    private async void OnEditRequested(BackupJob job)
    {
        if (_viewModel == null)
            return;

        var editor = JobEditorViewModel.FromJob(job);
        var saved = await ShowEditorDialog(editor);
        if (saved)
        {
            _viewModel.UpdateFromEditor(editor);
        }
    }

    private async Task<bool> ShowEditorDialog(JobEditorViewModel editor)
    {
        var dialog = new JobEditorDialog
        {
            DataContext = editor
        };

        EventHandler<bool?>? handler = null;
        handler = (_, result) => dialog.Close(result);
        editor.CloseRequested += handler;

        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner == null)
        {
            dialog.Show();
            editor.CloseRequested -= handler;
            return false;
        }

        bool? result = await dialog.ShowDialog<bool?>(owner);

        editor.CloseRequested -= handler;
        return result == true;
    }
}
