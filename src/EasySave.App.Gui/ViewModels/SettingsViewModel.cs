using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.App.Gui.Localization;
using EasySave.App.Services;
using EasySave.Core.Enums;
using EasySave.EasyLog.Options;

namespace EasySave.App.Gui.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService? _settings;

    // Propriétés automatiques (le toolkit crée le reste pour nous)
    [ObservableProperty] private bool _encryptionEnabled;
    [ObservableProperty] private string _encryptionKey = string.Empty;
    [ObservableProperty] private Language _selectedLanguage;
    [ObservableProperty] private LogFormat _selectedLogFormat;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRemoteLogServerSettingsVisible))]
    private LogStorageMode _selectedLogStorageMode;
    [ObservableProperty] private string _extensionsToEncrypt = string.Empty;
    [ObservableProperty] private string _businessSoftwareProcessName = string.Empty;
    [ObservableProperty] private string _logServerHost = "localhost";
    [ObservableProperty] private string _logServerPort = "9696";
    
    // Listes pour remplir les menus déroulants (ComboBox)
    public Language[] Languages => Enum.GetValues<Language>();
    public LogFormat[] LogFormats => Enum.GetValues<LogFormat>();
    public LogStorageMode[] LogStorageModes => Enum.GetValues<LogStorageMode>();
    public bool IsRemoteLogServerSettingsVisible
        => SelectedLogStorageMode is LogStorageMode.ServerOnly or LogStorageMode.LocalAndServer;

    public SettingsViewModel(SettingsService? settings = null)
    {
        _settings = settings;

        // Si settings est null (mode design), on ne charge rien
        if (settings == null) return; 

        // On charge les données existantes
        EncryptionEnabled = settings.EncryptionEnabled;
        EncryptionKey = settings.EncryptionKey;
        SelectedLanguage = settings.Language;
        SelectedLogFormat = settings.LogFormat;
        SelectedLogStorageMode = settings.LogStorageMode;
        LogServerHost = settings.LogServerHost;
        LogServerPort = settings.LogServerPort.ToString();
        
        // On transforme la liste d'extensions en une chaîne de caractères séparée par des virgules
        // Si ton service possède une propriété ExtensionsToEncrypt (List<string>)
        ExtensionsToEncrypt = string.Join(", ", settings.ExtensionsToEncrypt);
        BusinessSoftwareProcessName = settings.BusinessSoftwareProcessName ?? string.Empty;
    }
    // Changes are applied on save to avoid log spam.

    [RelayCommand]
    private void SaveSettings()
    {
        if (_settings == null)
            return;

        var list = ExtensionsToEncrypt.Split(',')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        // On garde la valeur actuelle si la saisie du port est invalide pour eviter un crash de l'ecran.
        var parsedServerPort = int.TryParse(LogServerPort, out var portValue)
            && portValue is > 0 and <= 65535
            ? portValue
            : _settings.LogServerPort;

        _settings.ApplySettings(
            encryptionEnabled: EncryptionEnabled,
            encryptionKey: EncryptionKey,
            language: SelectedLanguage,
            logFormat: SelectedLogFormat,
            logStorageMode: SelectedLogStorageMode,
            logServerHost: LogServerHost,
            logServerPort: parsedServerPort,
            extensionsToEncrypt: list,
            businessSoftwareProcessName: BusinessSoftwareProcessName);

        Loc.Instance.SetLanguage(SelectedLanguage);
        OnPropertyChanged(nameof(Languages));
        OnPropertyChanged(nameof(LogFormats));
        OnPropertyChanged(nameof(LogStorageModes));
        OnPropertyChanged(nameof(IsRemoteLogServerSettingsVisible));
    }

    public void SetFrench() => Loc.Instance.SetLanguage(Language.French);
    public void SetEnglish() => Loc.Instance.SetLanguage(Language.English);

    public void RefreshLocalization()
    {
        OnPropertyChanged(nameof(Languages));
        OnPropertyChanged(nameof(LogFormats));
        OnPropertyChanged(nameof(LogStorageModes));
    }
}



