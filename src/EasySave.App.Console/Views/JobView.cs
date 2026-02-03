using EasySave.App.Console.Input;
using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Views;

public sealed class JobView
{
    private readonly ConsoleInput _input;

    public JobView(ConsoleInput input)
    {
        _input = input;
    }

    public void ShowJobMenu()
    {
        System.Console.WriteLine();
        System.Console.WriteLine(Strings.UI_JobsTitle);
        System.Console.WriteLine($"1 - {Strings.UI_ListJobs}");
        System.Console.WriteLine($"2 - {Strings.UI_CreateJob}");
        System.Console.WriteLine($"3 - {Strings.UI_UpdateJob}");
        System.Console.WriteLine($"4 - {Strings.UI_DeleteJob}");
        System.Console.WriteLine($"0 - {Strings.UI_Back}");
    }

    public int ReadMenuChoice()
    {
        return _input.ReadInt("> ");
    }

    public string AskJobName()
    {
        return _input.ReadNonEmptyString(Strings.UI_JobNamePrompt);
    }

    public string AskSourcePath()
    {
        return _input.ReadNonEmptyString(Strings.UI_SourcePathPrompt);
    }

    public string AskTargetPath()
    {
        return _input.ReadNonEmptyString(Strings.UI_TargetPathPrompt);
    }

    public BackupType AskBackupType()
    {
        System.Console.WriteLine(Strings.UI_BackupTypePrompt);
        System.Console.WriteLine($"1 - {Strings.UI_BackupTypeFull}");
        System.Console.WriteLine($"2 - {Strings.UI_BackupTypeDifferential}");

        var choice = _input.ReadChoice("> ", new[] { 1, 2 });
        return choice == 1 ? BackupType.Full : BackupType.Differential;
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

    public string AskJobId()
    {
        var id = _input.ReadInt(Strings.UI_JobIdPrompt);
        return id.ToString();
    }
}
