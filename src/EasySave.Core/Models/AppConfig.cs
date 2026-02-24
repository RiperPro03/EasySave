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
    public LogStorageMode LogStorageMode { get; private set; } = LogStorageMode.LocalOnly;
    public string LogServerHost { get; private set; } = "localhost";
    public int LogServerPort { get; private set; } = 9696;
    public string? BusinessSoftwareProcessName { get; private set; }
    public bool EncryptionEnabled { get; private set; }
    public string EncryptionKey { get; private set; } = string.Empty;
    private List<string> _extensionsToEncrypt = new();
    public IReadOnlyList<string> ExtensionsToEncrypt => _extensionsToEncrypt;
    public int LargeFileThresholdKb { get; private set; } = 10000; 

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
        EncryptionEnabled = false;
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
    /// Updates the log storage strategy.
    /// </summary>
    /// <param name="storageMode">The storage mode used by EasyLog.</param>
    public void ChangeLogStorageMode(LogStorageMode storageMode)
    {
        LogStorageMode = storageMode;
    }

    /// <summary>
    /// Updates the centralized log server connection settings.
    /// </summary>
    /// <param name="host">The server host name or IP address.</param>
    /// <param name="port">The server port.</param>
    public void UpdateLogServerConnection(string host, int port)
    {
        LogServerHost = Guard.NotNullOrWhiteSpace(host, nameof(host));

        if (port is <= 0 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");

        LogServerPort = port;
    }

    /// <summary>
    /// Enables or disables encryption.
    /// </summary>
    /// <param name="enabled">Whether encryption is enabled.</param>
    public void SetEncryptionEnabled(bool enabled)
    {
        EncryptionEnabled = enabled;
    }

    /// <summary>
    /// Toggles encryption on or off.
    /// </summary>
    public void ToggleEncryption()
    {
        EncryptionEnabled = !EncryptionEnabled;
    }

    /// <summary>
    /// Updates the encryption key.
    /// </summary>
    /// <param name="newKey">The new key (empty to clear).</param>
    public void UpdateEncryptionKey(string? newKey)
    {
        EncryptionKey = newKey ?? string.Empty;
    }

    /// <summary>
    /// Updates the list of extensions to encrypt.
    /// </summary>
    /// <param name="extensions">The extensions to encrypt.</param>
    public void UpdateExtensionsToEncrypt(IEnumerable<string> extensions)
    {
        _extensionsToEncrypt = extensions
            .Where(ext => !string.IsNullOrWhiteSpace(ext))
            .Select(ext => ext.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
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
    /// <summary>
    /// Updates the threshold for large file detection
    /// </summary>
    /// <param name="thresholdkb">Threshold in kilobytes</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void UpdateLargeFileThreshold(int thresholdkb)
    {
        LargeFileThresholdKb = thresholdkb;
    }
}
