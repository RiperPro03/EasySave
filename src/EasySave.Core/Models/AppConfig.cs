using EasySave.Core.Common;
using EasySave.Core.Enums;
using EasySave.EasyLog.Options;

namespace EasySave.Core.Models;

/// <summary>
/// Configuration globale de l'application.
/// Centralise les parametres applicatifs (langue, repertoires, etc.).
/// </summary>
public class AppConfig
{
    public Language Language { get; private set; }
    public string LogDirectory { get; private set; }
    public LogFormat LogFormat { get; private set; }

    private AppConfig(Language language, string logDirectory, LogFormat logFormat)
    {
        Language = language;
        LogDirectory = Guard.NotNullOrWhiteSpace(logDirectory, nameof(logDirectory));
        LogFormat = logFormat;
    }

    /// <summary>
    /// Charge la configuration par defaut de l'application.
    /// </summary>
    public static AppConfig LoadDefaults()
    {
        return new AppConfig(
            language: Language.English,
            logDirectory: GetDefaultLogDirectory(),
            logFormat: LogFormat.Json
        );
    }

    /// <summary>
    /// Permet de modifier la langue a l'execution.
    /// </summary>
    public void ChangeLanguage(Language language)
    {
        Language = language;
    }

    /// <summary>
    /// Met a jour le repertoire de logs.
    /// </summary>
    public void UpdateLogDirectory(string logDirectory)
    {
        LogDirectory = Guard.NotNullOrWhiteSpace(logDirectory, nameof(logDirectory));
    }

    /// <summary>
    /// Met a jour le format de logs.
    /// </summary>
    public void ChangeLogFormat(LogFormat logFormat)
    {
        LogFormat = logFormat;
    }

    private static string GetDefaultLogDirectory()
    {
        // Exemple simple et portable (a adapter si besoin)
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave",
            "Logs");
    }
}
