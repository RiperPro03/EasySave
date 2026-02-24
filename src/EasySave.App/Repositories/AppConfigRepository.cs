using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.Core.Logging;
using EasySave.App.Services;
using EasySave.EasyLog.Options;
using EasySave.App.Utils;

namespace EasySave.App.Repositories;

/// <summary>
/// Handles persistence of application configuration settings.
/// </summary>
public sealed class AppConfigRepository
{
    private const string ConfigFileName = "setting.json";
    private readonly IPathProvider _pathProvider;
    private readonly string _configFilePath;
    private readonly IAppLogService? _logService;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AppConfigRepository"/> class.
    /// </summary>
    /// <param name="pathProvider">Provides configuration paths.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pathProvider"/> is null.</exception>
    public AppConfigRepository(IPathProvider pathProvider, IAppLogService? logService = null)
    {
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _configFilePath = Path.Combine(_pathProvider.ConfigPath, ConfigFileName);
        _logService = logService;
    }

    /// <summary>
    /// Loads configuration from disk or creates defaults when missing or invalid.
    /// </summary>
    /// <returns>The loaded configuration.</returns>
    public AppConfig Load()
    {
        _pathProvider.EnsureDirectoriesCreated();

        if (!File.Exists(_configFilePath))
        {
            var defaults = AppConfig.LoadDefaults();
            Save(defaults);
            return defaults;
        }

        var json = File.ReadAllText(_configFilePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            var defaults = AppConfig.LoadDefaults();
            Save(defaults);
            return defaults;
        }

        SettingsDto? dto;
        try
        {
            // On transforme le texte JSON en objet C#
            dto = JsonSerializer.Deserialize<SettingsDto>(json, _options);
        }
        catch (JsonException)
        {
            // Si le fichier est corrompu, on remet tout par defaut pour eviter le plantage
            var defaults = AppConfig.LoadDefaults();
            Save(defaults);
            return defaults;
        }

        if (dto is null)
        {
            var defaults = AppConfig.LoadDefaults();
            Save(defaults);
            return defaults;
        }

        var config = AppConfig.LoadDefaults();
        config.ChangeLanguage(dto.Language);
        config.ChangeLogFormat(dto.LogFormat);
        config.ChangeLogStorageMode(dto.LogStorageMode);
        // Compatibilite ascendante: on garde les valeurs par defaut si l'ancien fichier n'a pas ces champs.
        config.UpdateLogServerConnection(
            string.IsNullOrWhiteSpace(dto.LogServerHost) ? config.LogServerHost : dto.LogServerHost,
            dto.LogServerPort is <= 0 or > 65535 ? config.LogServerPort : dto.LogServerPort);
        config.ChangeBussinessSoftware(dto.BusinessSoftwareProcessName);
        config.SetEncryptionEnabled(dto.EncryptionEnabled);
        config.UpdateEncryptionKey(dto.EncryptionKey);
        config.UpdateExtensionsToEncrypt(dto.ExtensionsToEncrypt ?? new List<string>());
        return config;
    }

    /// <summary>
    /// Saves configuration to disk.
    /// </summary>
    /// <param name="config">The configuration to persist.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public void Save(AppConfig config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        _pathProvider.EnsureDirectoriesCreated();

        var dto = new SettingsDto
        {
            Language = config.Language,
            LogFormat = config.LogFormat,
            LogStorageMode = config.LogStorageMode,
            LogServerHost = config.LogServerHost,
            LogServerPort = config.LogServerPort,
            BusinessSoftwareProcessName = config.BusinessSoftwareProcessName,
            EncryptionEnabled = config.EncryptionEnabled,
            EncryptionKey = config.EncryptionKey,
            ExtensionsToEncrypt = config.ExtensionsToEncrypt.ToList()
        };

        var json = JsonSerializer.Serialize(dto, _options);
        File.WriteAllText(_configFilePath, json);

        WriteSettingsLog(config);
    }

    private void WriteSettingsLog(AppConfig config)
    {
        if (_logService == null)
            return;

        var entry = LogEntryBuilder.Create(
                eventName: "settings.saved",
                category: LogEventCategory.Settings,
                action: LogEventAction.Save,
                message: "Settings saved")
            .WithLevel(LogLevel.Info)
            .WithSettings(
                config.Language,
                config.LogFormat,
                ToUncOrEmpty(config.LogDirectory),
                ToUncOrEmpty(_configFilePath),
                config.LogStorageMode,
                config.LogServerHost,
                config.LogServerPort,
                config.EncryptionEnabled,
                config.ExtensionsToEncrypt,
                config.BusinessSoftwareProcessName)
            .Build();

        _logService.Write(entry);
    }

    /// <summary>
    /// Normalizes a path to UNC for logging.
    /// </summary>
    private static string ToUncOrEmpty(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        return UncResolver.ResolveToUncForLog(path);
    }

    /// <summary>
    /// DTO used for JSON serialization of settings.
    /// </summary>
    private sealed class SettingsDto
    {
        public Language Language { get; set; } = Language.English;
        public LogFormat LogFormat { get; set; } = LogFormat.Json;
        public LogStorageMode LogStorageMode { get; set; } = LogStorageMode.LocalOnly;
        public string? LogServerHost { get; set; } = "localhost";
        public int LogServerPort { get; set; } = 9696;
        public string? BusinessSoftwareProcessName { get; set; }
        public bool EncryptionEnabled { get; set; }
        public string? EncryptionKey { get; set; }
        public List<string>? ExtensionsToEncrypt { get; set; }
    }
}
