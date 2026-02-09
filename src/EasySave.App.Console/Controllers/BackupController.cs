using EasySave.App.Console.Input;
using EasySave.App.Console.Views;
using EasySave.App.Services;
using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Resources;

namespace EasySave.App.Console.Controllers;

// 'sealed' : Personne ne peut hériter de cette classe.Cela permet de sécuriser la classe.
public sealed class BackupController
{
    // On prépare les outils (services et vues) dont on aura besoin plus tard.
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

    // La boucle principale du menu sauvegarde.
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

            // On gère les actions selon le chiffre tapé.
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
                    // Si l'utilisateur tape n'importe quoi, on affiche une erreur.
                    _consoleView.ShowError(Strings.Error_InvalidChoice);
                    _consoleView.WaitForKey();
                    break;
            }
        }
    }

    // Lancement de jobs.:
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

        var results = new List<BackupResultDto>();
        foreach (var id in ids)
        {
            // On lance chaque job trouvé un par un.
            var result = RunJobById(id, waitForKey: false);
            if (result is not null)
                results.Add(result);
        }

        // On affiche le bilan final de tout ce qui a été fait.
        _backupView.ShowBatchResult(results);
    }

    // La méthode qui fait le vrai boulot pour UN seul job.
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

    // Lancer tous les jobs enregistrés.
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
            // On réutilise la logique de sauvegarde pour chaque job.
            _backupView.ShowRunStart(job);
            var result = _backupService.Run(job);
            _backupView.ShowRunEnd(result);
            results.Add(result);
        }

        _backupView.ShowBatchResult(results);
        _consoleView.WaitForKey();
    }

    // Mode où l'on choisit visuellement quel job lancer.
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