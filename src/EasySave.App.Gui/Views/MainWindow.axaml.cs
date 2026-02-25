using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using EasySave.App.Gui.Models;
using EasySave.App.Gui.ViewModels;

namespace EasySave.App.Gui.Views;

/// <summary>
/// Main application window.
/// </summary>
public partial class MainWindow : Window
{
    private readonly WindowNotificationManager _notifications;
    private MainWindowViewModel? _subscribedViewModel;

    public MainWindow()
    {
        InitializeComponent();
        _notifications = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 4
        };
        DataContextChanged += OnDataContextChanged;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Disposes the data context if it implements <see cref="IDisposable"/>.
    /// </summary>
    /// <param name="e">Close event arguments.</param>
    protected override void OnClosed(EventArgs e)
    {
        UnsubscribeCurrentViewModel();
        DataContextChanged -= OnDataContextChanged;

        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnClosed(e);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        UnsubscribeCurrentViewModel();

        if (DataContext is MainWindowViewModel vm)
        {
            _subscribedViewModel = vm;
            vm.NotificationRequested += OnNotificationRequested;
        }
    }

    private void UnsubscribeCurrentViewModel()
    {
        if (_subscribedViewModel is null)
            return;

        _subscribedViewModel.NotificationRequested -= OnNotificationRequested;
        _subscribedViewModel = null;
    }

    private void OnNotificationRequested(object? sender, UiNotificationEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _notifications.Show(new Notification(
                e.Title,
                e.Message,
                ToNotificationType(e.Severity)));
        });
    }

    private static NotificationType ToNotificationType(UiNotificationSeverity severity)
    {
        return severity switch
        {
            UiNotificationSeverity.Success => NotificationType.Success,
            UiNotificationSeverity.Warning => NotificationType.Warning,
            UiNotificationSeverity.Error => NotificationType.Error,
            _ => NotificationType.Information
        };
    }
}
