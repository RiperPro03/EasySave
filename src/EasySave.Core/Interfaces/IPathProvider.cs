using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Provides application file system paths.
/// </summary>
public interface IPathProvider
{
    /// <summary>
    /// Gets the path to the logs directory.
    /// </summary>
    string LogsPath { get; }

    /// <summary>
    /// Gets the path to the state file.
    /// </summary>
    string StatePath { get; }

    /// <summary>
    /// Gets the path to the configuration file.
    /// </summary>
    string ConfigPath { get; }

    /// <summary>
    /// Ensures required directories exist.
    /// </summary>
    void EnsureDirectoriesCreated();
}