using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

/// <summary>
/// Handles backup-related console actions.
/// </summary>
public sealed class BackupController
{
    // On prépare les outils (services et vues) dont on aura besoin
    private readonly IBackupService _backupService;
    private readonly IJobService _jobService;
    private readonly BackupView _backupView;
    private readonly ConsoleView _consoleView;
    private readonly ArgsParser _argsParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupController"/> class.
    /// </summary>
    /// <param name="backupService">Service used to run backups.</param>
    /// <param name="jobService">Service used to query jobs.</param>
    /// <param name="backupView">View used to display backup UI.</param>
    /// <param name="consoleView">View used for global console output.</param>
    /// <param name="argsParser">Parser for CLI arguments.</param>
    public BackupController(
        IBackupService backupService,
        IJobService jobService,
        BackupView backupView,
        ConsoleView consoleView,
        ArgsParser argsParser)
    {
        _backupService = backupService;
        _jobService = jobService;
        _backupView = backupView;
        _consoleView = consoleView;
        _argsParser = argsParser;
    }

    /// <summary>
    /// Shows the backup menu and handles user choices.
    /// </summary>
    public void RunMenu()
    {
        var exit = false;
        while (!exit)
        {
            // On nettoie l'écran et on affiche le menu via la "Vue".
            _consoleView.Clear();
            _consoleView.ShowHeader();
            _backupView.ShowBackupMenu();

            var choice = _backupView.ReadMenuChoice();
            
            // On gere les actions selon le chiffre tape.
            switch (choice)
            {
                case 1:
                    // Execution d'un job specifique.
                    RunOneInteractive();
                    break;
                case 2:
                    // Execution de tous les jobs.
                    RunAll();
                    break;
                case 0:
                    exit = true;
                    break;
                default:
                    // Si l'utilisateur tape une entre inatendu, on affiche une erreur.
                    _consoleView.ShowError(Strings.Error_InvalidChoice);
                    _consoleView.WaitForKey();
                    break;
            }
        }
    }

    /// <summary>
    /// Executes jobs from a raw argument string.
    /// </summary>
    /// <param name="rawArgs">Raw command-line arguments.</param>
    public void RunFromArgs(string rawArgs)
    {
        IReadOnlyList<int> ids;
        try
        {
            // On transforme le texte en une liste de nombres.
            ids = _argsParser.Parse(rawArgs);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or ArgumentOutOfRangeException)
        {
            // Si le texte est mal écrit, on affiche l'erreur.
            _consoleView.ShowError(ex.Message);
            return;
        }

        if (!_backupService.CanStartSequence(out var sequenceReason))
        {
            _consoleView.ShowError(sequenceReason ?? "Business software is running. Cannot start sequence.");
            return;
        }

        var results = new List<BackupResultDto>();
        foreach (var id in ids)
        {
            if (!_backupService.CanStartSequence(out var stepReason))
            {
                _consoleView.ShowError(stepReason ?? "Business software is running. Cannot start sequence.");
                break;
            }
            // Ne pas bloquer en mode batch.
            // On lance chaque job trouvé un par un.
            var result = RunJobById(id, waitForKey: false);
            if (result is not null)
                results.Add(result);
        }
        
        // On affiche le bilan final de tout ce qui a été fait.
        _backupView.ShowBatchResult(results);
    }

    /// <summary>
    /// Runs a single job by its numeric identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <param name="waitForKey">Whether to wait for a key press after execution.</param>
    /// <returns>The execution result, or <c>null</c> when the job does not exist.</returns>
    public BackupResultDto? RunJobById(int id, bool waitForKey = true)
    {
        // On demande au service de nous donner les infos du job via son ID.
        var job = _jobService.GetById(id.ToString());
        if (job is null)
        {
            _consoleView.ShowError($"Job with ID {id} not found.");
            if (waitForKey)
                _consoleView.WaitForKey();
            return null;
        }

        _backupView.ShowRunStart(job);
        var result = _backupService.Run(job);
        _backupView.ShowRunEnd(result);

        if (waitForKey)
            _consoleView.WaitForKey();

        return result;
    }

    /// <summary>
    /// Runs all configured jobs.
    /// </summary>
    public void RunAll()
    {
        var jobs = _jobService.GetAll();
        if (jobs.Count == 0)
        {
            _consoleView.ShowInfo(Strings.UI_NoJobsConfigured);
            _consoleView.WaitForKey();
            return;
        }

        if (!_backupService.CanStartSequence(out var sequenceReason))
        {
            _consoleView.ShowError(sequenceReason ?? "Business software is running. Cannot start sequence.");
            _consoleView.WaitForKey();
            return;
        }

        var results = new List<BackupResultDto>();
        foreach (var job in jobs)
        {
            if (!_backupService.CanStartSequence(out var stepReason))
            {
                _consoleView.ShowError(stepReason ?? "Business software is running. Cannot start sequence.");
                break;
            }
            // Executer en serie pour chaque job.
            _backupView.ShowRunStart(job);
            var result = _backupService.Run(job);
            _backupView.ShowRunEnd(result);
            results.Add(result);
        }

        _backupView.ShowBatchResult(results);
        _consoleView.WaitForKey();
    }

    /// <summary>
    /// Runs one job interactively by asking the user for the ID.
    /// </summary>
    private void RunOneInteractive()
    {
        var jobs = _jobService.GetAll();
        _backupView.ShowJobs(jobs);

        if (jobs.Count == 0)
        {
            _consoleView.WaitForKey();
            return;
        }
        
        // On demande l'ID à l'utilisateur et on le lance.
        var id = _backupView.AskJobId();
        RunJobById(id, waitForKey: false);
        _consoleView.WaitForKey();
    }
}
