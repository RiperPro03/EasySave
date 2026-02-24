using System.Globalization;
using System.Linq;
using System.Reflection;
using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using EasySave.App.Gui.Localization;
using EasySave.App.Gui.ViewModels;
using EasySave.App.Gui.Views;
using EasySave.App.Repositories;
using EasySave.App.Services;
using EasySave.Core.Common;
using EasySave.Core.Logging;
using EasySave.Core.Models;

namespace EasySave.App.Gui;

/// <summary>
/// Avalonia application bootstrapper for the GUI.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Loads XAML resources for the application.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Builds core services and wires the main window.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Evite les validations en double entre Avalonia et CommunityToolkit.
            DisableAvaloniaDataAnnotationValidation();

            // Construit les services une seule fois pour la duree de l'app GUI.
            var pathProvider = new PathProvider();
            var config = AppConfig.LoadDefaults();
            var logContext = BuildLogContext();
            var appLogService = new AppLogService(
                pathProvider.LogsPath,
                () => config.LogFormat,
                () => config.LogStorageMode,
                () => config.LogServerHost,
                () => config.LogServerPort,
                logContext);
            var configRepository = new AppConfigRepository(pathProvider, appLogService);
            config = configRepository.Load();

            // --- DEBUT DE TA PARTIE (KISS & FIX) ---
            // On crée le service avec les ingrédients dont il a besoin (config et repository)
            var settingsService = new SettingsService(config, configRepository);
            // --- FIN DE TA PARTIE ---

            Loc.Instance.SetLanguage(config.Language);

            var jobService = new JobService(pathProvider, appLogService);
            var backupService = new BackupService(
                jobService,
                config,
                logDirectory: pathProvider.LogsPath,
                logFormatProvider: () => config.LogFormat,
                logService: appLogService);

            desktop.MainWindow = new MainWindow
            {
                // On passe bien settingsService en 4ème position comme prévu dans le ViewModel
                DataContext = new MainWindowViewModel(jobService, backupService, pathProvider.LogsPath, settingsService, appLogService),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Builds a log context shared by GUI log entries.
    /// </summary>
    private static LogContext BuildLogContext()
    {
        var version = "3.0.0";
        return new LogContext
        {
            AppName = "EasySave",
            AppVersion = version,
            HostName = Environment.MachineName,
            UserName = Environment.UserName,
            ProcessId = Environment.ProcessId
        };
    }

    /// <summary>
    /// Removes Avalonia data annotation validation to avoid duplicate validations.
    /// </summary>
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Recupere la liste des plugins a retirer.
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // Retire chaque plugin detecte.
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
