using System.Globalization;
using EasySave.Core.Common;
using EasySave.Core.Models;
using EasySave.App.Console.Controllers;
using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.App.Repositories;
using EasySave.App.Services;



namespace EasySave.App.Console;

/// <summary>
/// Application entry point for the console UI.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Configures services and starts the console workflow.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>

    private static void Main(string[] args)
    {
        var pathProvider = new PathProvider();
        var configRepository = new AppConfigRepository(pathProvider);
        // Charge la configuration utilisateur.
        var config = configRepository.Load();

        // Applique la culture selon la langue config.
        var culture = Localization.GetCulture(config.Language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        var jobService = new JobService(pathProvider);
        var backupService = new BackupService(
            jobService,
            logDirectory: pathProvider.LogsPath,
            logFormatProvider: () => config.LogFormat);

        var input = new ConsoleInput();
        var argsParser = new ArgsParser();

        var consoleView = new ConsoleView();
        var jobView = new JobView(input);
        var backupView = new BackupView(input);

        var settingsController = new SettingsController(config, configRepository, consoleView, input);
        var jobController = new JobController(jobService, jobView, consoleView);
        var backupController = new BackupController(backupService, jobService, backupView, consoleView, argsParser);
        var menuController = new MenuController(consoleView, input, jobController, backupController, settingsController);

        if (args.Length > 0)
        {
            // Mode batch: execute les jobs passes en argument.
            var rawArgs = string.Join(" ", args);
            backupController.RunFromArgs(rawArgs);
            return;
        }

        // Mode interactif: affiche le menu principal.
        menuController.Run();
    }
}

