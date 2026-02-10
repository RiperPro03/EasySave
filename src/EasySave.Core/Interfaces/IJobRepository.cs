using EasySave.Core.Models;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Persistence contract for backup jobs.
/// </summary>
public interface IJobRepository
{
    /// <summary>
    /// Retrieves all stored jobs.
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
    /// Adds a new job.
    /// </summary>
    /// <param name="job">The job to add.</param>
    void Add(BackupJob job);

    /// <summary>
    /// Updates an existing job.
    /// </summary>
    /// <param name="job">The job to update.</param>
    void Update(BackupJob job);

    /// <summary>
    /// Removes a job by its identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    void Remove(string id);

}


