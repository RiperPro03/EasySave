using EasySave.App.Console.Input;
using EasySave.Core.DTO;
using EasySave.Core.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Views;

public sealed class BackupView
{
    private readonly ConsoleInput _input;

    public BackupView(ConsoleInput input)
    {
        _input = input;
    }

    public void ShowBackupMenu()
    {
        System.Console.WriteLine();
        System.Console.WriteLine(Strings.UI_BackupTitle);
        System.Console.WriteLine($"1 - {Strings.UI_RunOneJob}");
        System.Console.WriteLine($"2 - {Strings.UI_RunAllJobs}");
        System.Console.WriteLine($"0 - {Strings.UI_Back}");
    }

    public int ReadMenuChoice()
    {
        return _input.ReadInt("> ");
    }

    public void ShowJobs(IReadOnlyList<BackupJob> jobs)
    {
        System.Console.WriteLine();
        if (jobs.Count == 0)
        {
            System.Console.WriteLine(Strings.UI_NoJobsConfigured);
            return;
        }

        foreach (var job in jobs)
        {
            System.Console.WriteLine($"{job.Id} - {job}");
        }
    }

    public int AskJobId()
    {
        return _input.ReadInt(Strings.UI_JobIdPrompt);
    }

    public void ShowRunStart(BackupJob job)
    {
        System.Console.WriteLine(string.Format(Strings.UI_RunStart, job.Id, job.Name));
    }

    public void ShowRunEnd(BackupResultDto result)
    {
        var status = result.Success ? Strings.UI_ResultSuccess : Strings.UI_ResultFailed;
        System.Console.WriteLine(string.Format(Strings.UI_Result, status, result.Message));
    }

    public void ShowBatchResult(IReadOnlyList<BackupResultDto> results)
    {
        if (results.Count == 0)
        {
            System.Console.WriteLine(Strings.UI_NoJobsExecuted);
            return;
        }

        System.Console.WriteLine();
        System.Console.WriteLine(Strings.UI_BatchSummary);
        var successCount = results.Count(r => r.Success);
        System.Console.WriteLine(string.Format(Strings.UI_BatchSuccess, successCount, results.Count));
    }
}
