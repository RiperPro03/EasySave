using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using EasySave.App.Gui.ViewModels;

namespace EasySave.App.Gui;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Creates a view for a view model based on naming convention.
    /// </summary>
    /// <param name="param">The view model instance.</param>
    /// <returns>The matching view or a placeholder when missing.</returns>
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        // Convention: FooViewModel -> FooView dans le meme namespace.
        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <summary>
    /// Indicates whether this template can handle the data.
    /// </summary>
    /// <param name="data">The data instance.</param>
    /// <returns><c>true</c> when the data is a view model.</returns>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
