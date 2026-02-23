using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Gui.ViewModels;

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

    private string _priorityExtensionsRaw = string.Empty;
    public string PriorityExtensionsRaw
    {
        get => _priorityExtensionsRaw;
        set
        {
            if (SetProperty(ref _priorityExtensionsRaw, value))
            {
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(PriorityExtensions));
            }
        }
    }

    /// <summary>
    /// Transforme la chaîne brute en liste de strings pour le traitement Core.
    /// Nettoie les espaces et ajoute le point si manquant.
    /// </summary>
    public List<string> PriorityExtensions => PriorityExtensionsRaw
        .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(ext => ext.Trim().StartsWith(".") ? ext.Trim() : "." + ext.Trim())
        .ToList();

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

    public string DialogTitle { get; private set; } = Strings.Gui_JobEditor_CreateTitle;
    public string DialogSubtitle { get; private set; } = Strings.Gui_JobEditor_CreateSubtitle;
    public string SaveButtonText { get; private set; } = Strings.Gui_JobEditor_CreateButton;

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
            DialogTitle = Strings.Gui_JobEditor_CreateTitle,
            DialogSubtitle = Strings.Gui_JobEditor_CreateSubtitle,
            SaveButtonText = Strings.Gui_JobEditor_CreateButton
        };
    }

    public static JobEditorViewModel FromJob(BackupJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        var vm = new JobEditorViewModel
        {
            IsEditMode = true,
            DialogTitle = Strings.Gui_JobEditor_EditTitle,
            DialogSubtitle = Strings.Gui_JobEditor_EditSubtitle,
            SaveButtonText = Strings.Gui_JobEditor_SaveChanges,
            JobId = job.Id,
            Name = job.Name,
            SourcePath = job.SourcePath,
            TargetPath = job.TargetPath,
            SelectedType = job.Type,
            IsActive = job.IsActive,
            CreatedAt = job.CreatedAt.ToLocalTime(),
            LastRun = job.LastRun?.ToLocalTime()
        };

        // --- CHARGEMENT DES EXTENSIONS PRIORITAIRES ---
        vm.PriorityExtensionsRaw = job.PriorityExtensions != null
            ? string.Join(", ", job.PriorityExtensions)
            : string.Empty;

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
            NameError = Strings.Gui_JobEditor_Error_NameRequired;
            return;
        }

        if (Name.Length > NameMaxLength)
        {
            NameError = string.Format(Strings.Gui_JobEditor_Error_NameLength, NameMaxLength);
            return;
        }

        NameError = string.Empty;
    }

    private void ValidateSourcePath()
    {
        SourcePathError = string.IsNullOrWhiteSpace(SourcePath)
            ? Strings.Gui_JobEditor_Error_SourceRequired
            : string.Empty;
    }

    private void ValidateTargetPath()
    {
        TargetPathError = string.IsNullOrWhiteSpace(TargetPath)
            ? Strings.Gui_JobEditor_Error_TargetRequired
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