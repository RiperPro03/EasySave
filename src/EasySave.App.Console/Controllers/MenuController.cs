using EasySave.App.Console.Input;
using EasySave.App.Console.Models;
using EasySave.App.Console.Views;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

///<summary>
/// Cette classe est le point d'entrée principal qui redirige l'utilisateur vers les différents sous-menus (Jobs, Sauvegardes, Paramètres)
/// </summary> 
public sealed class MenuController
{
    private readonly ConsoleView _view;
    private readonly ConsoleInput _input;
    private readonly JobController _jobController;
    private readonly BackupController _backupController;
    private readonly SettingsController _settingsController;

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

    public void Run()
    {
        var exit = false;
        while (!exit)
        {
            _view.Clear();
            _view.ShowHeader();

            ///<summary>
            /// Création de la liste des options affichées à l'écran
            /// </summary> 
            var options = new List<MenuOption>
            {
                new(1, Strings.Menu_Create),
                new(2, Strings.Menu_Run),
                new(3, Strings.Menu_Settings),
                new(0, Strings.Menu_Exit)
            };

            _view.ShowMenu(Strings.Menu_Title, options);

            var choice = _input.ReadInt("> ");

            ///<summary>
            /// Redirection vers le contrôleur spécialisé selon le choix de l'utilisateur 
            /// </summary> 
            switch (choice)
            {
                case 1:
                    _jobController.RunMenu();
                    break;
                case 2:
                    _backupController.RunMenu();
                    break;
                case 3:
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

