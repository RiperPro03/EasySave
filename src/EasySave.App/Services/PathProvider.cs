using System;
using EasySave.Core.Interfaces;
using System.IO;

namespace EasySave.App.Services;

/// <summary>
/// Provides application paths under the user profile.
/// </summary>
public class PathProvider : IPathProvider
{
    /// <summary>
    /// Base path under %APPDATA%\ProSoft\EasySave.
    /// </summary>
    private readonly string _basePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ProSoft",
        "EasySave");

    /// <summary>
    /// Gets the logs directory path.
    /// </summary>
    public string LogsPath => Path.Combine(_basePath, "Logs");

    /// <summary>
    /// Gets the state directory path.
    /// </summary>
    public string StatePath => Path.Combine(_basePath, "State");

    /// <summary>
    /// Gets the configuration directory path.
    /// </summary>
    public string ConfigPath => Path.Combine(_basePath, "Config");

    /// <summary>
    /// Ensures all required directories exist.
    /// </summary>
    public void EnsureDirectoriesCreated()
    {
        // Cree toute l'arborescence si elle n'existe pas
        Directory.CreateDirectory(LogsPath);
        Directory.CreateDirectory(StatePath);
        Directory.CreateDirectory(ConfigPath);
    }
}
