using System;
using EasySave.Core.Interfaces;
using System.IO;

namespace EasySave.App.Services;

public class PathProvider : IPathProvider
{
    /// <summary>
    /// Racine : %APPDATA%\ProSoft\EasySave
    /// </summary>
    private readonly string _basePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ProSoft",
        "EasySave");

    /// <summary>
    /// Construction des sous-chemins
    /// </summary>
    public string LogsPath => Path.Combine(_basePath, "Logs");
    public string StatePath => Path.Combine(_basePath, "State");
    public string ConfigPath => Path.Combine(_basePath, "Config");

    public void EnsureDirectoriesCreated()
    {
        ///<summary>
        /// Cree toute l'arborescence si elle n'existe pas
        /// </summary> 
        Directory.CreateDirectory(LogsPath);
        Directory.CreateDirectory(StatePath);
        Directory.CreateDirectory(ConfigPath);
    }
}
