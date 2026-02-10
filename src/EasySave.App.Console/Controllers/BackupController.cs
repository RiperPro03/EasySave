using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.App.Services;
using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

/// <summary>
/// 'sealed' : Personne ne peut hériter de cette classe.Cela permet de sécuriser la classe.
/// </summary>
public sealed class BackupController
{
    /// <summary>
    /// On prépare les outils (services et vues) dont on aura besoin plus tard.
    /// </summary>
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

    /// <summary>
    /// La boucle principale du menu sauvegarde.
    /// </summary>
    public void RunMenu()
    {
        var exit = false;
        while (!exit)
        {
            ///<summary>
            /// On nettoie l'écran et on affiche le menu via la "Vue".
            /// </summary> 
            _consoleView.Clear();
            _consoleView.ShowHeader();
            _backupView.ShowBackupMenu();

            var choice = _backupView.ReadMenuChoice();

            ///<summary>
            /// On gère les actions selon le chiffre tapé.
            /// </summary> 
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
                    ///<summary>
                    /// Si l'utilisateur tape n'importe quoi, on affiche une erreur.
                    /// </summary> 
                    _consoleView.ShowError(Strings.Error_InvalidChoice);
                    _consoleView.WaitForKey();
                    break;
            }
        }
    }

    /// <summary>
    /// Lancement de jobs.:
    /// </summary>
    public void RunFromArgs(string rawArgs)
    {
        IReadOnlyList<int> ids;
        try
        {
            ///<summary>
            /// On transforme le texte en une liste de nombres.
            /// </summary> 
            ids = _argsParser.Parse(rawArgs);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or ArgumentOutOfRangeException)
        {
            ///<summary>
            /// Si le texte est mal écrit, on affiche l'erreur.
            /// </summary>
            _consoleView.ShowError(ex.Message);
            return;
        }

        var results = new List<BackupResultDto>();
        foreach (var id in ids)
        {
            ///<summary>
            /// On lance chaque job trouvé un par un.
            /// </summary>
            var result = RunJobById(id, waitForKey: false);
            if (result is not null)
                results.Add(result);
        }

        ///<summary>
        /// On affiche le bilan final de tout ce qui a été fait.
        /// </summary>
        _backupView.ShowBatchResult(results);
    }

    ///<summary>
    /// La méthode qui fait le vrai boulot pour UN seul job.
    /// </summary> 
    public BackupResultDto? RunJobById(int id, bool waitForKey = true)
    {
        ///<summary>
        /// On demande au service de nous donner les infos du job via son ID.
        /// </summary> 
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
    /// Lancer tous les jobs enregistrés.
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

        var results = new List<BackupResultDto>();
        foreach (var job in jobs)
        {
            ///<summary>
            /// On réutilise la logique de sauvegarde pour chaque job.
            /// </summary>
            _backupView.ShowRunStart(job);
            var result = _backupService.Run(job);
            _backupView.ShowRunEnd(result);
            results.Add(result);
        }

        _backupView.ShowBatchResult(results);
        _consoleView.WaitForKey();
    }

    /// <summary>
    /// Mode où l'on choisit visuellement quel job lancer.
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

        ///<summary>
        /// On demande l'ID à l'utilisateur et on le lance.
        /// </summary> On demande l'ID à l'utilisateur et on le lance.
        var id = _backupView.AskJobId();
        RunJobById(id, waitForKey: false);
        _consoleView.WaitForKey();
    }
}