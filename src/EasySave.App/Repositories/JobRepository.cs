using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.App.Repositories;

public sealed class JobRepository : IJobRepository
{
    private readonly List<BackupJob> _jobs = new();

    public IReadOnlyList<BackupJob> GetAll()
    {
        return _jobs.AsReadOnly();
    }

    public BackupJob? GetById(string id)
    {
        return _jobs.FirstOrDefault(job => job.Id == id);
    }

    public void Add(BackupJob job)
    {
        _jobs.Add(job);
    }

    public void Update(BackupJob job)
    {
        var index = _jobs.FindIndex(existing => existing.Id == job.Id);
        if (index >= 0)
            _jobs[index] = job;
    }

    public void Remove(string id)
    {
        var job = GetById(id);
        if (job is not null)
            _jobs.Remove(job);
    }
}
