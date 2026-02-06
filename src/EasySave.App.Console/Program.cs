using System.Globalization;
using EasySave.Core.Common;
using EasySave.Core.Models;
using EasySave.App.Console.Controllers;
using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.App.Repositories;
using EasySave.App.Services;

namespace EasySave.App.Console;

internal static class Program
{
    private static void Main(string[] args)
    {
        var pathProvider = new PathProvider();
        var configRepository = new AppConfigRepository(pathProvider);
        var config = configRepository.Load();

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
            var rawArgs = string.Join(" ", args);
            backupController.RunFromArgs(rawArgs);
            return;
        }

        menuController.Run();
    }
}

