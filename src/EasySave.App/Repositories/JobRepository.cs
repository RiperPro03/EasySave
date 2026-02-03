using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.App.Repositories;
using System;
using EasySave.Core.Models;

public sealed class JobRepository : IJobRepository
{
    private List<BackupJob> _jobs = new();
    private const int MAX_JOBS = 5;
    
    public IReadOnlyList<BackupJob> GetAll() => _jobs.AsReadOnly();
    public BackupJob? GetById(string id)
    {
        foreach (var job in _jobs)
        {
            if (job.Id == id)
            {
                return job;
            }
        }
        return null;
    }

    public void Add(BackupJob job)
    {
        if (job == null) throw new ArgumentNullException(nameof(job));
        if (_jobs.Count >= MAX_JOBS)
        {
            throw new InvalidOperationException("Maximum number of jobs reached.");
        }
        if (string.IsNullOrWhiteSpace(job.Id))
        {
            throw new ArgumentException("Job ID cannot be null or whitespace.", nameof(job));
        }
        if (string.IsNullOrWhiteSpace(job.Name))
        {
            throw new ArgumentException("Job Name cannot be null or whitespace.", nameof(job));
        }
        if (string.IsNullOrWhiteSpace(job.SourcePath))
        {
            throw new ArgumentException("Job SourcePath cannot be null or whitespace.", nameof(job));
        }
        if (string.IsNullOrWhiteSpace(job.TargetPath))
        {
            throw new ArgumentException("Job TargetPath cannot be null or whitespace.", nameof(job));
        }

        _jobs.Add(job);
    }
    public void Remove(string id)
    {
        for (int i = 0; i < _jobs.Count; i++)
        {
            if (_jobs[i].Id == id)
            {
                _jobs.RemoveAt(i);
                return;
            }
        }
        throw new KeyNotFoundException($"Job with ID {id} not found.");
    }

    public void Update(BackupJob updatedjob)
    {
        if (updatedjob == null)
        {
            throw new ArgumentNullException(nameof(updatedjob));
        }

        for (int i = 0; i < _jobs.Count; i++)
        {
            if (_jobs[i].Id == updatedjob.Id)
            {
                _jobs[i].UpdateDefinition(updatedjob.Name, updatedjob.SourcePath, updatedjob.TargetPath, updatedjob.Type);
                return;
            }
        }
        throw new KeyNotFoundException($"Job with ID {updatedjob.Id} not found.");
    }
}
