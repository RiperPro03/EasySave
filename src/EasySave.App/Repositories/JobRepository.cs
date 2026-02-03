using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.App.Repositories;
using System;
using EasySave.Core.Models;
namespace EasySave.App.Repositories {

public sealed class JobRepository : IJobRepository
{
    private List<BackupJob> jobs = new List<BackupJob>();
    private const int maxjobs = 5;
    public IReadOnlyList<BackupJob> GetAll() => jobs.AsReadOnly();

    public void Add(BackupJob job)
    {
        if (job == null) throw new ArgumentNullException(nameof(job));
        if (jobs.Count >= maxjobs)
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

        jobs.Add(job);
    }
    public void Remove(string id)
    {
        for (int i = 0; i < jobs.Count; i++)
        {
            if (jobs[i].Id == id)
            {
                jobs.RemoveAt(i);
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

        for (int i = 0; i < jobs.Count; i++)
        {
            if (jobs[i].Id == updatedjob.Id)
            {
                jobs[i].UpdateDefinition(updatedjob.Name, updatedjob.SourcePath, updatedjob.TargetPath, updatedjob.Type);
                return;
            }
        }
        throw new KeyNotFoundException($"Job with ID {updatedjob.Id} not found.");
    }
}
