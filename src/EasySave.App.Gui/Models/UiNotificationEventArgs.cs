using System;

namespace EasySave.App.Gui.Models;

public sealed class UiNotificationEventArgs : EventArgs
{
    public UiNotificationEventArgs(string title, string message, UiNotificationSeverity severity)
    {
        Title = title;
        Message = message;
        Severity = severity;
    }

    public string Title { get; }
    public string Message { get; }
    public UiNotificationSeverity Severity { get; }
}
