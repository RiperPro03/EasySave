using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using EasySave.App.Gui.ViewModels;
using EasySave.App.Gui.Views;
using EasySave.App.Repositories;
using EasySave.App.Services;
using EasySave.Core.Common;

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
            // Plus d infos: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            // Construit les services une seule fois pour la duree de l'app GUI.
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
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(jobService, backupService, pathProvider.LogsPath),
            };
        }

        base.OnFrameworkInitializationCompleted();
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
