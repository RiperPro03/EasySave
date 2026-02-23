using System;
using System.Collections.Generic;
using System.Text;
namespace EasySave.Core.Models;

public class PriorityMonitor
{
    // On utilise un objet simple pour le verrouillage (compatible toutes versions .NET)
    private readonly object _lockObj = new();
    private int _priorityFilesInProgress = 0;

    // Propriété pour vérifier si un job est en train de traiter un fichier prioritaire
    public bool IsPriorityWorkActive
    {
        get
        {
            lock (_lockObj)
            {
                return _priorityFilesInProgress > 0;
            }
        }
    }

    // Appelé quand on commence un fichier prioritaire
    public void EnterPriorityZone()
    {
        lock (_lockObj)
        {
            _priorityFilesInProgress++;
        }
    }

    // Appelé quand on finit un fichier prioritaire
    public void ExitPriorityZone()
    {
        lock (_lockObj)
        {
            _priorityFilesInProgress--;
            // Sécurité pour ne jamais descendre sous 0
            if (_priorityFilesInProgress < 0) _priorityFilesInProgress = 0;
        }
    }
}