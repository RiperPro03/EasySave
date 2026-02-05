using System;
using EasySave.Core.Interfaces;
using System.IO;

namespace EasySave.App.Services;

public class PathProvider : IPathProvider
{
    // Racine : %APPDATA%\ProSoft\EasySave
    private readonly string _basePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ProSoft",
        "EasySave");

    // Construction des sous-chemins
    public string LogsPath => Path.Combine(_basePath, "Logs");
    public string StatePath => Path.Combine(_basePath, "State");
    public string ConfigPath => Path.Combine(_basePath, "Config");

    public void EnsureDirectoriesCreated()
    {
        // Cree toute l'arborescence si elle n'existe pas
        Directory.CreateDirectory(LogsPath);
        Directory.CreateDirectory(StatePath);
        Directory.CreateDirectory(ConfigPath);
    }
}
