using EasySave.Core.Resources;

namespace EasySave.App.Console;

public class ConsoleUI
{
    public void Clear()
    {
        if (System.Console.IsOutputRedirected)
            return;
        try
        {
            System.Console.Clear();
        }
        catch (IOException)
        {
            // Ignore
        }
        catch (PlatformNotSupportedException)
        {
            // Ignore
        }
    }

    public void ShowWelcome()
    {
        System.Console.WriteLine("=================================");
        System.Console.WriteLine(" EasySave - Backup Software ");
        System.Console.WriteLine("=================================");
    }

    public int ShowMainMenu()
    {
        System.Console.WriteLine();
        System.Console.WriteLine(Strings.Menu_Title);
        System.Console.WriteLine("1 - " + Strings.Menu_Create);
        System.Console.WriteLine("2 - " + Strings.Menu_Run);
        System.Console.WriteLine("3 - " + Strings.Menu_ChangeLanguage);
        System.Console.WriteLine("0 - " + Strings.Menu_Exit);
        System.Console.Write("> ");
        
        var line = System.Console.ReadLine();

        if (line is null)
        {
            return 0;
        }

        if (!int.TryParse(line, out var choice))
            return -1;

        return choice;
    }

    public void ShowError(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
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
        System.Console.WriteLine(Strings.UI_PressKey);
        
        if (System.Console.IsInputRedirected)
        {
            System.Console.ReadLine(); // consomme une ligne (ex: "\n" dans tes tests)
            return;
        }
        System.Console.ReadKey(true);
    }

}