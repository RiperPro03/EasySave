
using EasySave.App.Repositories;
using EasySave.Core.Models;
using EasySave.Core.Enums;
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
    public string? BusinessSoftwareProcessName => _config.BusinessSoftwareProcessName;
    
    public IReadOnlyList<string> ExtensionsToEncrypt => _config.ExtensionsToEncrypt;

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

    public void UpdateBusinessSoftwareProcessName(string? name)
    {
        _config.ChangeBussinessSoftware(string.IsNullOrWhiteSpace(name) ? null : name.Trim());
        _repository.Save(_config);
    }

    public void ApplySettings(
        bool encryptionEnabled,
        string? encryptionKey,
        Language language,
        LogFormat logFormat,
        IEnumerable<string> extensionsToEncrypt,
        string? businessSoftwareProcessName)
    {
        _config.SetEncryptionEnabled(encryptionEnabled);
        _config.UpdateEncryptionKey(encryptionKey);
        _config.ChangeLanguage(language);
        _config.ChangeLogFormat(logFormat);
        _config.UpdateExtensionsToEncrypt(extensionsToEncrypt);
        _config.ChangeBussinessSoftware(string.IsNullOrWhiteSpace(businessSoftwareProcessName)
            ? null
            : businessSoftwareProcessName.Trim());
        _repository.Save(_config);
    }
    
    public void UpdateExtensionsToEncrypt(List<string> extensions)
    {
        _config.UpdateExtensionsToEncrypt(extensions);
        _repository.Save(_config); // Sauvegarde physique sur le disque
    }
}
