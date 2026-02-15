using System;
using System.Globalization;
using System.Reflection;
using EasySave.App.Console.Controllers;
using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.App.Repositories;
using EasySave.App.Services;
using EasySave.Core.Common;
using EasySave.Core.Logging;
using EasySave.Core.Models;

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
        var config = AppConfig.LoadDefaults();
        var logContext = BuildLogContext();
        var appLogService = new AppLogService(pathProvider.LogsPath, () => config.LogFormat, logContext);
        var configRepository = new AppConfigRepository(pathProvider, appLogService);
        // Charge la configuration utilisateur.
        config = configRepository.Load();

        // Applique la culture selon la langue config.
        var culture = Localization.GetCulture(config.Language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        var jobService = new JobService(pathProvider, appLogService);
        var backupService = new BackupService(
            jobService,
            logDirectory: pathProvider.LogsPath,
            logFormatProvider: () => config.LogFormat,
            logService: appLogService);

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

    /// <summary>
    /// Builds a log context shared by console log entries.
    /// </summary>
    private static LogContext BuildLogContext()
    {
        var version = "2.0.0";
        return new LogContext
        {
            AppName = "EasySave",
            AppVersion = version,
            HostName = Environment.MachineName,
            UserName = Environment.UserName,
            ProcessId = Environment.ProcessId
        };
    }
}
