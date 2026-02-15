
using EasySave.App.Repositories;
using EasySave.Core.Models;
using  EasySave.Core.Enums;
using EasySave.EasyLog.Options;
    
namespace EasySave.App.Services;

public class SettingsService
{
    private readonly AppConfig _config;
    private readonly AppConfigRepository _repository;

    public SettingsService(AppConfig config, AppConfigRepository repository)
    {
        _config = config;
        _repository = repository;
    }

    public bool EncryptionEnabled => _config.EncryptionEnabled;
    public string EncryptionKey => _config.EncryptionKey;
    public Language Language => _config.Language;
    public LogFormat LogFormat => _config.LogFormat;
    
    public List<string> ExcludedExtensions { get; private set; } = new();

    public void ToggleEncryption()
    {
        _config.ToggleEncryption();
        _repository.Save(_config);
    }

    public void UpdateEncryptionKey(string newKey)
    {
        _config.UpdateEncryptionKey(newKey);
        _repository.Save(_config);
    }

    public void UpdateLanguage(Language lang)
    {
        _config.ChangeLanguage(lang);
        _repository.Save(_config);
    }

    public void UpdateLogFormat(LogFormat format)
    {
        _config.ChangeLogFormat(format);
        _repository.Save(_config);
    }
    
    public void UpdateExcludedExtensions(List<string> extensions)
    {
        _config.UpdateExcludedExtensions(extensions);
        _repository.Save(_config); // Sauvegarde physique sur le disque
    }
}
