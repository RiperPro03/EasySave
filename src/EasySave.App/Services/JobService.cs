using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.App.Services;

public sealed class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public IReadOnlyList<BackupJob> GetAll() => _jobRepository.GetAll();

    public BackupJob? GetById(string id) => _jobRepository.GetById(id);

    public void Create(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive = true)
    {
        var job = new BackupJob(
            id: id,
            name: name,
            sourcePath: sourcePath,
            targetPath: targetPath,
            type: type,
            isActive: isActive);

        _jobRepository.Add(job);
    }

    public void Update(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive)
    {
        var existing = _jobRepository.GetById(id);
        if (existing is null)
            throw new KeyNotFoundException($"Job with ID {id} not found.");

        var updated = new BackupJob(
            id: id,
            name: name,
            sourcePath: sourcePath,
            targetPath: targetPath,
            type: type,
            isActive: isActive,
            createdAtUtc: existing.CreatedAt,
            lastRunUtc: existing.LastRun);

        _jobRepository.Update(updated);
    }

    public void Delete(string id) => _jobRepository.Remove(id);
}
