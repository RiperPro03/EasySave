using EasySave.App.Repositories;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.App.Services;

/// <summary>
/// Application service for managing backup jobs.
/// </summary>
public sealed class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobService"/> class.
    /// </summary>
    /// <param name="pathProvider">Provides configuration paths.</param>
    public JobService(IPathProvider pathProvider)
        : this(new JobRepository(pathProvider))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JobService"/> class with a custom repository.
    /// </summary>
    /// <param name="jobRepository">The repository to use.</param>
    internal JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    /// <summary>
    /// Retrieves all jobs.
    /// </summary>
    /// <returns>A read-only list of jobs.</returns>
    public IReadOnlyList<BackupJob> GetAll() => _jobRepository.GetAll();

    /// <summary>
    /// Retrieves a job by identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <returns>The job, or <c>null</c> if not found.</returns>
    public BackupJob? GetById(string id) => _jobRepository.GetById(id);

    /// <summary>
    /// Creates a new job definition.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <param name="type">The backup type.</param>
    /// <param name="isActive">Whether the job is active.</param>
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

    /// <summary>
    /// Updates an existing job definition.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <param name="type">The backup type.</param>
    /// <param name="isActive">Whether the job is active.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the job does not exist.</exception>
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

    /// <summary>
    /// Marks a job as executed at a given time.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <param name="nowUtc">Optional UTC timestamp; defaults to now.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the job does not exist.</exception>
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

    /// <summary>
    /// Deletes a job by identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    public void Delete(string id) => _jobRepository.Remove(id);
}
