using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using System.Linq;

namespace EasySave.App.Repositories;

public sealed class JobRepository : IJobRepository
{
    // TODO(Gestion des chemins): remplacer cette implémentation in-memory
    // par une version persistée (ex: FileJobRepository) lorsqu'on aura défini
    // les chemins Logs/State/Config.
    private readonly List<BackupJob> _jobs = new();
    private const int MaxJobs = 5;
    
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
    }
    public void Remove(string id)
    {
        var job = GetById(id);
        if (job is null)
            throw new KeyNotFoundException($"Job with ID {id} not found.");

        _jobs.Remove(job);
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
    }
}
