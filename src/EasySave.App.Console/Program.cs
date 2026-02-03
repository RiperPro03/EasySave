using System.Globalization;
using EasySave.App.Console.Composition;
using EasySave.Core.Common;
using EasySave.Core.Models;

namespace EasySave.App.Console;

internal static class Program
{
    private static void Main(string[] args)
    {
        var config = AppConfig.LoadDefaults();

        var culture = Localization.GetCulture(config.Language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        var compositionRoot = new ConsoleCompositionRoot(config);

        if (args.Length > 0)
        {
            var rawArgs = string.Join(" ", args);
            compositionRoot.BackupController.RunFromArgs(rawArgs);
            return;
        }

        compositionRoot.MenuController.Run();
    }
}

