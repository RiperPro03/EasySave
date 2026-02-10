using EasySave.App.Console.Views;
using EasySave.Core.Interfaces;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

/// <summary>
/// Handles job management actions in the console UI.
/// </summary>
public sealed class JobController
{
    private readonly IJobService _jobService;
    private readonly JobView _jobView;
    private readonly ConsoleView _consoleView;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobController"/> class.
    /// </summary>
    /// <param name="jobService">Service used to manage jobs.</param>
    /// <param name="jobView">View used to display job UI.</param>
    /// <param name="consoleView">View used for global console output.</param>
    public JobController(IJobService jobService, JobView jobView, ConsoleView consoleView)
    {
        _jobService = jobService;
        _jobView = jobView;
        _consoleView = consoleView;
    }

    /// <summary>
    /// Shows the job menu and handles user choices.
    /// </summary>
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
                    // Liste tous les jobs.
                    ListJobs();
                    break;
                case 2:
                    // Creation d'un nouveau job.
                    CreateJob();
                    break;
                case 3:
                    // Mise a jour d'un job existant.
                    UpdateJob();
                    break;
                case 4:
                    // Suppression d'un job.
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

    /// <summary>
    /// Lists all jobs in the console.
    /// </summary>
    public void ListJobs()
    {
        var jobs = _jobService.GetAll();
        _jobView.ShowJobs(jobs);
        _consoleView.WaitForKey();
    }

    /// <summary>
    /// Creates a new job from user input.
    /// </summary>
    public void CreateJob()
    {
        try
        {
            var id = _jobView.AskJobId();
            var name = _jobView.AskJobName();
            var sourcePath = _jobView.AskSourcePath();
            var targetPath = _jobView.AskTargetPath();
            var type = _jobView.AskBackupType();
            
            // Envoi des donnees au Service pour créer le job
            _jobService.Create(id, name, sourcePath, targetPath, type);
            _consoleView.ShowSuccess("Job created.");
        }
        catch (Exception ex)
        {
            // Capture toute erreur metier pour l'afficher proprement
            _consoleView.ShowError(ex.Message);
        }
        _consoleView.WaitForKey();
    }

    /// <summary>
    /// Updates an existing job based on user choices.
    /// </summary>
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
            
            // Permet de ne modifier qu'une seule propriete au lieu de tout ressaisir
            var fieldChoice = _jobView.AskJobFieldToEdit();
            if (fieldChoice == 0)
            {
                // L'utilisateur annule la mise a jour.
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
                    // Nom
                    name = _jobView.AskJobName();
                    break;
                case 2:
                    // Chemin source
                    sourcePath = _jobView.AskSourcePath();
                    break;
                case 3:
                    // Chemin cible
                    targetPath = _jobView.AskTargetPath();
                    break;
                case 4:
                    // Type de sauvegarde
                    type = _jobView.AskBackupType();
                    break;
                case 5:
                    // Etat actif/inactif
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

    /// <summary>
    /// Deletes a job selected by the user.
    /// </summary>
    public void DeleteJob()
    {
        try
        {
            // Affichage des jobs deja crees.
            var jobs = _jobService.GetAll();
            _consoleView.ShowInfo("Existing jobs:");
            _jobView.ShowJobs(jobs);


            // Choix du job a supprimee
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