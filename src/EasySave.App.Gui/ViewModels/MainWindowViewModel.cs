using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.App.Gui.Enums;
using EasySave.App.Gui.Localization;
using EasySave.App.Gui.Models;
using EasySave.App.Services;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;
using EasySave.Core.Enums;
using EasySave.Core.Resources;
using EasySave.EasyLog.Options;

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
    private readonly SettingsService? _settingsService;
    private readonly HttpClient _logHubHealthClient = new();
    private readonly Dictionary<string, JobStatus> _jobStatusesById = new(StringComparer.Ordinal);
    private CancellationTokenSource? _logHubHealthCts;
    private string? _lastLogHubHealthNotificationKey;
    private JobStatus _lastJobStatus = JobStatus.Idle;
    private string? _lastJobName;
    private bool _disposed;
    public event EventHandler<UiNotificationEventArgs>? NotificationRequested;

    public string AppVersion { get; } = "v3.0.0";
    public string AppTagline => Strings.Gui_App_Tagline;
    public string SidebarWorkspaceLabel => Strings.Gui_Sidebar_Workspace;
    public string SidebarSystemLabel => Strings.Gui_Sidebar_System;
    public string NavDashboardLabel => Strings.Gui_Nav_Dashboard;
    public string NavBackupJobsLabel => Strings.Gui_Nav_BackupJobs;
    public string NavLiveExecutionLabel => Strings.Gui_Nav_LiveExecution;
    public string NavLogsLabel => Strings.Gui_Nav_Logs;
    public string NavSettingsLabel => Strings.Gui_Nav_Settings;
    public string NavAboutLabel => Strings.Gui_Nav_About;

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

    [ObservableProperty]
    private string _logHubHealthLabel = "LogHub: local";

    [ObservableProperty]
    private string _logHubHealthDotBrush = "#7A7A7A";

    [ObservableProperty]
    private string _logHubHealthTooltip = "LocalOnly";

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
        _jobsViewModel.NotificationRequested += OnChildNotificationRequested;
        _settingsViewModel.NotificationRequested += OnChildNotificationRequested;
        Loc.Instance.PropertyChanged += OnLocalizationChanged;
        // Valeur visible en mode design/local sans checker reseau.
        LogHubHealthLabel = "LogHub: local";
        LogHubHealthDotBrush = "#7A7A7A";
        LogHubHealthTooltip = "LocalOnly";
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

        _settingsService = settingsService;
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        var logReader = new LogReaderService(
            logsPath,
            () => settingsService.LogStorageMode,
            () => settingsService.LogServerHost,
            () => settingsService.LogServerPort);
        _dashboardViewModel = new DashboardViewModel(jobService, _backupService, logReader);
        _jobsViewModel = new JobsViewModel(jobService);
        _executionViewModel = new ExecutionViewModel(jobService, _backupService);
        _logsViewModel = new LogsViewModel(logReader);
        _settingsViewModel = new SettingsViewModel(settingsService);
        _aboutViewModel = new AboutViewModel();
        _appLogService = appLogService;
        _jobsViewModel.JobsChanged += OnJobsChanged;
        _jobsViewModel.NotificationRequested += OnChildNotificationRequested;
        _settingsViewModel.NotificationRequested += OnChildNotificationRequested;
        _backupService.StateChanged += OnBackupStateChanged;
        Loc.Instance.PropertyChanged += OnLocalizationChanged;
        if (_appLogService != null)
        {
            _appLogService.LogWritten += OnLogWritten;
        }
        StartLogHubHealthMonitor();
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
        MaybeNotifyJobStateTransition(e);
        _lastJobStatus = e.State.Status;
        _lastJobName = e.State.JobName;
        CurrentState = ResolveJobStatusLabel(e.State.Status);
        if (ActiveTab == NavigationTab.Dashboard)
        {
            StatusMessage = string.Format(Strings.Gui_Status_JobFormat, e.State.JobName, ResolveJobStatusLabel(e.State.Status));
        }
        LastUpdateTime = DateTime.Now;
    }

    private void OnChildNotificationRequested(object? sender, UiNotificationEventArgs e)
    {
        NotificationRequested?.Invoke(this, e);
    }

    private void MaybeNotifyJobStateTransition(JobStateChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.State.JobId))
            return;

        var status = e.State.Status;
        if (_jobStatusesById.TryGetValue(e.State.JobId, out var previous) && previous == status)
            return;

        _jobStatusesById[e.State.JobId] = status;

        UiNotificationEventArgs? notification = status switch
        {
            JobStatus.Running => new UiNotificationEventArgs(
                Strings.Gui_Nav_LiveExecution,
                string.Format(Strings.Gui_Execution_Notify_RunningFormat, e.State.JobName),
                UiNotificationSeverity.Info),
            JobStatus.Paused => new UiNotificationEventArgs(
                Strings.Gui_Nav_LiveExecution,
                string.Format(Strings.Gui_Execution_Notify_PausedFormat, e.State.JobName),
                UiNotificationSeverity.Warning),
            JobStatus.Completed => new UiNotificationEventArgs(
                Strings.Gui_Nav_LiveExecution,
                string.Format(Strings.Gui_Execution_Notify_CompletedFormat, e.State.JobName),
                UiNotificationSeverity.Success),
            JobStatus.Error => new UiNotificationEventArgs(
                Strings.Gui_Nav_LiveExecution,
                string.IsNullOrWhiteSpace(e.State.ErrorMessage)
                    ? string.Format(Strings.Gui_Execution_Notify_ErrorSimpleFormat, e.State.JobName)
                    : string.Format(Strings.Gui_Execution_Notify_ErrorWithDetailsFormat, e.State.JobName, e.State.ErrorMessage),
                UiNotificationSeverity.Error),
            _ => null
        };

        if (notification != null)
            NotificationRequested?.Invoke(this, notification);
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

    private void OnLocalizationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "Item[]")
            return;

        if (_uiContext != null)
        {
            _uiContext.Post(_ => RefreshLocalization(), null);
            return;
        }

        RefreshLocalization();
    }

    private void RefreshLocalization()
    {
        CurrentState = ResolveJobStatusLabel(_lastJobStatus);
        OnPropertyChanged(nameof(AppTagline));
        OnPropertyChanged(nameof(SidebarWorkspaceLabel));
        OnPropertyChanged(nameof(SidebarSystemLabel));
        OnPropertyChanged(nameof(NavDashboardLabel));
        OnPropertyChanged(nameof(NavBackupJobsLabel));
        OnPropertyChanged(nameof(NavLiveExecutionLabel));
        OnPropertyChanged(nameof(NavLogsLabel));
        OnPropertyChanged(nameof(NavSettingsLabel));
        OnPropertyChanged(nameof(NavAboutLabel));
        CurrentPageTitle = ActiveTab switch
        {
            NavigationTab.Dashboard => Strings.Gui_Nav_Dashboard,
            NavigationTab.Jobs => Strings.Gui_Nav_BackupJobs,
            NavigationTab.Execution => Strings.Gui_Nav_LiveExecution,
            NavigationTab.Logs => Strings.Gui_Nav_Logs,
            NavigationTab.Settings => Strings.Gui_Nav_Settings,
            NavigationTab.About => Strings.Gui_Nav_About,
            _ => Strings.Gui_Nav_Dashboard
        };

        if (ActiveTab == NavigationTab.Dashboard && !string.IsNullOrWhiteSpace(_lastJobName))
        {
            StatusMessage = string.Format(Strings.Gui_Status_JobFormat, _lastJobName, ResolveJobStatusLabel(_lastJobStatus));
        }
        else
        {
            StatusMessage = ActiveTab switch
            {
                NavigationTab.Dashboard => Strings.Gui_Status_Overview,
                NavigationTab.Jobs => Strings.Gui_Status_ManageJobs,
                NavigationTab.Execution => Strings.Gui_Status_LiveMonitoring,
                NavigationTab.Logs => Strings.Gui_Status_ViewLogs,
                NavigationTab.Settings => Strings.Gui_Status_SettingsInfo,
                NavigationTab.About => Strings.Gui_Status_AboutInfo,
                _ => Strings.Gui_Status_Ready
            };
        }
        
        _settingsViewModel.RefreshLocalization();
        ReloadCurrentView();
        LastUpdateTime = DateTime.Now;
    }

    private void ReloadCurrentView()
    {
        var view = CurrentView;
        CurrentView = null;
        CurrentView = view;
    }

    private void StartLogHubHealthMonitor()
    {
        if (_settingsService is null)
            return;

        _logHubHealthCts?.Cancel();
        _logHubHealthCts?.Dispose();
        _logHubHealthCts = new CancellationTokenSource();
        var token = _logHubHealthCts.Token;

        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await RefreshLogHubHealthAsync(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch
                {
                    PostLogHubHealthStatus("LogHub: offline", "#FF453A", "Health check failed");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }, token);
    }

    private async Task RefreshLogHubHealthAsync(CancellationToken cancellationToken)
    {
        if (_settingsService is null)
            return;

        if (_settingsService.LogStorageMode == LogStorageMode.LocalOnly)
        {
            PostLogHubHealthStatus("LogHub: local", "#7A7A7A", "LocalOnly");
            return;
        }

        var host = _settingsService.LogServerHost;
        var port = _settingsService.LogServerPort;
        string url = $"http://{host}:{port}/health";

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(700));

        try
        {
            using var response = await _logHubHealthClient.GetAsync(url, timeoutCts.Token).ConfigureAwait(false);
            bool online = response.IsSuccessStatusCode;
            PostLogHubHealthStatus(
                online ? "LogHub: online" : "LogHub: offline",
                online ? "#30D158" : "#FF453A",
                $"{url} ({(int)response.StatusCode})");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            PostLogHubHealthStatus("LogHub: offline", "#FF453A", $"{url} (unreachable)");
        }
    }

    private void PostLogHubHealthStatus(string label, string dotBrush, string tooltip)
    {
        MaybeNotifyLogHubHealthChange(label, tooltip);

        if (_uiContext != null)
        {
            _uiContext.Post(_ =>
            {
                LogHubHealthLabel = label;
                LogHubHealthDotBrush = dotBrush;
                LogHubHealthTooltip = tooltip;
            }, null);
            return;
        }

        LogHubHealthLabel = label;
        LogHubHealthDotBrush = dotBrush;
        LogHubHealthTooltip = tooltip;
    }

    private void MaybeNotifyLogHubHealthChange(string label, string tooltip)
    {
        if (_settingsService is null)
            return;

        // No connectivity notification in LocalOnly mode.
        if (_settingsService.LogStorageMode == LogStorageMode.LocalOnly)
        {
            _lastLogHubHealthNotificationKey = null;
            return;
        }

        string key = label;
        if (string.Equals(_lastLogHubHealthNotificationKey, key, StringComparison.Ordinal))
            return;

        if (_lastLogHubHealthNotificationKey is null)
        {
            _lastLogHubHealthNotificationKey = key;
            return;
        }

        _lastLogHubHealthNotificationKey = key;

        if (label.Contains("online", StringComparison.OrdinalIgnoreCase))
        {
            NotificationRequested?.Invoke(
                this,
                new UiNotificationEventArgs(
                    Strings.Gui_LogHub_Notify_Title,
                    Strings.Gui_LogHub_Notify_Online,
                    UiNotificationSeverity.Success));
            return;
        }

        if (label.Contains("offline", StringComparison.OrdinalIgnoreCase))
        {
            NotificationRequested?.Invoke(
                this,
                new UiNotificationEventArgs(
                    Strings.Gui_LogHub_Notify_Title,
                    Strings.Gui_LogHub_Notify_Offline,
                    UiNotificationSeverity.Warning));
        }
    }

    /// <summary>
    /// Unsubscribes from events and releases managed resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _logHubHealthCts?.Cancel();
        _logHubHealthCts?.Dispose();
        _logHubHealthCts = null;
        _logHubHealthClient.Dispose();
        Loc.Instance.PropertyChanged -= OnLocalizationChanged;

        if (_backupService != null)
        {
            _backupService.StateChanged -= OnBackupStateChanged;
        }

        _jobsViewModel.JobsChanged -= OnJobsChanged;
        _jobsViewModel.NotificationRequested -= OnChildNotificationRequested;
        _settingsViewModel.NotificationRequested -= OnChildNotificationRequested;
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
