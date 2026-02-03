using EasySave.App.Console.Controllers;
using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.App.Repositories;
using EasySave.App.Services;
using EasySave.Core.Models;

namespace EasySave.App.Console.Composition;

public sealed class ConsoleCompositionRoot
{
    public MenuController MenuController { get; }
    public BackupController BackupController { get; }

    public ConsoleCompositionRoot(AppConfig config)
    {
        var jobRepository = new JobRepository();
        var backupEngine = new BackupEngine();

        var input = new ConsoleInput();
        var argsParser = new ArgsParser();

        var consoleView = new ConsoleView();
        var jobView = new JobView(input);
        var backupView = new BackupView(input);

        var settingsController = new SettingsController(config, consoleView, input);
        var jobController = new JobController(jobRepository, jobView, consoleView);
        var backupController = new BackupController(backupEngine, jobRepository, backupView, consoleView, argsParser);

        MenuController = new MenuController(consoleView, input, jobController, backupController, settingsController);
        BackupController = backupController;
    }
}
