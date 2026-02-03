using EasySave.App.Repositories;
using EasySave.Core.Enums;
using EasySave.Core.Models;

namespace EasySave.Tests.App;

public class JobRepositoryTests
{
    [Fact]
    public void Add_WhenOverLimit_Throws()
    {
        var repository = new JobRepository();

        for (var i = 1; i <= 5; i++)
        {
            repository.Add(CreateJob(i));
        }

        Assert.Throws<InvalidOperationException>(() => repository.Add(CreateJob(6)));
    }

    private static BackupJob CreateJob(int id)
    {
        return new BackupJob(
            id.ToString(),
            $"Job {id}",
            $"C:\\Source{id}",
            $"C:\\Target{id}",
            BackupType.Full);
    }
}

