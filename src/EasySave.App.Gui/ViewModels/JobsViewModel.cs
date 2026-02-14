using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.App.Gui.ViewModels;

/// <summary>
/// View model for the Jobs page.
/// </summary>
public sealed partial class JobsViewModel : ViewModelBase
{
    private const int MaxJobs = 5;
    private readonly IJobService? _jobService;

    public ObservableCollection<BackupJob> Jobs { get; } = new();

    [ObservableProperty]
    private int _jobsCount;

    [ObservableProperty]
    private int _activeJobsCount;

    [ObservableProperty]
    private bool _hasJobs;

    [ObservableProperty]
    private bool _canCreateJob;

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
                _jobService.Delete(job.Id);
                LastError = null;
                Refresh();
            }
            else
            {
                Jobs.Remove(job);
                LastError = null;
                NotifyJobsChanged();
            }
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
        }
    }

    [RelayCommand]
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
                _jobService.Create(id, editor.Name, editor.SourcePath, editor.TargetPath, editor.SelectedType, editor.IsActive);
                LastError = null;
                Refresh();
            }
            else
            {
                var id = GenerateNextId();
                Jobs.Add(new BackupJob(id, editor.Name, editor.SourcePath, editor.TargetPath, editor.SelectedType, editor.IsActive));
                LastError = null;
                NotifyJobsChanged();
            }
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
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
                _jobService.Update(editor.JobId, editor.Name, editor.SourcePath, editor.TargetPath, editor.SelectedType, editor.IsActive);
                LastError = null;
                Refresh();
            }
            else
            {
                var existing = Jobs.FirstOrDefault(job => job.Id == editor.JobId);
                if (existing == null)
                    return;

                existing.UpdateDefinition(editor.Name, editor.SourcePath, editor.TargetPath, editor.SelectedType);
                if (editor.IsActive)
                    existing.Enable();
                else
                    existing.Disable();

                LastError = null;
                UpdateDerivedState();
                NotifyJobsChanged();
            }
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
        }
    }

    private void UpdateDerivedState()
    {
        JobsCount = Jobs.Count;
        ActiveJobsCount = Jobs.Count(job => job.IsActive);
        HasJobs = JobsCount > 0;
        CanCreateJob = JobsCount < MaxJobs;
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

    private void SeedSampleJobs()
    {
        Jobs.Add(new BackupJob("1", "Documents", "C:\\Users\\Demo\\Documents", "D:\\Backups\\Docs", BackupType.Full, true));
        Jobs.Add(new BackupJob("2", "Photos", "C:\\Users\\Demo\\Pictures", "D:\\Backups\\Photos", BackupType.Differential, false));
    }
}
