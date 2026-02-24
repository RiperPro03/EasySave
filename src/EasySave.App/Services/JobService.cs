using EasySave.App.Repositories;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Logging;
using EasySave.Core.Models;
using EasySave.App.Utils;

namespace EasySave.App.Services;

/// <summary>
/// Application service for managing backup jobs.
/// This service acts as a link between controllers (UI) and data storage (Repository).
/// </summary>
public sealed class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IAppLogService? _logService;

    public JobService(IPathProvider pathProvider, IAppLogService? logService = null)
        : this(new JobRepository(pathProvider), logService)
    {
    }

    internal JobService(IJobRepository jobRepository, IAppLogService? logService = null)
    {
        _jobRepository = jobRepository;
        _logService = logService;
    }

    public IReadOnlyList<BackupJob> GetAll() => _jobRepository.GetAll();

    public BackupJob? GetById(string id) => _jobRepository.GetById(id);

    // MODIFIÉ : Ajout du paramètre priorityExtensions et transmission au constructeur
    public void Create(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive = true, List<string>? priorityExtensions = null) // Ajouté paramètre
    {
        var job = new BackupJob(
            id: id,
            name: name,
            sourcePath: sourcePath,
            targetPath: targetPath,
            type: type,
            isActive: isActive,
            priorityExtensions: priorityExtensions); // Ajouté ici

        _jobRepository.Add(job);
        WriteJobLog(LogEventAction.Create, job);
    }

    public void Update(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive, List<string> priorityExtensions) // Ajouté paramètre
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
            priorityExtensions: priorityExtensions, // Ajouté ici
            createdAtUtc: existing.CreatedAt,
            lastRunUtc: existing.LastRun);

        _jobRepository.Update(updated);
        WriteJobLog(LogEventAction.Update, updated);
    }

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
            priorityExtensions: existing.PriorityExtensions,
            createdAtUtc: existing.CreatedAt,
            lastRunUtc: nowUtc ?? DateTime.UtcNow);

        _jobRepository.Update(updated);
    }

    public void Delete(string id)
    {
        var existing = _jobRepository.GetById(id);
        _jobRepository.Remove(id);
        if (existing != null)
        {
            WriteJobLog(LogEventAction.Delete, existing);
        }
    }

    private void WriteJobLog(LogEventAction action, BackupJob job)
    {
        if (_logService == null)
            return;

        var eventName = action switch
        {
            LogEventAction.Create => "job.created",
            LogEventAction.Update => "job.updated",
            LogEventAction.Delete => "job.deleted",
            _ => "job.changed"
        };

        var message = action switch
        {
            LogEventAction.Create => "Job created",
            LogEventAction.Update => "Job updated",
            LogEventAction.Delete => "Job deleted",
            _ => "Job changed"
        };

        var entry = LogEntryBuilder.Create(
                eventName: eventName,
                category: LogEventCategory.Job,
                action: action,
                message: message)
            .WithLevel(LogLevel.Info)
            .WithJob(
                id: job.Id,
                name: job.Name,
                type: job.Type,
                sourcePath: ToUncOrEmpty(job.SourcePath),
                targetPath: ToUncOrEmpty(job.TargetPath),
                status: null,
                isActive: job.IsActive)
            .Build();

        _logService.Write(entry);
    }

    private static string ToUncOrEmpty(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        return UncResolver.ResolveToUncForLog(path);
    }
}