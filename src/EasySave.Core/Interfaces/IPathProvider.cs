using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Core.Interfaces;

public interface IPathProvider
{
    string LogsPath { get; }
    string StatePath { get; }
    string ConfigPath { get; }
    void EnsureDirectoriesCreated(); // Crée les dossiers s'ils manquent
}