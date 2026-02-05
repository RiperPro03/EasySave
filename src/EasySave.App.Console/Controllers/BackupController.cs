using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

public sealed class BackupController
{
    private readonly IBackupEngine _backupEngine;
    private readonly IJobRepository _jobRepository;
    private readonly BackupView _backupView;
    private readonly ConsoleView _consoleView;
    private readonly ArgsParser _argsParser;

    public BackupController(
        IBackupEngine backupEngine,
        IJobRepository jobRepository,
        BackupView backupView,
        ConsoleView consoleView,
        ArgsParser argsParser)
    {
        _backupEngine = backupEngine;
        _jobRepository = jobRepository;
        _backupView = backupView;
        _consoleView = consoleView;
        _argsParser = argsParser;
    }

    public void RunMenu()
    {
        var exit = false;
        while (!exit)
        {
            _consoleView.Clear();
            _consoleView.ShowHeader();
            _backupView.ShowBackupMenu();

            var choice = _backupView.ReadMenuChoice();

            switch (choice)
            {
                case 1:
                    RunOneInteractive();
                    break;
                case 2:
                    RunAll();
                    break;
                case 0:
                    exit = true;
                    break;
                default:
                    _consoleView.ShowError(Strings.Error_InvalidChoice);
                    _consoleView.WaitForKey();
                    break;
            }
        }
    }

    public void RunFromArgs(string rawArgs)
    {
        _consoleView.ShowInfo("//TODO: batch execution will be handled by EasySave.App.");
        _consoleView.WaitForKey();
    }

    public BackupResultDto? RunJobById(int id, bool waitForKey = true)
    {
        _consoleView.ShowInfo("//TODO: backup execution will be handled by EasySave.App.");
        if (waitForKey)
            _consoleView.WaitForKey();

        return null;
    }

    public void RunAll()
    {
        var jobs = _jobRepository.GetAll();

        if (jobs.Count == 0)
        {
            _consoleView.ShowError(Strings.UI_NoJobsConfigured);
            _consoleView.WaitForKey();
            return;
        }

        var results = new List<BackupResultDto>();

        foreach (var job in jobs)
        {
            // Afficher début
            _backupView.ShowRunStart(job);

            // Exécuter
            var result = _backupEngine.Run(job);
            results.Add(result);

            // Afficher fin
            _backupView.ShowRunEnd(result);
        }

        // Résumé global
        _backupView.ShowBatchResult(results);

        _consoleView.WaitForKey();
    }


    
    private void RunOneInteractive()
    {
        var jobs = _jobRepository.GetAll();

        if (jobs.Count == 0)
        {
            _consoleView.ShowError(Strings.UI_NoJobsConfigured);
            _consoleView.WaitForKey();
            return;
        }

        // 1. Afficher les jobs
        _backupView.ShowJobs(jobs);

        // 2. Demander l'ID
        var jobId = _backupView.AskJobId();

        var job = _jobRepository.GetById(jobId.ToString());
        if (job is null)
        {
            //_consoleView.ShowError(Strings.Error_JobNotFound);
            _consoleView.ShowError("Job introuvable."); // TODO : Mettre les langues dessus
            _consoleView.WaitForKey();
            return;
        }

        // 3. Afficher le début
        _backupView.ShowRunStart(job);

        // 4. Exécuter VIA BackupEngine
        var result = _backupEngine.Run(job);

        // 5. Afficher la fin
        _backupView.ShowRunEnd(result);

        _consoleView.WaitForKey();
    }


}
