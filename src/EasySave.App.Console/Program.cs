using System.Globalization;
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

        var controller = new MenuController(config);
        controller.Start(args);
    }
}