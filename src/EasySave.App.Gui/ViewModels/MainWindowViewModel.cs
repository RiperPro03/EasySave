using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.App.Gui.Enums;
using EasySave.App.Services;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;

namespace EasySave.App.Gui.ViewModels;

/// <summary>
/// Coordinates navigation state and top-level UI bindings.
/// </summary>
/// <remarks>
/// Keeps one instance of each view model to preserve UI state between tabs.
/// </remarks>
public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IBackupService? _backupService;
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly JobsViewModel _jobsViewModel;
    private readonly ExecutionViewModel _executionViewModel;
    private readonly LogsViewModel _logsViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly AboutViewModel _aboutViewModel;
    private readonly IAppLogService? _appLogService;
    private readonly SynchronizationContext? _uiContext;
    private bool _disposed;

    public string AppVersion { get; } = "v2.0.0";

    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private string _currentState = "Idle";
    [ObservableProperty] private DateTime _lastUpdateTime = DateTime.Now;
    [ObservableProperty] private string _currentPageTitle = "Dashboard";
    [ObservableProperty] private object? _currentView;

    // Suivi de l onglet actif pour le style de la sidebar.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDashboardActive)), NotifyPropertyChangedFor(nameof(IsDashboardInactive))]
    [NotifyPropertyChangedFor(nameof(IsJobsActive)), NotifyPropertyChangedFor(nameof(IsJobsInactive))]
    [NotifyPropertyChangedFor(nameof(IsExecutionActive)), NotifyPropertyChangedFor(nameof(IsExecutionInactive))]
    [NotifyPropertyChangedFor(nameof(IsLogsActive)), NotifyPropertyChangedFor(nameof(IsLogsInactive))]
    [NotifyPropertyChangedFor(nameof(IsSettingsActive)), NotifyPropertyChangedFor(nameof(IsSettingsInactive))]
    [NotifyPropertyChangedFor(nameof(IsAboutActive)), NotifyPropertyChangedFor(nameof(IsAboutInactive))]
    private NavigationTab _activeTab = NavigationTab.Dashboard;

    public bool IsDashboardActive => ActiveTab == NavigationTab.Dashboard;
    public bool IsDashboardInactive => !IsDashboardActive;
    public bool IsJobsActive => ActiveTab == NavigationTab.Jobs;
    public bool IsJobsInactive => !IsJobsActive;
    public bool IsExecutionActive => ActiveTab == NavigationTab.Execution;
    public bool IsExecutionInactive => !IsExecutionActive;
    public bool IsLogsActive => ActiveTab == NavigationTab.Logs;
    public bool IsLogsInactive => !IsLogsActive;
    public bool IsSettingsActive => ActiveTab == NavigationTab.Settings;
    public bool IsSettingsInactive => !IsSettingsActive;
    public bool IsAboutActive => ActiveTab == NavigationTab.About;
    public bool IsAboutInactive => !IsAboutActive;

    /// <summary>
    /// Creates a design-time instance with placeholder view models.
    /// </summary>
    public MainWindowViewModel()
    {
        _uiContext = SynchronizationContext.Current;
        _dashboardViewModel = new DashboardViewModel();
        _jobsViewModel = new JobsViewModel();
        _executionViewModel = new ExecutionViewModel();
        _logsViewModel = new LogsViewModel();
        
        // FIX KISS : On passe null! ici pour éviter l'erreur de paramètre obligatoire
        _settingsViewModel = new SettingsViewModel(null!); 
        
        _aboutViewModel = new AboutViewModel();
        _jobsViewModel.JobsChanged += OnJobsChanged;
        ShowDashboard();
    }

    /// <summary>
    /// Creates a runtime instance wired to core services.
    /// </summary>
    /// <param name="jobService">Job service used by the dashboard.</param>
    /// <param name="backupService">Backup service that publishes state updates.</param>
    /// <param name="logsPath">Directory containing log files.</param>
    /// <param name="settingsService">Service for application settings.</param>
    /// <param name="appLogService">Service for application logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when a required service is null.</exception>
    public MainWindowViewModel(
        IJobService jobService,
        IBackupService backupService,
        string? logsPath,
        SettingsService settingsService, // <--- AJOUT POUR TES SETTINGS
        IAppLogService? appLogService = null)
    {
        _uiContext = SynchronizationContext.Current;
        if (jobService is null) throw new ArgumentNullException(nameof(jobService));

        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        var logReader = new LogReaderService(logsPath);
        _dashboardViewModel = new DashboardViewModel(jobService, _backupService, logReader);
        _jobsViewModel = new JobsViewModel(jobService);
        _executionViewModel = new ExecutionViewModel();
        _logsViewModel = new LogsViewModel(logReader);
        
        // FIX KISS : On branche ton service ici
        _settingsViewModel = new SettingsViewModel(settingsService); 
        
        _aboutViewModel = new AboutViewModel();
        _appLogService = appLogService;
        _jobsViewModel.JobsChanged += OnJobsChanged;
        _backupService.StateChanged += OnBackupStateChanged;
        if (_appLogService != null) _appLogService.LogWritten += OnLogWritten;
        ShowDashboard();
    }

    [RelayCommand] private void ShowDashboard() { CurrentPageTitle = "Dashboard"; StatusMessage = "Overview"; CurrentView = _dashboardViewModel; ActiveTab = NavigationTab.Dashboard; LastUpdateTime = DateTime.Now; }
    [RelayCommand] private void ShowJobs() { CurrentPageTitle = "Backup Jobs"; StatusMessage = "Manage jobs"; CurrentView = _jobsViewModel; ActiveTab = NavigationTab.Jobs; LastUpdateTime = DateTime.Now; }
    [RelayCommand] private void ShowExecution() { CurrentPageTitle = "Live Execution"; StatusMessage = "Monitoring"; CurrentView = _executionViewModel; ActiveTab = NavigationTab.Execution; LastUpdateTime = DateTime.Now; }
    [RelayCommand] private void ShowLogs() { CurrentPageTitle = "Logs"; StatusMessage = "View logs"; CurrentView = _logsViewModel; ActiveTab = NavigationTab.Logs; LastUpdateTime = DateTime.Now; }
    [RelayCommand] private void ShowSettings() { CurrentPageTitle = "Settings"; StatusMessage = "Configuration"; CurrentView = _settingsViewModel; ActiveTab = NavigationTab.Settings; LastUpdateTime = DateTime.Now; }
    [RelayCommand] private void ShowAbout() { CurrentPageTitle = "About"; StatusMessage = "Info"; CurrentView = _aboutViewModel; ActiveTab = NavigationTab.About; LastUpdateTime = DateTime.Now; }
    
    private void OnBackupStateChanged(object? sender, JobStateChangedEventArgs e) { CurrentState = e.State.Status.ToString(); StatusMessage = $"{e.State.JobName}: {e.State.Status}"; LastUpdateTime = DateTime.Now; }
    private void OnJobsChanged() => _dashboardViewModel.RefreshJobSummary();
    private void OnLogWritten(object? sender, EventArgs e) { if (_uiContext != null) _uiContext.Post(_ => HandleLogWritten(), null); else HandleLogWritten(); }
    private void HandleLogWritten() { _dashboardViewModel.NotifyLogWritten(); _logsViewModel.RefreshLogs(); }

    /// <summary>
    /// Unsubscribes from events and releases managed resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_backupService != null) _backupService.StateChanged -= OnBackupStateChanged;
        _jobsViewModel.JobsChanged -= OnJobsChanged;
        if (_appLogService != null) _appLogService.LogWritten -= OnLogWritten;
        _dashboardViewModel.Dispose();
        GC.SuppressFinalize(this);
    }
}