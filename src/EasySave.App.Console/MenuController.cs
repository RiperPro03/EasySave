using System.Globalization;
using EasySave.Core.Common;
using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Console;

public class MenuController
{
    private readonly ConsoleUI _ui;
    private readonly AppConfig _config;

    public MenuController(AppConfig config)
    {
        _ui = new ConsoleUI();
        _config = config;
    }

    public void Start(string[] args)
    {
        // TODO v1.0
        // - Parser les arguments CLI (1-3, 1;3)
        // - Sinon afficher le menu

        var exit = false;
        while (!exit)
        {
            _ui.Clear();
            _ui.ShowWelcome();

            var choice = _ui.ShowMainMenu(); // à mettre à jour pour afficher l'option 3

            switch (choice)
            {
                case 1:
                    CreateBackupJob();
                    break;

                case 2:
                    RunBackupJob();
                    break;

                case 3:
                    ChangeLanguage();
                    break;

                case 0:
                    exit = true;
                    break;

                default:
                    _ui.ShowError(Strings.Error_InvalidChoice);
                    _ui.WaitForKey();
                    break;
            }
        }
    }

    private void CreateBackupJob()
    {
        _ui.Clear();
        _ui.ShowInfo("Create backup job – TODO");
        _ui.WaitForKey();
    }

    private void RunBackupJob()
    {
        _ui.Clear();
        _ui.ShowInfo("Run backup job – TODO");
        _ui.WaitForKey();
    }

    private void ChangeLanguage()
    {
        _ui.Clear();

        _ui.ShowInfo("1 - English");
        _ui.ShowInfo("2 - Français");
        System.Console.Write("> ");

        var input = System.Console.ReadLine();
        var selected = int.TryParse(input, out var c) ? c : -1;

        var newLanguage = selected switch
        {
            1 => Language.English,
            2 => Language.French,
            _ => (Language?)null
        };

        if (newLanguage is null)
        {
            _ui.ShowError(Strings.Error_InvalidChoice);
            _ui.WaitForKey();
            return;
        }

        _config.ChangeLanguage(newLanguage.Value);

        var culture = Localization.GetCulture(_config.Language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        _ui.ShowInfo(Strings.Info_LanguageChanged);
        _ui.WaitForKey();
    }
}
