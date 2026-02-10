using Avalonia;
using System;

namespace EasySave.App.Gui;

/// <summary>
/// Application entry point.
/// </summary>
sealed class Program
{
    // Code d initialisation. Ne pas utiliser Avalonia ou des APIs avant AppMain.
    // Le SynchronizationContext n est pas pret avant AppMain.
    // Sinon le demarrage peut casser.
    
    /// <summary>
    /// Starts the desktop application lifetime.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Configuration Avalonia, utilisee aussi par le designer.
    
    /// <summary>
    /// Configures Avalonia for desktop usage.
    /// </summary>
    /// <returns>The configured <see cref="AppBuilder"/>.</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}