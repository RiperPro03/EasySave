using System;
using System.Linq;
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
    [ObservableProperty] private string _excludedExtensions = string.Empty;
    
    // Listes pour remplir les menus déroulants (ComboBox)
    public Language[] Languages => Enum.GetValues<Language>();
    public LogFormat[] LogFormats => Enum.GetValues<LogFormat>();

    public SettingsViewModel(SettingsService settings)
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
        // Si ton service possède une propriété ExcludedExtensions (List<string>)
        ExcludedExtensions = string.Join(", ", settings.ExcludedExtensions);
    }
    // Sauvegarde auto quand on change d'option
    partial void OnSelectedLanguageChanged(Language value) => _settings.UpdateLanguage(value);
    partial void OnSelectedLogFormatChanged(LogFormat value) => _settings.UpdateLogFormat(value);

    // Cette méthode est appelée automatiquement quand l'utilisateur change le texte
    partial void OnExcludedExtensionsChanged(string value)
    {
        // On transforme la chaîne en liste : on sépare par virgule, on enlève les espaces et les vides
        var list = value.Split(',')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        
        _settings.UpdateExcludedExtensions(list);
    }
    
    [RelayCommand]
    private void SaveSettings()
    {
        _settings.UpdateEncryptionKey(EncryptionKey);
        if (EncryptionEnabled != _settings.EncryptionEnabled) _settings.ToggleEncryption();
    }
}