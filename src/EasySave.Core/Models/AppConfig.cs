using EasySave.Core.Common;
using EasySave.Core.Enums;
using EasySave.EasyLog.Options;

namespace EasySave.Core.Models;

/// <summary>
/// Global application configuration.
/// Centralizes application settings (language, directories, and so on).
/// </summary>
public class AppConfig
{
    public Language Language { get; private set; }
    public string LogDirectory { get; private set; }
    public LogFormat LogFormat { get; private set; }
    

    /// <summary>
    /// Initializes a new instance of the <see cref="AppConfig"/> class.
    /// </summary>
    /// <param name="language">The default language.</param>
    /// <param name="logDirectory">The log directory.</param>
    /// <param name="logFormat">The log format.</param>
    private AppConfig(Language language, string logDirectory, LogFormat logFormat)
    {
        Language = language;
        LogDirectory = Guard.NotNullOrWhiteSpace(logDirectory, nameof(logDirectory));
        LogFormat = logFormat;
        // Initialisation de la liste pour éviter les surprises
        ExcludedExtensions = new List<string>();
    }

    /// <summary>
    /// Loads the default application configuration.
    /// </summary>
    /// <returns>The default configuration.</returns>
    public static AppConfig LoadDefaults()
    {
        return new AppConfig(
            language: Language.English,
            logDirectory: GetDefaultLogDirectory(),
            logFormat: LogFormat.Json
        );
    }

    /// <summary>
    /// Changes the current language.
    /// </summary>
    /// <param name="language">The new language.</param>
    public void ChangeLanguage(Language language)
    {
        Language = language;
    }

    /// <summary>
    /// Updates the log directory.
    /// </summary>
    /// <param name="logDirectory">The new log directory.</param>
    public void UpdateLogDirectory(string logDirectory)
    {
        LogDirectory = Guard.NotNullOrWhiteSpace(logDirectory, nameof(logDirectory));
    }

    /// <summary>
    /// Updates the log format.
    /// </summary>
    /// <param name="logFormat">The new log format.</param>
    public void ChangeLogFormat(LogFormat logFormat)
    {
        LogFormat = logFormat;
    }

    /// <summary>
    /// Builds the default log directory path.
    /// </summary>
    /// <returns>The default log directory path.</returns>
    private static string GetDefaultLogDirectory()
    {
        // Use a per-user application data folder to keep logs writable.
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave",
            "Logs");
    }
    
    // Pour la partie chiffrement, on stock la variable qui nous dit si on doit chiffrer 
    // ainsi que la clé elle même
    public bool EncryptionEnabled { get; private set; }
    public string EncryptionKey { get; private set; } = "default_key_change_it";
    
    // Propriété pour les extensions exclues
    public List<string> ExcludedExtensions { get; private set; } = new();

    public void ToggleEncryption()
    {
        EncryptionEnabled = !EncryptionEnabled;
    }

    public void UpdateEncryptionKey(string newKey)
    {
        EncryptionKey = newKey;
    }
    
    /// <summary>
    /// Updates the list of file extensions excluded from encryption.
    /// </summary>
    /// <param name="extensions">The list of extensions (e.g., ".pdf", ".txt").</param>
    public void UpdateExcludedExtensions(List<string> extensions)
    {
        // On s'assure que la liste n'est jamais null (KISS/CLEAN)
        ExcludedExtensions = extensions ?? new List<string>();
    }
    
    

}


