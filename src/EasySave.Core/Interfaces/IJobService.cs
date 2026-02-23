using EasySave.Core.Enums;
using EasySave.Core.Models;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Application service for managing backup jobs.
/// </summary>
public interface IJobService
{
    /// <summary>
    /// Retrieves all known jobs.
    /// </summary>
    /// <returns>A read-only list of jobs.</returns>
    IReadOnlyList<BackupJob> GetAll();

    /// <summary>
    /// Retrieves a job by its identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <returns>The job, or <c>null</c> if not found.</returns>
    BackupJob? GetById(string id);

    /// <summary>
    /// Creates a new job definition.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <param name="type">The backup type.</param>
    /// <param name="isActive">Whether the job is active.</param>
    /// <param name="priorityExtensions">The priority extensions list.</param>
    void Create(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive = true, List<string>? priorityExtensions = null);

    /// <summary>
    /// Updates an existing job definition.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <param name="type">The backup type.</param>
    /// <param name="isActive">Whether the job is active.</param>
    /// <param name="priorityExtensions">The new priority extensions list.</param>
    void Update(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive, List<string> priorityExtensions);

    /// <summary>
    /// Marks a job as executed at a given time.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <param name="nowUtc">Optional UTC timestamp; uses current time when null.</param>
    void MarkExecuted(string id, DateTime? nowUtc = null);

    /// <summary>
    /// Deletes a job by its identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    void Delete(string id);
}