using System.Globalization;
using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.App.Repositories;
using EasySave.Core.Common;
using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.Core.Resources;
using EasySave.EasyLog.Options;

namespace EasySave.App.Console.Controllers;

// Cette classe permet à l'utilisateur de modifier la langue de l'application et le format des logs
public sealed class SettingsController
{
    private readonly AppConfig _config;
    private readonly AppConfigRepository _configRepository;
    private readonly ConsoleView _consoleView;
    private readonly ConsoleInput _input;

    public SettingsController(
        AppConfig config,
        AppConfigRepository configRepository,
        ConsoleView consoleView,
        ConsoleInput input)
    {
        _config = config;
        _configRepository = configRepository;
        _consoleView = consoleView;
        _input = input;
    }

    public void Run()
    {
        _consoleView.Clear();
        _consoleView.ShowHeader();

        _consoleView.ShowInfo($"1 - {Strings.Lang_English}");
        _consoleView.ShowInfo($"2 - {Strings.Lang_French}");
        _consoleView.ShowInfo($"3 - {Strings.UI_LogFormatJson}");
        _consoleView.ShowInfo($"4 - {Strings.UI_LogFormatXml}");
        _consoleView.ShowInfo($"0 - {Strings.UI_Back}");

        var choice = _input.ReadChoice("> ", new[] { 0, 1, 2, 3, 4 });

        switch (choice)
        {
            case 1:
                UpdateLanguage(Language.English);
                break;
            case 2:
                UpdateLanguage(Language.French);
                break;
            case 3:
                UpdateLogFormat(LogFormat.Json);
                break;
            case 4:
                UpdateLogFormat(LogFormat.Xml);
                break;
            case 0:
                return;
        }

        // Cette classe permet à l'utilisateur de modifier la langue de l'application et le format des logs
        _configRepository.Save(_config);
        _consoleView.WaitForKey();
    }

    // Change la culture du programme pour traduire les textes instantanément
    private void UpdateLanguage(Language language)
    {
        _config.ChangeLanguage(language);

        var culture = Localization.GetCulture(_config.Language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        _consoleView.ShowSuccess(Strings.Info_LanguageChanged);
    }

    // Modifie le type de fichier utilisé pour l'enregistrement des logs
    private void UpdateLogFormat(LogFormat logFormat)
    {
        _config.ChangeLogFormat(logFormat);
        _consoleView.ShowSuccess(Strings.Info_LogFormatChanged);
    }
}
