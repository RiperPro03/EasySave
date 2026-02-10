using EasySave.App.Console.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Views;

/// <summary>
/// Provides common console UI rendering helpers.
/// </summary>
public sealed class ConsoleView
{
    /// <summary>
    /// Clears the console screen.
    /// </summary>
    public void Clear()
    {
        System.Console.Clear();
    }

    /// <summary>
    /// Displays the application header.
    /// </summary>
    public void ShowHeader()
    {
        System.Console.WriteLine("=================================");
        System.Console.WriteLine(" EasySave - Backup Software ");
        System.Console.WriteLine("=================================");
    }

    /// <summary>
    /// Displays a menu with a title and options.
    /// </summary>
    /// <param name="title">Menu title.</param>
    /// <param name="options">Menu options.</param>
    public void ShowMenu(string title, IEnumerable<MenuOption> options)
    {
        System.Console.WriteLine();
        System.Console.WriteLine(title);
        foreach (var option in options)
        {
            System.Console.WriteLine($"{option.Id} - {option.Label}");
        }
    }

    public void ShowError(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine(message);
        System.Console.ResetColor();
    }

    /// <summary>
    /// Displays a success message.
    /// </summary>
    /// <param name="message">Message to display.</param>
    public void ShowSuccess(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine(message);
        System.Console.ResetColor();
    }

    /// <summary>
    /// Displays an informational message.
    /// </summary>
    /// <param name="message">Message to display.</param>
    public void ShowInfo(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine(message);
        System.Console.ResetColor();
    }

    /// <summary>
    /// Waits for a key press before continuing.
    /// </summary>
    public void WaitForKey()
    {
        System.Console.WriteLine();
        // Pause l'ecran pour laisser le temps de lire.
        System.Console.WriteLine(Strings.UI_PressKey);
        System.Console.ReadKey(true);
    }
}
