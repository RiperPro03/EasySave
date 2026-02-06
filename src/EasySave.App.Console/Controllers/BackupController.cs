using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.App.Services;
using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

public sealed class BackupController
{
    private readonly IBackupService _backupService;
    private readonly IJobService _jobService;
    private readonly BackupView _backupView;
    private readonly ConsoleView _consoleView;
    private readonly ArgsParser _argsParser;

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

    public void RunAll()
    {
        var jobs = _jobService.GetAll();
        if (jobs.Count == 0)
        {
            _consoleView.ShowInfo(Strings.UI_NoJobsConfigured);
            _consoleView.WaitForKey();
            return;
        }

        var results = new List<BackupResultDto>();
        foreach (var job in jobs)
        {
            _backupView.ShowRunStart(job);
            var result = _backupService.Run(job);
            _backupView.ShowRunEnd(result);
            results.Add(result);
        }

        _backupView.ShowBatchResult(results);
        _consoleView.WaitForKey();
    }

    private void RunOneInteractive()
    {
        var jobs = _jobService.GetAll();
        _backupView.ShowJobs(jobs);

        if (jobs.Count == 0)
        {
            _consoleView.WaitForKey();
            return;
        }

        var id = _backupView.AskJobId();
        RunJobById(id, waitForKey: false);
        _consoleView.WaitForKey();
    }
}
