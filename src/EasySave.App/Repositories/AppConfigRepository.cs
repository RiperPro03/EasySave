using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.EasyLog.Options;

namespace EasySave.App.Repositories;

public sealed class AppConfigRepository
{
    private const string ConfigFileName = "setting.json";
    private readonly IPathProvider _pathProvider;
    private readonly string _configFilePath;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AppConfigRepository(IPathProvider pathProvider)
    {
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _configFilePath = Path.Combine(_pathProvider.ConfigPath, ConfigFileName);
    }

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
            dto = JsonSerializer.Deserialize<SettingsDto>(json, _options);
        }
        catch (JsonException)
        {
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
        return config;
    }

    public void Save(AppConfig config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        _pathProvider.EnsureDirectoriesCreated();

        var dto = new SettingsDto
        {
            Language = config.Language,
            LogFormat = config.LogFormat
        };

        var json = JsonSerializer.Serialize(dto, _options);
        File.WriteAllText(_configFilePath, json);
    }

    private sealed class SettingsDto
    {
        public Language Language { get; set; } = Language.English;
        public LogFormat LogFormat { get; set; } = LogFormat.Json;
    }
}
