using EasySave.Core.Common;
using EasySave.Core.Enums;

namespace EasySave.Core.Models;

/// <summary>
/// Represents a backup job in the domain.
/// </summary>
public sealed class BackupJob
{
    public string Id { get; }
    public string Name { get; private set; }
    public string SourcePath { get; private set; }
    public string TargetPath { get; private set; }
    public BackupType Type { get; private set; }

    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? LastRun { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupJob"/> class.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <param name="type">The backup type.</param>
    /// <param name="isActive">Whether the job is active.</param>
    /// <param name="createdAtUtc">Optional creation time in UTC.</param>
    /// <param name="lastRunUtc">Optional last run time in UTC.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when any required string is null, empty, or whitespace.
    /// </exception>
    public BackupJob(
        string id,
        string name,
        string sourcePath,
        string targetPath,
        BackupType type,
        bool isActive = true,
        DateTime? createdAtUtc = null,
        DateTime? lastRunUtc = null)
    {
        Id = Guard.NotNullOrWhiteSpace(id, nameof(id));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        SourcePath = Guard.NotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        TargetPath = Guard.NotNullOrWhiteSpace(targetPath, nameof(targetPath));

        Type = type;

        IsActive = isActive;
        // Normalize timestamps to UTC for consistent persistence.
        CreatedAt = (createdAtUtc ?? DateTime.UtcNow).ToUniversalTime();
        LastRun = lastRunUtc?.ToUniversalTime();
    }

    /// <summary>
    /// Activates the job.
    /// </summary>
    public void Enable() => IsActive = true;

    /// <summary>
    /// Deactivates the job.
    /// </summary>
    public void Disable() => IsActive = false;

    /// <summary>
    /// Updates the last run timestamp.
    /// </summary>
    /// <param name="nowUtc">Optional UTC time; defaults to <see cref="DateTime.UtcNow"/>.</param>
    public void MarkExecuted(DateTime? nowUtc = null)
        => LastRun = (nowUtc ?? DateTime.UtcNow).ToUniversalTime();

    /// <summary>
    /// Updates the main job fields.
    /// </summary>
    /// <param name="name">The display name.</param>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
    /// <param name="type">The backup type.</param>
    public void UpdateDefinition(string name, string sourcePath, string targetPath, BackupType type)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        SourcePath = Guard.NotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        TargetPath = Guard.NotNullOrWhiteSpace(targetPath, nameof(targetPath));
        Type = type;
    }
    
    /// <summary>
    /// Returns a human-readable representation of the job.
    /// </summary>
    /// <returns>A formatted job summary string.</returns>
    public override string ToString()
        => $"{Name} ({Type}) | {SourcePath} -> {TargetPath} | Active={IsActive}";
}
