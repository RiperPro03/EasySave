﻿using System;
using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Logging;
using EasySave.EasyLog.Factories;
using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Options;

namespace EasySave.App.Services;

/// <summary>
/// Writes log entries and publishes notifications.
/// </summary>
public sealed class AppLogService : IAppLogService
{
    private readonly string? _logDirectory;
    private readonly Func<LogFormat> _logFormatProvider;
    private readonly Func<LogStorageMode> _logStorageModeProvider;
    private readonly Func<string> _logServerHostProvider;
    private readonly Func<int> _logServerPortProvider;
    private readonly LogContext? _context;
    private readonly object _logWriteLock = new();
    
    public event EventHandler? LogWritten;
    
    private LogFormat _currentFormat;
    private LogStorageMode _currentStorageMode;
    private string _currentServerHost = string.Empty;
    private int _currentServerPort;
    private ILogger<LogEntryDto>? _logger;

    public AppLogService(
        string? logDirectory,
        Func<LogFormat> logFormatProvider,
        LogContext? context = null)
        : this(
            logDirectory,
            logFormatProvider,
            () => LogStorageMode.LocalOnly,
            () => "localhost",
            () => 9696,
            context)
    {
    }

    public AppLogService(
        string? logDirectory,
        Func<LogFormat> logFormatProvider,
        Func<LogStorageMode> logStorageModeProvider,
        Func<string> logServerHostProvider,
        Func<int> logServerPortProvider,
        LogContext? context = null)
    {
        _logDirectory = logDirectory;
        _logFormatProvider = logFormatProvider ?? throw new ArgumentNullException(nameof(logFormatProvider));
        _logStorageModeProvider = logStorageModeProvider ?? throw new ArgumentNullException(nameof(logStorageModeProvider));
        _logServerHostProvider = logServerHostProvider ?? throw new ArgumentNullException(nameof(logServerHostProvider));
        _logServerPortProvider = logServerPortProvider ?? throw new ArgumentNullException(nameof(logServerPortProvider));
        _context = context;
        _currentFormat = _logFormatProvider();
        _currentStorageMode = _logStorageModeProvider();
        EnsureLoggers();
    }

    public void Write(LogEntryDto entry)
    {
        ApplyContext(entry);
        WriteInternal(entry, ref _logger);
    }

    private void WriteInternal<T>(T entry, ref ILogger<T>? logger)
    {
        if (entry is null)
            return;

        var notifyWritten = false;
        lock (_logWriteLock)
        {
            EnsureLoggers();

            if (logger is null)
            {
                notifyWritten = true;
            }
            else
            {
                notifyWritten = logger.Write(entry);
            }
        }

        if (notifyWritten)
            LogWritten?.Invoke(this, EventArgs.Empty);
    }

    private void EnsureLoggers()
    {
        var desiredFormat = _logFormatProvider();
        var desiredStorageMode = _logStorageModeProvider();
        var desiredServerHost = _logServerHostProvider();
        var desiredServerPort = _logServerPortProvider();

        if (desiredFormat == _currentFormat
            && desiredStorageMode == _currentStorageMode
            && string.Equals(desiredServerHost, _currentServerHost, StringComparison.Ordinal)
            && desiredServerPort == _currentServerPort
            && _logger != null)
            return;

        _currentFormat = desiredFormat;
        _currentStorageMode = desiredStorageMode;
        _currentServerHost = desiredServerHost;
        _currentServerPort = desiredServerPort;
        if (string.IsNullOrWhiteSpace(_logDirectory))
        {
            _logger = null;
            return;
        }

        _logger = CreateLogger<LogEntryDto>(
            _logDirectory,
            desiredFormat,
            desiredStorageMode,
            desiredServerHost,
            desiredServerPort);
    }

    private static ILogger<T> CreateLogger<T>(
        string logDirectory,
        LogFormat format,
        LogStorageMode storageMode,
        string serverHost,
        int serverPort)
    {
        var options = new LogOptions
        {
            LogDirectory = logDirectory,
            Format = format,
            StorageMode = storageMode,
            Server = new LogServerOptions
            {
                Host = serverHost,
                Port = serverPort
            }
        };

        return LoggerFactory.Create<T>(options);
    }

    /// <summary>
    /// Applies shared context fields to the entry when missing.
    /// </summary>
    /// <param name="entry">Entry to enrich.</param>
    private void ApplyContext(LogEntryDto entry)
    {
        if (_context is null)
            return;

        entry.App ??= new LogAppDto
        {
            Name = _context.AppName,
            Version = _context.AppVersion
        };

        entry.Host ??= new LogHostDto
        {
            Name = _context.HostName,
            User = _context.UserName,
            Pid = _context.ProcessId
        };

    }
}
