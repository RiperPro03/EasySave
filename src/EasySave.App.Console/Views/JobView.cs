using EasySave.App.Console.Input;
using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Views;

/// <summary>
/// Renders job-related console UI.
/// This class manages everything the user sees when creating or modifying a job.
/// </summary>
public sealed class JobView
{
    private readonly ConsoleInput _input;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobView"/> class.
    /// </summary>
    /// <param name="input">Console input helper.</param>
    public JobView(ConsoleInput input)
    {
        _input = input;
    }

    /// <summary>
    /// Displays the job menu.
    /// </summary>
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

    /// <summary>
    /// Asks the user for a job name.
    /// </summary>
    /// <returns>The job name.</returns>
    public string AskJobName()
    {
        return _input.ReadNonEmptyString(Strings.UI_JobNamePrompt);
    }

    /// <summary>
    /// Asks the user for a source path.
    /// </summary>
    /// <returns>The source path.</returns>
    public string AskSourcePath()
    {
        return _input.ReadNonEmptyString(Strings.UI_SourcePathPrompt);
    }

    /// <summary>
    /// Asks the user for a target path.
    /// </summary>
    /// <returns>The target path.</returns>
    public string AskTargetPath()
    {
        return _input.ReadNonEmptyString(Strings.UI_TargetPathPrompt);
    }

    /// <summary>
    /// Asks the user for a backup type.
    /// </summary>
    /// <returns>The selected backup type.</returns>
    public BackupType AskBackupType()
    {
        System.Console.WriteLine(Strings.UI_BackupTypePrompt);
        System.Console.WriteLine($"1 - {Strings.UI_BackupTypeFull}");
        System.Console.WriteLine($"2 - {Strings.UI_BackupTypeDifferential}");

        var choice = _input.ReadChoice("> ", new[] { 1, 2 });
        return choice == 1 ? BackupType.Full : BackupType.Differential;
    }

    /// <summary>
    /// Displays the list of jobs.
    /// </summary>
    /// <param name="jobs">The jobs to display.</param>
    public void ShowJobs(IReadOnlyList<BackupJob> jobs)
    {
        System.Console.WriteLine();
        if (jobs.Count == 0)
        {
            // Aucun job configure.
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

    /// <summary>
    /// Asks the user which field to edit.
    /// </summary>
    /// <returns>The field choice.</returns>
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

    /// <summary>
    /// Asks the user to set the active state.
    /// </summary>
    /// <returns><c>true</c> for active; <c>false</c> for inactive.</returns>
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
