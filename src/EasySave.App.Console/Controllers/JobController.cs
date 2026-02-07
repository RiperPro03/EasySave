using EasySave.App.Console.Views;
using EasySave.App.Services;
using EasySave.Core.Interfaces;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

public sealed class JobController
{
    private readonly IJobService _jobService;
    private readonly JobView _jobView;
    private readonly ConsoleView _consoleView;

    public JobController(IJobService jobService, JobView jobView, ConsoleView consoleView)
    {
        _jobService = jobService;
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
        var jobs = _jobService.GetAll();
        _jobView.ShowJobs(jobs);
        _consoleView.WaitForKey();
    }

    public void CreateJob()
    {
        try
        {
            var id = _jobView.AskJobId();
            var name = _jobView.AskJobName();
            var sourcePath = _jobView.AskSourcePath();
            var targetPath = _jobView.AskTargetPath();
            var type = _jobView.AskBackupType();

            _jobService.Create(id, name, sourcePath, targetPath, type);
            _consoleView.ShowSuccess("Job created.");
        }
        catch (Exception ex)
        {
            _consoleView.ShowError(ex.Message);
        }
        _consoleView.WaitForKey();
    }

    public void UpdateJob()
    {
        try
        {
            var jobs = _jobService.GetAll();
            _jobView.ShowJobs(jobs);
            var id = _jobView.AskJobId();
            var existing = _jobService.GetById(id);
            if (existing == null)
            {
                _consoleView.ShowError($"Job with ID {id} not found.");
                _consoleView.WaitForKey();
                return;
            }

            var fieldChoice = _jobView.AskJobFieldToEdit();
            if (fieldChoice == 0)
            {
                _consoleView.ShowInfo("Update cancelled.");
                _consoleView.WaitForKey();
                return;
            }

            var name = existing.Name;
            var sourcePath = existing.SourcePath;
            var targetPath = existing.TargetPath;
            var type = existing.Type;
            var isActive = existing.IsActive;

            switch (fieldChoice)
            {
                case 1:
                    name = _jobView.AskJobName();
                    break;
                case 2:
                    sourcePath = _jobView.AskSourcePath();
                    break;
                case 3:
                    targetPath = _jobView.AskTargetPath();
                    break;
                case 4:
                    type = _jobView.AskBackupType();
                    break;
                case 5:
                    isActive = _jobView.AskJobActiveState();
                    break;
            }

            _jobService.Update(id, name, sourcePath, targetPath, type, isActive);
            _consoleView.ShowSuccess("Job updated.");
        }
        catch (Exception ex)
        {
            _consoleView.ShowError(ex.Message);
        }
        _consoleView.WaitForKey();
    }

    public void DeleteJob()
    {
        try
        {
            //Affichage des jobs déja crée
            var jobs = _jobService.GetAll();
            _consoleView.ShowInfo("Existing jobs:");
            _jobView.ShowJobs(jobs);

            var id = _jobView.AskJobId();
            _jobService.Delete(id);
            _consoleView.ShowSuccess("Job deleted.");
        }
        catch (Exception ex)
        {
            _consoleView.ShowError(ex.Message);
        }
        _consoleView.WaitForKey();
    }
}