using Avalonia;
using System;
using System.Threading;

namespace EasySave.App.Gui;

/// <summary>
/// Application entry point.
/// </summary>
sealed class Program
{
    // Code d initialisation. Ne pas utiliser Avalonia ou des APIs avant AppMain.
    // Le SynchronizationContext n est pas pret avant AppMain.
    // Sinon le demarrage peut casser.
    private const string MutexName = "EasySave_App_Gui_SingleInstance"; 
    private static Mutex? _mutex;
    
    /// <summary>
    /// Starts the desktop application lifetime.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    [STAThread] public static void Main(string[] args) 
    { 
        bool createdNew; 
        _mutex = new Mutex(initiallyOwned: true, name: MutexName, createdNew: out createdNew); 
        if (!createdNew) { 
            // Une instance existe déjà → on quitte proprement
            Console.WriteLine("The application is already in use"); 
            return;
            
        }

        try {
            BuildAvaloniaApp() 
                .StartWithClassicDesktopLifetime(args);
        } 
        finally { 
            // Libération du mutex à la fermeture
            _mutex.ReleaseMutex(); 
        } 
    }

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