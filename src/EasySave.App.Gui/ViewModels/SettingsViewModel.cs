using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.App.Gui.Localization;
using EasySave.App.Gui.Models;
using EasySave.App.Services;
using EasySave.Core.Enums;
using EasySave.Core.Resources;
using EasySave.EasyLog.Options;

namespace EasySave.App.Gui.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService? _settings;
    public event EventHandler<UiNotificationEventArgs>? NotificationRequested;

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
    [ObservableProperty] private int _largeFileThresholdKb = 10000;
    
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
        LargeFileThresholdKb = settings.LargeFileThresholdKb;
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

        bool requiresRemoteServer = SelectedLogStorageMode is LogStorageMode.ServerOnly or LogStorageMode.LocalAndServer;
        if (requiresRemoteServer && string.IsNullOrWhiteSpace(LogServerHost))
        {
            NotificationRequested?.Invoke(
                this,
                new UiNotificationEventArgs(
                    Strings.Gui_Nav_Settings,
                    Strings.Gui_Settings_Notify_Error_HostRequired,
                    UiNotificationSeverity.Error));
            return;
        }

        if (!int.TryParse(LogServerPort, out var parsedServerPort) || parsedServerPort is <= 0 or > 65535)
        {
            NotificationRequested?.Invoke(
                this,
                new UiNotificationEventArgs(
                    Strings.Gui_Nav_Settings,
                    Strings.Gui_Settings_Notify_Error_PortInvalid,
                    UiNotificationSeverity.Error));
            return;
        }

        // Threshold conversion (string → int)
        int threshold = LargeFileThresholdKb > 0
            ? LargeFileThresholdKb
            : _settings.LargeFileThresholdKb;

        try
        {
            _settings.ApplySettings(
                encryptionEnabled: EncryptionEnabled,
                encryptionKey: EncryptionKey,
                language: SelectedLanguage,
                logFormat: SelectedLogFormat,
                logStorageMode: SelectedLogStorageMode,
                logServerHost: LogServerHost,
                logServerPort: parsedServerPort,
                extensionsToEncrypt: list,
                businessSoftwareProcessName: BusinessSoftwareProcessName,
                largeFileThresholdKb: threshold);
        }
        catch (Exception ex)
        {
            NotificationRequested?.Invoke(
                this,
                new UiNotificationEventArgs(
                    Strings.Gui_Nav_Settings,
                    string.Format(Strings.Gui_Settings_Notify_Error_SaveFailedFormat, ex.Message),
                    UiNotificationSeverity.Error));
            return;
        }

        LargeFileThresholdKb = threshold;

        NotificationRequested?.Invoke(
            this,
            new UiNotificationEventArgs(
                Strings.Gui_Nav_Settings,
                Strings.Gui_Settings_Notify_SaveSuccess,
                UiNotificationSeverity.Success));

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

    partial void OnLargeFileThresholdKbChanged(int value)
    {
        if (value < 1)
            LargeFileThresholdKb = 1;
    }
}



