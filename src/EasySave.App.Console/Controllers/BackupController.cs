using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

public sealed class BackupController
{
    private readonly IBackupEngine _backupEngine;
    private readonly BackupView _backupView;
    private readonly ConsoleView _consoleView;
    private readonly ArgsParser _argsParser;

    public BackupController(
        IBackupEngine backupEngine,
        BackupView backupView,
        ConsoleView consoleView,
        ArgsParser argsParser)
    {
        _backupEngine = backupEngine;
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
        _consoleView.ShowInfo("//TODO: run all jobs will be handled by EasySave.App.");
        _consoleView.WaitForKey();
    }

    private void RunOneInteractive()
    {
        _consoleView.ShowInfo("//TODO: run one job will be handled by EasySave.App.");
        _consoleView.WaitForKey();
    }
}
