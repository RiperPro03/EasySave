using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.App.Gui.Enums;
using EasySave.App.Services;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;
using EasySave.Core.Enums;
using EasySave.Core.Resources;

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

    [ObservableProperty]
    private string _statusMessage = Strings.Gui_Status_Ready;

    [ObservableProperty]
    private string _currentState = Strings.Gui_JobStatus_Idle;

    [ObservableProperty]
    private DateTime _lastUpdateTime = DateTime.Now;

    [ObservableProperty]
    private string _currentPageTitle = Strings.Gui_Nav_Dashboard;

    [ObservableProperty]
    private object? _currentView;

    // Suivi de l onglet actif pour le style de la sidebar.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDashboardActive))]
    [NotifyPropertyChangedFor(nameof(IsDashboardInactive))]
    [NotifyPropertyChangedFor(nameof(IsJobsActive))]
    [NotifyPropertyChangedFor(nameof(IsJobsInactive))]
    [NotifyPropertyChangedFor(nameof(IsExecutionActive))]
    [NotifyPropertyChangedFor(nameof(IsExecutionInactive))]
    [NotifyPropertyChangedFor(nameof(IsLogsActive))]
    [NotifyPropertyChangedFor(nameof(IsLogsInactive))]
    [NotifyPropertyChangedFor(nameof(IsSettingsActive))]
    [NotifyPropertyChangedFor(nameof(IsSettingsInactive))]
    [NotifyPropertyChangedFor(nameof(IsAboutActive))]
    [NotifyPropertyChangedFor(nameof(IsAboutInactive))]
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
        _settingsViewModel = new SettingsViewModel();
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
    /// <exception cref="ArgumentNullException">Thrown when a required service is null.</exception>
    public MainWindowViewModel(
        IJobService jobService,
        IBackupService backupService,
        string? logsPath,
        SettingsService settingsService,
        IAppLogService? appLogService = null)
    {
        _uiContext = SynchronizationContext.Current;
        if (jobService is null)
            throw new ArgumentNullException(nameof(jobService));
        if (settingsService is null)
            throw new ArgumentNullException(nameof(settingsService));

        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        var logReader = new LogReaderService(logsPath);
        _dashboardViewModel = new DashboardViewModel(jobService, _backupService, logReader);
        _jobsViewModel = new JobsViewModel(jobService);
        _executionViewModel = new ExecutionViewModel(jobService, _backupService);
        _logsViewModel = new LogsViewModel(logReader);
        _settingsViewModel = new SettingsViewModel(settingsService);
        _aboutViewModel = new AboutViewModel();
        _appLogService = appLogService;
        _jobsViewModel.JobsChanged += OnJobsChanged;
        _backupService.StateChanged += OnBackupStateChanged;
        if (_appLogService != null)
        {
            _appLogService.LogWritten += OnLogWritten;
        }
        ShowDashboard();
    }

    [RelayCommand]
    private void ShowDashboard()
    {
        CurrentPageTitle = Strings.Gui_Nav_Dashboard;
        StatusMessage = Strings.Gui_Status_Overview;
        CurrentView = _dashboardViewModel;
        ActiveTab = NavigationTab.Dashboard;
        LastUpdateTime = DateTime.Now;
    }

    [RelayCommand]
    private void ShowJobs()
    {
        CurrentPageTitle = Strings.Gui_Nav_BackupJobs;
        StatusMessage = Strings.Gui_Status_ManageJobs;
        CurrentView = _jobsViewModel;
        ActiveTab = NavigationTab.Jobs;
        LastUpdateTime = DateTime.Now;
    }

    [RelayCommand]
    private void ShowExecution()
    {
        CurrentPageTitle = Strings.Gui_Nav_LiveExecution;
        StatusMessage = Strings.Gui_Status_LiveMonitoring;
        CurrentView = _executionViewModel;
        ActiveTab = NavigationTab.Execution;
        LastUpdateTime = DateTime.Now;
    }

    [RelayCommand]
    private void ShowLogs()
    {
        CurrentPageTitle = Strings.Gui_Nav_Logs;
        StatusMessage = Strings.Gui_Status_ViewLogs;
        CurrentView = _logsViewModel;
        ActiveTab = NavigationTab.Logs;
        LastUpdateTime = DateTime.Now;
    }

    [RelayCommand]
    private void ShowSettings()
    {
        CurrentPageTitle = Strings.Gui_Nav_Settings;
        StatusMessage = Strings.Gui_Status_SettingsInfo;
        CurrentView = _settingsViewModel;
        ActiveTab = NavigationTab.Settings;
        LastUpdateTime = DateTime.Now;
    }

    [RelayCommand]
    private void ShowAbout()
    {
        CurrentPageTitle = Strings.Gui_Nav_About;
        StatusMessage = Strings.Gui_Status_AboutInfo;
        CurrentView = _aboutViewModel;
        ActiveTab = NavigationTab.About;
        LastUpdateTime = DateTime.Now;
    }
    
    private void OnBackupStateChanged(object? sender, JobStateChangedEventArgs e)
    {
        CurrentState = ResolveJobStatusLabel(e.State.Status);
        if (ActiveTab == NavigationTab.Dashboard)
        {
            StatusMessage = string.Format(Strings.Gui_Status_JobFormat, e.State.JobName, ResolveJobStatusLabel(e.State.Status));
        }
        LastUpdateTime = DateTime.Now;
    }

    private void OnJobsChanged()
    {
        _dashboardViewModel.RefreshJobSummary();
        _executionViewModel.RefreshJobs();
    }

    private void OnLogWritten(object? sender, EventArgs e)
    {
        if (_uiContext != null)
        {
            _uiContext.Post(_ => HandleLogWritten(), null);
            return;
        }

        HandleLogWritten();
    }

    private void HandleLogWritten()
    {
        _dashboardViewModel.NotifyLogWritten();
        _logsViewModel.RefreshLogs();
    }

    /// <summary>
    /// Unsubscribes from events and releases managed resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_backupService != null)
        {
            _backupService.StateChanged -= OnBackupStateChanged;
        }

        _jobsViewModel.JobsChanged -= OnJobsChanged;
        if (_appLogService != null)
        {
            _appLogService.LogWritten -= OnLogWritten;
        }
        _dashboardViewModel.Dispose();
        _executionViewModel.Dispose();
        GC.SuppressFinalize(this);
    }

    private static string ResolveJobStatusLabel(JobStatus status)
    {
        return status switch
        {
            JobStatus.Idle => Strings.Gui_JobStatus_Idle,
            JobStatus.Running => Strings.Gui_JobStatus_Running,
            JobStatus.Paused => Strings.Gui_JobStatus_Paused,
            JobStatus.Completed => Strings.Gui_JobStatus_Completed,
            JobStatus.Error => Strings.Gui_JobStatus_Error,
            _ => Strings.Gui_Common_Unknown
        };
    }
}
