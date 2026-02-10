using EasySave.App.Console.Input;
using EasySave.App.Console.Models;
using EasySave.App.Console.Views;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

/// <summary>
/// Handles the main console menu navigation.
/// </summary>
public sealed class MenuController
{
    private readonly ConsoleView _view;
    private readonly ConsoleInput _input;
    private readonly JobController _jobController;
    private readonly BackupController _backupController;
    private readonly SettingsController _settingsController;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuController"/> class.
    /// </summary>
    /// <param name="view">Console view for output.</param>
    /// <param name="input">Console input helper.</param>
    /// <param name="jobController">Controller for job management.</param>
    /// <param name="backupController">Controller for backups.</param>
    /// <param name="settingsController">Controller for settings.</param>
    public MenuController(
        ConsoleView view,
        ConsoleInput input,
        JobController jobController,
        BackupController backupController,
        SettingsController settingsController)
    {
        _view = view;
        _input = input;
        _jobController = jobController;
        _backupController = backupController;
        _settingsController = settingsController;
    }

    /// <summary>
    /// Runs the main menu loop.
    /// </summary>
    public void Run()
    {
        var exit = false;
        while (!exit)
        {
            _view.Clear();
            _view.ShowHeader();

            // Construit la liste des options du menu principal.
            var options = new List<MenuOption>
            {
                new(1, Strings.Menu_Create),
                new(2, Strings.Menu_Run),
                new(3, Strings.Menu_Settings),
                new(0, Strings.Menu_Exit)
            };

            _view.ShowMenu(Strings.Menu_Title, options);

            var choice = _input.ReadInt("> ");

            switch (choice)
            {
                case 1:
                    // Gestion des jobs.
                    _jobController.RunMenu();
                    break;
                case 2:
                    // Lancement des sauvegardes.
                    _backupController.RunMenu();
                    break;
                case 3:
                    // Parametres applicatifs.
                    _settingsController.Run();
                    break;
                case 0:
                    exit = true;
                    break;
                default:
                    _view.ShowError(Strings.Error_InvalidChoice);
                    _view.WaitForKey();
                    break;
            }
        }
    }
}

