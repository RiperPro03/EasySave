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
    private readonly LogContext? _context;
    private readonly object _logWriteLock = new();
    
    public event EventHandler? LogWritten;
    
    private LogFormat _currentFormat;
    private ILogger<LogEntryDto>? _logger;

    public AppLogService(
        string? logDirectory,
        Func<LogFormat> logFormatProvider,
        LogContext? context = null)
    {
        _logDirectory = logDirectory;
        _logFormatProvider = logFormatProvider ?? throw new ArgumentNullException(nameof(logFormatProvider));
        _context = context;
        _currentFormat = _logFormatProvider();
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
        if (desiredFormat == _currentFormat
            && _logger != null)
            return;

        _currentFormat = desiredFormat;
        if (string.IsNullOrWhiteSpace(_logDirectory))
        {
            _logger = null;
            return;
        }

        _logger = CreateLogger<LogEntryDto>(_logDirectory, desiredFormat);
    }

    private static ILogger<T> CreateLogger<T>(string logDirectory, LogFormat format)
    {
        var options = new LogOptions
        {
            LogDirectory = logDirectory,
            Format = format
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
