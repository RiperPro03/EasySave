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
    [ObservableProperty] private string _extensionsToEncrypt = string.Empty;
    [ObservableProperty] private string _businessSoftwareProcessName = string.Empty;
    
    // Listes pour remplir les menus déroulants (ComboBox)
    public Language[] Languages => Enum.GetValues<Language>();
    public LogFormat[] LogFormats => Enum.GetValues<LogFormat>();

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

        _settings.ApplySettings(
            encryptionEnabled: EncryptionEnabled,
            encryptionKey: EncryptionKey,
            language: SelectedLanguage,
            logFormat: SelectedLogFormat,
            extensionsToEncrypt: list,
            businessSoftwareProcessName: BusinessSoftwareProcessName);

        Loc.Instance.SetLanguage(SelectedLanguage);
        OnPropertyChanged(nameof(Languages));
        OnPropertyChanged(nameof(LogFormats));
    }

    public void SetFrench() => Loc.Instance.SetLanguage(Language.French);
    public void SetEnglish() => Loc.Instance.SetLanguage(Language.English);

    public void RefreshLocalization()
    {
        OnPropertyChanged(nameof(Languages));
        OnPropertyChanged(nameof(LogFormats));
    }
}



