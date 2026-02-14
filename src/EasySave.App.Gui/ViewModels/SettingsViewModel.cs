using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.App.Services;
using EasySave.Core.Enums;
using EasySave.EasyLog.Options;

namespace EasySave.App.Gui.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settings;

    // Propriétés automatiques (le toolkit crée le reste pour nous)
    [ObservableProperty] private bool _encryptionEnabled;
    [ObservableProperty] private string _encryptionKey = string.Empty;
    [ObservableProperty] private Language _selectedLanguage;
    [ObservableProperty] private LogFormat _selectedLogFormat;

    // Listes pour remplir les menus déroulants (ComboBox)
    public Language[] Languages => Enum.GetValues<Language>();
    public LogFormat[] LogFormats => Enum.GetValues<LogFormat>();

    public SettingsViewModel(SettingsService settings)
    {
        _settings = settings;

        // Si settings est null (mode design), on ne charge rien
        if (settings == null) return; 

        EncryptionEnabled = settings.EncryptionEnabled;
        EncryptionKey = settings.EncryptionKey;
        SelectedLanguage = settings.Language;
        SelectedLogFormat = settings.LogFormat;
    }
    // Sauvegarde auto quand on change d'option
    partial void OnSelectedLanguageChanged(Language value) => _settings.UpdateLanguage(value);
    partial void OnSelectedLogFormatChanged(LogFormat value) => _settings.UpdateLogFormat(value);

    [RelayCommand]
    private void SaveSettings()
    {
        _settings.UpdateEncryptionKey(EncryptionKey);
        if (EncryptionEnabled != _settings.EncryptionEnabled) _settings.ToggleEncryption();
    }
}