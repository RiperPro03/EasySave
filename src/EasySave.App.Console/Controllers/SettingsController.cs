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

/// <summary>
/// Handles application settings in the console UI.
/// </summary>
public sealed class SettingsController
{
    private readonly AppConfig _config;
    private readonly AppConfigRepository _configRepository;
    private readonly ConsoleView _consoleView;
    private readonly ConsoleInput _input;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsController"/> class.
    /// </summary>
    /// <param name="config">Application configuration.</param>
    /// <param name="configRepository">Repository used to persist configuration.</param>
    /// <param name="consoleView">View used for console output.</param>
    /// <param name="input">Console input helper.</param>
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

    /// <summary>
    /// Shows the settings menu and applies changes.
    /// </summary>
    public void Run()
    {
        _consoleView.Clear();
        _consoleView.ShowHeader();

        // Affiche les options disponibles (langue et format de log).
        _consoleView.ShowInfo($"1 - {Strings.Lang_English}");
        _consoleView.ShowInfo($"2 - {Strings.Lang_French}");
        _consoleView.ShowInfo($"3 - {Strings.UI_LogFormatJson}");
        _consoleView.ShowInfo($"4 - {Strings.UI_LogFormatXml}");
        

        _consoleView.ShowInfo($"0 - {Strings.UI_Back}");

        // Lecture du choix utilisateur.
        var choice = _input.ReadChoice("> ", new[] { 0, 1, 2, 3, 4, 5, 6 });

        switch (choice)
        {
            case 1:
                // Change la langue en anglais.
                UpdateLanguage(Language.English);
                break;
            case 2:
                // Change la langue en français.
                UpdateLanguage(Language.French);
                break;
            case 3:
                // Change le format de log en JSON.
                UpdateLogFormat(LogFormat.Json);
                break;
            case 4:
                // Change le format de log en XML.
                UpdateLogFormat(LogFormat.Xml);
                break;
            case 0:
                return;
        }

        // Persiste la configuration après modification.
        _configRepository.Save(_config);
        _consoleView.WaitForKey();
    }

    /// <summary>
    /// Updates the current language and applies the culture.
    /// </summary>
    /// <param name="language">The language to apply.</param>
    private void UpdateLanguage(Language language)
    {
        _config.ChangeLanguage(language);

        // Met à jour la culture courante pour l'UI.
        var culture = Localization.GetCulture(_config.Language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        _consoleView.ShowSuccess(Strings.Info_LanguageChanged);
    }

    /// <summary>
    /// Updates the log format used by the application.
    /// </summary>
    private void UpdateLogFormat(LogFormat logFormat)
    {
        _config.ChangeLogFormat(logFormat);
        _consoleView.ShowSuccess(Strings.Info_LogFormatChanged);
    }

}
