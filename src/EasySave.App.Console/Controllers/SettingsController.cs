using System.Globalization;
using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.Core.Common;
using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

public sealed class SettingsController
{
    private readonly AppConfig _config;
    private readonly ConsoleView _consoleView;
    private readonly ConsoleInput _input;

    public SettingsController(AppConfig config, ConsoleView consoleView, ConsoleInput input)
    {
        _config = config;
        _consoleView = consoleView;
        _input = input;
    }

    public void Run()
    {
        _consoleView.Clear();
        _consoleView.ShowHeader();

        _consoleView.ShowInfo($"1 - {Strings.Lang_English}");
        _consoleView.ShowInfo($"2 - {Strings.Lang_French}");

        var choice = _input.ReadChoice("> ", new[] { 1, 2 });

        var newLanguage = choice switch
        {
            1 => Language.English,
            2 => Language.French,
            _ => Language.English
        };

        _config.ChangeLanguage(newLanguage);

        var culture = Localization.GetCulture(_config.Language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        _consoleView.ShowSuccess(Strings.Info_LanguageChanged);
        _consoleView.WaitForKey();
    }
}
