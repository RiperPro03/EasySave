using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using System.Text.Json;

namespace EasySave.App.Repositories;

/// <summary>
/// JSON-backed repository for backup jobs.
/// </summary>
internal sealed class JobRepository : IJobRepository
{
    private readonly IPathProvider _pathProvider;
    private readonly List<BackupJob> _jobs;
    private readonly string _jobsFilePath;
    private const int MaxJobs = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobRepository"/> class.
    /// </summary>
    /// <param name="pathProvider">Provides configuration paths.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pathProvider"/> is null.</exception>
    public JobRepository(IPathProvider pathProvider)
    {
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _pathProvider.EnsureDirectoriesCreated();

        _jobsFilePath = Path.Combine(_pathProvider.ConfigPath, "jobs.json");
        _jobs = LoadJobs();
    }

    /// <summary>
    /// Returns all persisted jobs.
    /// </summary>
    /// <returns>A read-only list of jobs.</returns>
    public IReadOnlyList<BackupJob> GetAll() => _jobs.AsReadOnly();

    /// <summary>
    /// Gets a job by identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <returns>The matching job, or <c>null</c> if not found.</returns>
    public BackupJob? GetById(string id) => _jobs.FirstOrDefault(job => job.Id == id);

    /// <summary>
    /// Adds a new job.
    /// </summary>
    /// <param name="job">The job to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the maximum job count is reached or the ID already exists.
    /// </exception>
    public void Add(BackupJob job)
    {
        if (job is null)
            throw new ArgumentNullException(nameof(job));

        if (_jobs.Count >= MaxJobs)
        {
            throw new InvalidOperationException("Maximum number of jobs reached.");
        }

        if (_jobs.Any(existing => existing.Id == job.Id))
            throw new InvalidOperationException($"Job with ID {job.Id} already exists.");

        _jobs.Add(job);
        SaveJobs();
    }
    /// <summary>
    /// Removes a job by identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the job does not exist.</exception>
    public void Remove(string id)
    {
        var job = GetById(id);
        if (job is null)
            throw new KeyNotFoundException($"Job with ID {id} not found.");

        _jobs.Remove(job);
        SaveJobs();
    }

    /// <summary>
    /// Updates an existing job.
    /// </summary>
    /// <param name="updatedjob">The updated job definition.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="updatedjob"/> is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the job does not exist.</exception>
    public void Update(BackupJob updatedjob)
    {
        if (updatedjob == null)
        {
            throw new ArgumentNullException(nameof(updatedjob));
        }

        var existingJob = GetById(updatedjob.Id);
        if (existingJob is null)
            throw new KeyNotFoundException($"Job with ID {updatedjob.Id} not found.");

        existingJob.UpdateDefinition(updatedjob.Name, updatedjob.SourcePath, updatedjob.TargetPath, updatedjob.Type);
        if (updatedjob.IsActive)
            existingJob.Enable();
        else
            existingJob.Disable();

        if (updatedjob.LastRun is not null)
        {
            existingJob.MarkExecuted(updatedjob.LastRun);
        }

        SaveJobs();
    }

    /// <summary>
    /// Loads jobs from the JSON file.
    /// </summary>
    /// <returns>A list of valid jobs.</returns>
    private List<BackupJob> LoadJobs()
    {
        if (!File.Exists(_jobsFilePath))
            return new List<BackupJob>();

        var json = File.ReadAllText(_jobsFilePath);
        if (string.IsNullOrWhiteSpace(json))
            return new List<BackupJob>();

        List<BackupJobDto> dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<List<BackupJobDto>>(json) ?? new List<BackupJobDto>();
        }
        catch (JsonException)
        {
            return new List<BackupJob>();
        }
        var jobs = new List<BackupJob>();

        foreach (var dto in dtos)
        {
            if (dto is null || !dto.IsValid())
                continue;

            try
            {
                jobs.Add(dto.ToModel());
            }
            catch (ArgumentException)
            {
                // Ignore invalid persisted entries.
            }
        }

        return jobs;
    }

    /// <summary>
    /// Persists jobs to the JSON file.
    /// </summary>
    private void SaveJobs()
    {
        _pathProvider.EnsureDirectoriesCreated();

        var dtos = _jobs.Select(BackupJobDto.FromModel).ToList();
        var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_jobsFilePath, json);
    }


}