using EasySave.App.Console.Input;
using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Views;

/// <summary>
/// Cette classe gère tout ce que l'utilisateur voit quand il crée ou modifie un travail
/// </summary>
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

    public int AskJobFieldToEdit()
    {
        System.Console.WriteLine();
        System.Console.WriteLine("Select field to edit:");
        System.Console.WriteLine("1 - Name");
        System.Console.WriteLine("2 - Source path");
        System.Console.WriteLine("3 - Target path");
        System.Console.WriteLine("4 - Type");
        System.Console.WriteLine("5 - Active/Inactive");
        System.Console.WriteLine("0 - Back");

        return _input.ReadChoice("> ", new[] { 0, 1, 2, 3, 4, 5 });
    }

    public bool AskJobActiveState()
    {
        System.Console.WriteLine();
        System.Console.WriteLine("Set job status:");
        System.Console.WriteLine("1 - Active");
        System.Console.WriteLine("2 - Inactive");

        var choice = _input.ReadChoice("> ", new[] { 1, 2 });
        return choice == 1;
    }
}
