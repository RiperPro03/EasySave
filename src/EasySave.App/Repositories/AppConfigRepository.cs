using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.EasyLog.Options;

namespace EasySave.App.Repositories;
/// <summary>
///Cette classe sert à lire et écrire le fichier "setting.json" qui contient les options de l'appli
/// <summary>
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
    /// <summary>
    /// Charge les réglages depuis le fichier, ou crée des réglages par défaut si le fichier n'existe pas
    /// <summary>

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
            /// <summary>
            /// On transforme le texte JSON en objet C#
            /// <summary>
            dto = JsonSerializer.Deserialize<SettingsDto>(json, _options);
        }
        catch (JsonException)
        {
            /// <summary>
            /// Si le fichier est corrompu, on remet tout par défaut pour éviter le plantage
            /// <summary>
            
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
    /// <summary>
    /// Enregistre les réglages actuels dans le fichier JSON
    /// </summary>
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
