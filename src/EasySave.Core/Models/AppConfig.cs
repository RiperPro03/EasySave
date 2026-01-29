using EasySave.Core.Common;
using EasySave.Core.Enums;

namespace EasySave.Core.Models;

/// <summary>
/// Configuration globale de l'application.
/// Centralise les paramètres applicatifs (langue, répertoires, etc.).
/// </summary>
public class AppConfig
{
    public Language Language { get; private set; }
    public string LogDirectory { get; private set; }

    private AppConfig(Language language, string logDirectory)
    {
        Language = language;
        LogDirectory = Guard.NotNullOrWhiteSpace(logDirectory, nameof(logDirectory));
    }

    /// <summary>
    /// Charge la configuration par défaut de l'application.
    /// </summary>
    public static AppConfig LoadDefaults()
    {
        return new AppConfig(
            language: Language.English,
            logDirectory: GetDefaultLogDirectory()
        );
    }

    /// <summary>
    /// Permet de modifier la langue à l'exécution.
    /// </summary>
    public void ChangeLanguage(Language language)
    {
        Language = language;
    }

    /// <summary>
    /// Met à jour le répertoire de logs.
    /// </summary>
    public void UpdateLogDirectory(string logDirectory)
    {
        LogDirectory = Guard.NotNullOrWhiteSpace(logDirectory, nameof(logDirectory));
    }

    private static string GetDefaultLogDirectory()
    {
        // Exemple simple et portable (à adapter si besoin)
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave",
            "Logs");
    }

}