using EasySave.App.Repositories;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.App.Services;

// Ce service fait le lien entre les contrôleurs (UI) et le stockage des données (Repository)
public sealed class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IPathProvider pathProvider)
        : this(new JobRepository(pathProvider))
    {
    }

    internal JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public IReadOnlyList<BackupJob> GetAll() => _jobRepository.GetAll();

    public BackupJob? GetById(string id) => _jobRepository.GetById(id);

    // Crée un nouvel objet BackupJob et demande au Repository de l'enregistrer
    public void Create(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive = true)
    {
        var job = new BackupJob(
            id: id,
            name: name,
            sourcePath: sourcePath,
            targetPath: targetPath,
            type: type,
            isActive: isActive);

        _jobRepository.Add(job);
    }

    // Met à jour un travail existant tout en conservant sa date de création d'origine
    public void Update(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive)
    {
        var existing = _jobRepository.GetById(id);
        if (existing is null)
            throw new KeyNotFoundException($"Job with ID {id} not found.");

        var updated = new BackupJob(
            id: id,
            name: name,
            sourcePath: sourcePath,
            targetPath: targetPath,
            type: type,
            isActive: isActive,
            createdAtUtc: existing.CreatedAt,
            lastRunUtc: existing.LastRun);

        _jobRepository.Update(updated);
    }

    // Spécifiquement utilisé pour mettre à jour l'horodatage après une sauvegarde réussie
    public void MarkExecuted(string id, DateTime? nowUtc = null)
    {
        var existing = _jobRepository.GetById(id);
        if (existing is null)
            throw new KeyNotFoundException($"Job with ID {id} not found.");

        var updated = new BackupJob(
            id: existing.Id,
            name: existing.Name,
            sourcePath: existing.SourcePath,
            targetPath: existing.TargetPath,
            type: existing.Type,
            isActive: existing.IsActive,
            createdAtUtc: existing.CreatedAt,
            lastRunUtc: nowUtc ?? DateTime.UtcNow);

        _jobRepository.Update(updated);
    }

    public void Delete(string id) => _jobRepository.Remove(id);
}
