using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.App.Gui.Models;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Gui.ViewModels;

/// <summary>
/// View model for the Jobs page.
/// </summary>
public sealed partial class JobsViewModel : ViewModelBase
{
    private readonly IJobService? _jobService;
    public event EventHandler<UiNotificationEventArgs>? NotificationRequested;

    public ObservableCollection<BackupJob> Jobs { get; } = new();

    [ObservableProperty]
    private int _jobsCount;

    [ObservableProperty]
    private int _activeJobsCount;

    [ObservableProperty]
    private bool _hasJobs;

    [ObservableProperty]
    private string? _lastError;

    public event Action? CreateRequested;
    public event Action<BackupJob>? EditRequested;
    public event Action? JobsChanged;

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    public JobsViewModel()
    {
        Jobs.CollectionChanged += (_, _) => UpdateDerivedState();
        SeedSampleJobs();
    }

    /// <summary>
    /// Runtime constructor.
    /// </summary>
    public JobsViewModel(IJobService jobService)
    {
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        Jobs.CollectionChanged += (_, _) => UpdateDerivedState();
        Refresh();
    }

    [RelayCommand]
    private void CreateJob()
    {
        CreateRequested?.Invoke();
    }

    [RelayCommand]
    private void EditJob(BackupJob? job)
    {
        if (job == null)
            return;

        EditRequested?.Invoke(job);
    }

    [RelayCommand]
    private void DeleteJob(BackupJob? job)
    {
        if (job == null)
            return;

        try
        {
            if (_jobService != null)
            {
                var jobName = job.Name;
                _jobService.Delete(job.Id);
                LastError = null;
                Refresh();
                NotifySuccess(string.Format(Strings.Gui_Jobs_Notify_DeletedFormat, jobName));
            }
            else
            {
                var jobName = job.Name;
                Jobs.Remove(job);
                LastError = null;
                NotifyJobsChanged();
                NotifySuccess(string.Format(Strings.Gui_Jobs_Notify_DeletedFormat, jobName));
            }
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            NotifyError(string.Format(Strings.Gui_Jobs_Notify_ErrorFormat, ex.Message));
        }
    }

    private void Refresh()
    {
        if (_jobService == null)
            return;

        try
        {
            Jobs.Clear();
            foreach (var job in _jobService.GetAll())
            {
                Jobs.Add(job);
            }

            LastError = null;
            NotifyJobsChanged();
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
        }
    }

    public void CreateFromEditor(JobEditorViewModel editor)
    {
        if (editor == null)
            return;

        try
        {
            if (_jobService != null)
            {
                var id = GenerateNextId();
                // Ajout de editor.PriorityExtensions pour la création via service
                _jobService.Create(id, editor.Name, editor.SourcePath, editor.TargetPath, editor.SelectedType, editor.IsActive, editor.PriorityExtensions);
                LastError = null;
                Refresh();
                NotifySuccess(string.Format(Strings.Gui_Jobs_Notify_CreatedFormat, editor.Name));
            }
            else
            {
                var id = GenerateNextId();
                // Ajout de editor.PriorityExtensions pour le mode démo/sans service
                Jobs.Add(new BackupJob(id, editor.Name, editor.SourcePath, editor.TargetPath, editor.SelectedType, editor.IsActive, editor.PriorityExtensions));
                LastError = null;
                NotifyJobsChanged();
                NotifySuccess(string.Format(Strings.Gui_Jobs_Notify_CreatedFormat, editor.Name));
            }
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            NotifyError(string.Format(Strings.Gui_Jobs_Notify_ErrorFormat, ex.Message));
        }
    }

    public void UpdateFromEditor(JobEditorViewModel editor)
    {
        if (editor == null || string.IsNullOrWhiteSpace(editor.JobId))
            return;

        try
        {
            if (_jobService != null)
            {
                // Ajout de editor.PriorityExtensions pour la mise à jour via service
                _jobService.Update(editor.JobId, editor.Name, editor.SourcePath, editor.TargetPath, editor.SelectedType, editor.IsActive, editor.PriorityExtensions);
                LastError = null;
                Refresh();
                NotifySuccess(string.Format(Strings.Gui_Jobs_Notify_UpdatedFormat, editor.Name));
            }
            else
            {
                var existing = Jobs.FirstOrDefault(job => job.Id == editor.JobId);
                if (existing == null)
                    return;

                // Mise à jour de la définition incluant les extensions prioritaires
                existing.UpdateDefinition(editor.Name, editor.SourcePath, editor.TargetPath, editor.SelectedType, editor.PriorityExtensions);
                if (editor.IsActive)
                    existing.Enable();
                else
                    existing.Disable();

                LastError = null;
                UpdateDerivedState();
                NotifyJobsChanged();
                NotifySuccess(string.Format(Strings.Gui_Jobs_Notify_UpdatedFormat, editor.Name));
            }
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            NotifyError(string.Format(Strings.Gui_Jobs_Notify_ErrorFormat, ex.Message));
        }
    }

    private void UpdateDerivedState()
    {
        JobsCount = Jobs.Count;
        ActiveJobsCount = Jobs.Count(job => job.IsActive);
        HasJobs = JobsCount > 0;
    }

    private string GenerateNextId()
    {
        var numericIds = Jobs
            .Select(job => job.Id)
            .Select(id => int.TryParse(id, out var value) ? (int?)value : null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        if (numericIds.Count == 0)
            return Guid.NewGuid().ToString("N");

        return (numericIds.Max() + 1).ToString();
    }

    private void NotifyJobsChanged()
    {
        JobsChanged?.Invoke();
    }

    private void NotifySuccess(string message)
        => NotificationRequested?.Invoke(this, new UiNotificationEventArgs(Strings.Gui_Nav_BackupJobs, message, UiNotificationSeverity.Success));

    private void NotifyError(string message)
        => NotificationRequested?.Invoke(this, new UiNotificationEventArgs(Strings.Gui_Nav_BackupJobs, message, UiNotificationSeverity.Error));

    private void SeedSampleJobs()
    {
        Jobs.Add(new BackupJob("1", "Documents", "C:\\Users\\Demo\\Documents", "D:\\Backups\\Docs", BackupType.Full, true));
        Jobs.Add(new BackupJob("2", "Photos", "C:\\Users\\Demo\\Pictures", "D:\\Backups\\Photos", BackupType.Differential, false));
    }
}
