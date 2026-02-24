using System;
using System.Collections.Generic;
using System.IO;
using EasySave.Core.DTO;
using EasySave.EasyLog.Factories;
using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Options;

namespace EasySave.App.Services;

/// <summary>
/// Reads application logs through EasyLog.
/// </summary>
public sealed class LogReaderService
{
    private const int LocalAndServerConnectTimeoutMs = 150;
    private const int LocalAndServerReceiveTimeoutMs = 250;
    private readonly string _resolvedLogDirectory;
    private readonly Func<LogStorageMode> _logStorageModeProvider;
    private readonly Func<string> _logServerHostProvider;
    private readonly Func<int> _logServerPortProvider;
    private readonly object _syncRoot = new();
    private ILogReader<LogEntryDto>? _logReader;
    private LogStorageMode _currentStorageMode;
    private string _currentServerHost = string.Empty;
    private int _currentServerPort;

    /// <summary>
    /// Initializes a new instance of the service.
    /// Uses the default EasySave AppData log folder when no path is provided.
    /// </summary>
    /// <param name="logDirectory">Optional path to the logs folder.</param>
    public LogReaderService(string? logDirectory = null)
        : this(
            logDirectory,
            () => LogStorageMode.LocalOnly,
            () => "localhost",
            () => 9696)
    {
    }

    /// <summary>
    /// Initializes a new instance of the service with dynamic storage settings.
    /// </summary>
    /// <param name="logDirectory">Optional path to the local logs folder.</param>
    /// <param name="logStorageModeProvider">Provides the current storage mode.</param>
    /// <param name="logServerHostProvider">Provides the centralized log server host.</param>
    /// <param name="logServerPortProvider">Provides the centralized log server port.</param>
    public LogReaderService(
        string? logDirectory,
        Func<LogStorageMode> logStorageModeProvider,
        Func<string> logServerHostProvider,
        Func<int> logServerPortProvider)
    {
        _resolvedLogDirectory = string.IsNullOrWhiteSpace(logDirectory)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ProSoft",
                "EasySave",
                "Logs")
            : logDirectory;
        _logStorageModeProvider = logStorageModeProvider ?? throw new ArgumentNullException(nameof(logStorageModeProvider));
        _logServerHostProvider = logServerHostProvider ?? throw new ArgumentNullException(nameof(logServerHostProvider));
        _logServerPortProvider = logServerPortProvider ?? throw new ArgumentNullException(nameof(logServerPortProvider));
        _currentStorageMode = _logStorageModeProvider();
    }

    /// <summary>
    /// Reads log entries from a limited number of files in the log directory.
    /// </summary>
    public IReadOnlyList<LogEntryDto> ReadEntries(int maxFiles = 7)
    {
        EnsureReader();
        return _logReader!.ReadEntries(maxFiles);
    }

    /// <summary>
    /// Reads all parseable log entries from every supported file in the log directory.
    /// </summary>
    public IReadOnlyList<LogEntryDto> ReadAllEntries()
    {
        EnsureReader();
        return _logReader!.ReadAllEntries();
    }

    private void EnsureReader()
    {
        var desiredStorageMode = _logStorageModeProvider();
        var desiredServerHost = _logServerHostProvider();
        var desiredServerPort = _logServerPortProvider();

        lock (_syncRoot)
        {
            if (_logReader != null
                && desiredStorageMode == _currentStorageMode
                && string.Equals(desiredServerHost, _currentServerHost, StringComparison.Ordinal)
                && desiredServerPort == _currentServerPort)
            {
                return;
            }

            _currentStorageMode = desiredStorageMode;
            _currentServerHost = desiredServerHost;
            _currentServerPort = desiredServerPort;

            // En mode mixte, la lecture distante est best-effort pour ne pas bloquer l'UI.
            // On applique des timeouts courts; les logs locaux restent prioritaires.
            bool isMixedMode = desiredStorageMode == LogStorageMode.LocalAndServer;

            // Le reader EasyLog est reconstruit seulement si la strategie ou la cible serveur change.
            _logReader = LogReaderFactory.Create<LogEntryDto>(new LogOptions
            {
                LogDirectory = _resolvedLogDirectory,
                StorageMode = desiredStorageMode,
                Server = new LogServerOptions
                {
                    Host = desiredServerHost,
                    Port = desiredServerPort,
                    ConnectTimeoutMs = isMixedMode ? LocalAndServerConnectTimeoutMs : 5000,
                    ReceiveTimeoutMs = isMixedMode ? LocalAndServerReceiveTimeoutMs : 10000
                }
            });
        }
    }
}
