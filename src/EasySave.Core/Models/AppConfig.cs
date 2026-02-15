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
    public string? BusinessSoftwareProcessName { get; private set; }

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
    /// Update the bussiness software process name.
    /// </summary>
    /// <param name="name">The process name to monitor.</param>
    public void ChangeBussinessSoftware(string? name)
    {
        BusinessSoftwareProcessName = name;
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
}
