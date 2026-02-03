using EasySave.App.Console.Views;
using EasySave.Core.Interfaces;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

public sealed class JobController
{
    private readonly IJobRepository _jobRepository;
    private readonly JobView _jobView;
    private readonly ConsoleView _consoleView;

    public JobController(IJobRepository jobRepository, JobView jobView, ConsoleView consoleView)
    {
        _jobRepository = jobRepository;
        _jobView = jobView;
        _consoleView = consoleView;
    }

    public void RunMenu()
    {
        var exit = false;
        while (!exit)
        {
            _consoleView.Clear();
            _consoleView.ShowHeader();
            _jobView.ShowJobMenu();

            var choice = _jobView.ReadMenuChoice();

            switch (choice)
            {
                case 1:
                    ListJobs();
                    break;
                case 2:
                    CreateJob();
                    break;
                case 3:
                    UpdateJob();
                    break;
                case 4:
                    DeleteJob();
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

    public void ListJobs()
    {
        _consoleView.ShowInfo("//TODO: list jobs will be handled by EasySave.App.");
        _consoleView.WaitForKey();
    }

    public void CreateJob()
    {
        _consoleView.ShowInfo("//TODO: create job will be handled by EasySave.App.");
        _consoleView.WaitForKey();
    }

    public void UpdateJob()
    {
        _consoleView.ShowInfo("//TODO: update job will be handled by EasySave.App.");
        _consoleView.WaitForKey();
    }

    public void DeleteJob()
    {
        _consoleView.ShowInfo("//TODO: delete job will be handled by EasySave.App.");
        _consoleView.WaitForKey();
    }
}
