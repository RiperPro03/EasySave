using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Enums;
using EasySave.Core.Models;

namespace EasySave.App.Gui.ViewModels;

/// <summary>
/// View model for creating or editing a backup job.
/// </summary>
public sealed partial class JobEditorViewModel : ViewModelBase
{
    private const int NameMaxLength = 50;

    public event EventHandler<bool?>? CloseRequested;

    public string JobId { get; private set; } = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _sourcePath = string.Empty;

    [ObservableProperty]
    private string _targetPath = string.Empty;

    [ObservableProperty]
    private BackupType _selectedType = BackupType.Full;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNameError))]
    private string _nameError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSourcePathError))]
    private string _sourcePathError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTargetPathError))]
    private string _targetPathError = string.Empty;

    public bool HasNameError => !string.IsNullOrWhiteSpace(NameError);
    public bool HasSourcePathError => !string.IsNullOrWhiteSpace(SourcePathError);
    public bool HasTargetPathError => !string.IsNullOrWhiteSpace(TargetPathError);

    public IReadOnlyList<BackupType> AvailableTypes { get; } = new[]
    {
        BackupType.Full,
        BackupType.Differential
    };

    public bool IsEditMode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastRun { get; private set; }

    public string DialogTitle { get; private set; } = "Create Job";
    public string DialogSubtitle { get; private set; } = "Define a new backup job";
    public string SaveButtonText { get; private set; } = "Create Job";

    public bool CanSave =>
        !HasNameError &&
        !HasSourcePathError &&
        !HasTargetPathError &&
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(SourcePath) &&
        !string.IsNullOrWhiteSpace(TargetPath);

    public JobEditorViewModel()
    {
        CreatedAt = DateTime.Now;
        ValidateAll();
    }

    public static JobEditorViewModel CreateNew()
    {
        return new JobEditorViewModel
        {
            IsEditMode = false,
            DialogTitle = "Create Job",
            DialogSubtitle = "Define a new backup job",
            SaveButtonText = "Create Job"
        };
    }

    public static JobEditorViewModel FromJob(BackupJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        var vm = new JobEditorViewModel
        {
            IsEditMode = true,
            DialogTitle = "Edit Job",
            DialogSubtitle = "Update the backup job settings",
            SaveButtonText = "Save Changes",
            JobId = job.Id,
            Name = job.Name,
            SourcePath = job.SourcePath,
            TargetPath = job.TargetPath,
            SelectedType = job.Type,
            IsActive = job.IsActive,
            CreatedAt = job.CreatedAt.ToLocalTime(),
            LastRun = job.LastRun?.ToLocalTime()
        };

        vm.ValidateAll();
        return vm;
    }

    [RelayCommand]
    private void Save()
    {
        ValidateAll();
        if (!CanSave)
            return;

        CloseRequested?.Invoke(this, true);
    }

    private void ValidateAll()
    {
        ValidateName();
        ValidateSourcePath();
        ValidateTargetPath();
        OnPropertyChanged(nameof(CanSave));
    }

    private void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Name is required.";
            return;
        }

        if (Name.Length > NameMaxLength)
        {
            NameError = $"Name must be under {NameMaxLength} characters.";
            return;
        }

        NameError = string.Empty;
    }

    private void ValidateSourcePath()
    {
        SourcePathError = string.IsNullOrWhiteSpace(SourcePath)
            ? "Source path is required."
            : string.Empty;
    }

    private void ValidateTargetPath()
    {
        TargetPathError = string.IsNullOrWhiteSpace(TargetPath)
            ? "Target path is required."
            : string.Empty;
    }

    partial void OnNameChanged(string value)
    {
        ValidateName();
        OnPropertyChanged(nameof(CanSave));
    }

    partial void OnSourcePathChanged(string value)
    {
        ValidateSourcePath();
        OnPropertyChanged(nameof(CanSave));
    }

    partial void OnTargetPathChanged(string value)
    {
        ValidateTargetPath();
        OnPropertyChanged(nameof(CanSave));
    }

    partial void OnSelectedTypeChanged(BackupType value)
    {
        OnPropertyChanged(nameof(CanSave));
    }
}
