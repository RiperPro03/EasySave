using EasySave.Core.Enums;
using EasySave.Core.Models;

namespace EasySave.Core.DTO;

/// <summary>
/// Persistence DTO for a backup job (JSON/disk).
/// Represents external data that may be invalid.
/// </summary>
public sealed class BackupJobDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? SourcePath { get; set; }
    public string? TargetPath { get; set; }

    /// <summary>
    /// Gets or sets the backup type stored as a string for JSON compatibility.
    /// </summary>
    public string? Type { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastRun { get; set; }
    public bool EncryptFiles { get; set; } = false;
    public string? EncryptionKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupJobDto"/> class.
    /// </summary>
    /// <remarks>Required by some JSON/XML serializers.</remarks>
    public BackupJobDto() { }

    /// <summary>
    /// Checks minimal consistency of data loaded from persistence.
    /// </summary>
    /// <returns>
    /// <c>true</c> when the DTO contains the minimum required fields; otherwise <c>false</c>.
    /// </returns>
    public bool IsValid()
        => !string.IsNullOrWhiteSpace(Id)
        && !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(SourcePath)
        && !string.IsNullOrWhiteSpace(TargetPath)
        && !string.IsNullOrWhiteSpace(Type)
        && Enum.TryParse<BackupType>(Type, out _);

    /// <summary>
    /// Converts the DTO to a domain model.
    /// </summary>
    /// <returns>The corresponding <see cref="BackupJob"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the DTO is invalid or the backup type cannot be parsed.
    /// </exception>
    public BackupJob ToModel()
    {
        if (!IsValid())
            throw new ArgumentException("Invalid BackupJobDto. Cannot convert to BackupJob.");

        if (!Enum.TryParse<BackupType>(Type, out var backupType))
            throw new ArgumentException($"Invalid BackupType value: {Type}");

        return new BackupJob(
            id: Id!,
            name: Name!,
            sourcePath: SourcePath!,
            targetPath: TargetPath!,
            type: backupType,
            isActive: IsActive,
            createdAtUtc: CreatedAt,
            lastRunUtc: LastRun,
            encryptFiles: EncryptFiles,
            encryptionKey: EncryptionKey
        );
    }

    /// <summary>
    /// Creates a DTO from a valid domain model.
    /// </summary>
    /// <param name="job">The domain model to convert.</param>
    /// <returns>A DTO representing the provided job.</returns>
    public static BackupJobDto FromModel(BackupJob job)
    {
        return new BackupJobDto
        {
            Id = job.Id,
            Name = job.Name,
            SourcePath = job.SourcePath,
            TargetPath = job.TargetPath,
            Type = job.Type.ToString(),
            IsActive = job.IsActive,
            CreatedAt = job.CreatedAt,
            LastRun = job.LastRun,
            EncryptFiles = job.EncryptFiles,
            EncryptionKey = job.EncryptionKey
        };
    }
}
