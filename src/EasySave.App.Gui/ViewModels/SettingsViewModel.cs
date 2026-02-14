using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.App.Services;
using EasySave.Core.Enums;
using EasySave.EasyLog.Options;

namespace EasySave.App.Gui.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public Language[] Languages { get; } = Enum.GetValues<Language>();
    public LogFormat[] LogFormats { get; } = Enum.GetValues<LogFormat>();

    private readonly SettingsService _settings;

    [ObservableProperty] private bool _encryptionEnabled;
    [ObservableProperty] private string _encryptionKey = string.Empty;
    [ObservableProperty] private Language _language;
    [ObservableProperty] private LogFormat _logFormat;

    public SettingsViewModel(SettingsService settings)
    {
        _settings = settings;

        EncryptionEnabled = settings.EncryptionEnabled;
        EncryptionKey = settings.EncryptionKey;
        Language = settings.Language;
        LogFormat = settings.LogFormat;
    }

    // --- Commands ---

    [RelayCommand]
    private void ToggleEncryption()
    {
        _settings.ToggleEncryption();
        EncryptionEnabled = _settings.EncryptionEnabled;
    }

    [RelayCommand]
    private void SaveEncryptionKey()
    {
        _settings.UpdateEncryptionKey(EncryptionKey);
    }

    [RelayCommand]
    private void ChangeLanguage(Language lang)
    {
        _settings.UpdateLanguage(lang);
        Language = lang;
    }

    [RelayCommand]
    private void ChangeLogFormat(LogFormat format)
    {
        _settings.UpdateLogFormat(format);
        LogFormat = format;
    }
}