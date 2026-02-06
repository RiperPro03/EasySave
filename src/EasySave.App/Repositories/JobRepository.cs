using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using System.Text.Json;

namespace EasySave.App.Repositories;

internal sealed class JobRepository : IJobRepository
{
    private readonly IPathProvider _pathProvider;
    private readonly List<BackupJob> _jobs;
    private readonly string _jobsFilePath;
    private const int MaxJobs = 5;

    public JobRepository(IPathProvider pathProvider)
    {
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _pathProvider.EnsureDirectoriesCreated();

        _jobsFilePath = Path.Combine(_pathProvider.ConfigPath, "jobs.json");
        _jobs = LoadJobs();
    }

    public IReadOnlyList<BackupJob> GetAll() => _jobs.AsReadOnly();
    public BackupJob? GetById(string id) => _jobs.FirstOrDefault(job => job.Id == id);

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
    public void Remove(string id)
    {
        var job = GetById(id);
        if (job is null)
            throw new KeyNotFoundException($"Job with ID {id} not found.");

        _jobs.Remove(job);
        SaveJobs();
    }

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

    private void SaveJobs()
    {
        _pathProvider.EnsureDirectoriesCreated();

        var dtos = _jobs.Select(BackupJobDto.FromModel).ToList();
        var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_jobsFilePath, json);
    }
    

}
