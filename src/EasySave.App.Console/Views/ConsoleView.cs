using EasySave.App.Console.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Views;

public sealed class ConsoleView
{
    public void Clear()
    {
        System.Console.Clear();
    }

    public void ShowHeader()
    {
        System.Console.WriteLine("=================================");
        System.Console.WriteLine(" EasySave - Backup Software ");
        System.Console.WriteLine("=================================");
    }

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

    public void ShowSuccess(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine(message);
        System.Console.ResetColor();
    }

    public void ShowInfo(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine(message);
        System.Console.ResetColor();
    }

    public void WaitForKey()
    {
        System.Console.WriteLine();
        System.Console.WriteLine(Strings.UI_PressKey);
        System.Console.ReadKey(true);
    }
}
