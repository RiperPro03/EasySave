using System;
using System.Collections.Generic;
using System.Globalization;
using EasySave.Core.DTO;

namespace EasySave.App.Gui.Models;

/// <summary>
/// Presentation wrapper that exposes the raw log DTO.
/// </summary>
public sealed class LogEntryItem
{
    public LogEntryItem(LogEntryDto entry)
    {
        Entry = entry ?? throw new ArgumentNullException(nameof(entry));
        Sections = BuildSections(entry);
    }

    public LogEntryDto Entry { get; }
    public IReadOnlyList<LogSection> Sections { get; }

    private static IReadOnlyList<LogSection> BuildSections(LogEntryDto entry)
    {
        var sections = new List<LogSection>();

        if (entry.Job is not null)
        {
            var fields = new List<LogField>();
            AddField(fields, "Id", entry.Job.Id);
            AddField(fields, "Name", entry.Job.Name);
            AddField(fields, "Type", entry.Job.Type);
            AddField(fields, "Status", entry.Job.Status);
            AddField(fields, "IsActive", entry.Job.IsActive);
            AddField(fields, "SourcePath", entry.Job.SourcePath);
            AddField(fields, "TargetPath", entry.Job.TargetPath);

            if (fields.Count > 0)
                sections.Add(new LogSection("Job", fields));
        }

        if (entry.File is not null)
        {
            var fields = new List<LogField>();
            AddField(fields, "SourcePath", entry.File.SourcePath);
            AddField(fields, "TargetPath", entry.File.TargetPath);
            AddField(fields, "SizeBytes", entry.File.SizeBytes);
            AddField(fields, "TransferTimeMs", entry.File.TransferTimeMs);
            AddField(fields, "IsDirectory", entry.File.IsDirectory);

            if (fields.Count > 0)
                sections.Add(new LogSection("File", fields));
        }

        if (entry.Crypto is not null)
        {
            var fields = new List<LogField>
            {
                new("Tool", entry.Crypto.Tool),
                new("ExtensionMatched", entry.Crypto.ExtensionMatched.ToString())
            };

            AddField(fields, "EncryptionTimeMs", (long?)entry.Crypto.EncryptionTimeMs);
            AddField(fields, "Extension", entry.Crypto.Extension);
            AddField(fields, "InstanceLock", entry.Crypto.InstanceLock);

            sections.Add(new LogSection("Crypto", fields));
        }

        if (entry.Settings is not null)
        {
            var fields = new List<LogField>();
            AddField(fields, "Language", entry.Settings.Language);
            AddField(fields, "LogFormat", entry.Settings.LogFormat);
            AddField(fields, "LogStorageMode", entry.Settings.LogStorageMode);
            AddField(fields, "LogDirectory", entry.Settings.LogDirectory);
            AddField(fields, "ConfigPath", entry.Settings.ConfigPath);
            AddField(fields, "LogServerHost", entry.Settings.LogServerHost);
            AddField(fields, "LogServerPort", entry.Settings.LogServerPort);
            AddField(fields, "EncryptionEnabled", entry.Settings.EncryptionEnabled);
            if (entry.Settings.ExtensionsToEncrypt is not null)
            {
                var extensions = string.Join(", ", entry.Settings.ExtensionsToEncrypt);
                AddField(fields, "ExtensionsToEncrypt", extensions);
            }
            AddField(fields, "BusinessSoftwareProcessName", entry.Settings.BusinessSoftwareProcessName);
            AddField(fields, "LargeFileThresholdKb", entry.Settings.LargeFileThresholdKb);

            if (fields.Count > 0)
                sections.Add(new LogSection("Settings", fields));
        }

        if (entry.Summary is not null)
        {
            var fields = new List<LogField>
            {
                new("CopiedCount", entry.Summary.CopiedCount.ToString(CultureInfo.InvariantCulture)),
                new("SkippedCount", entry.Summary.SkippedCount.ToString(CultureInfo.InvariantCulture)),
                new("ErrorCount", entry.Summary.ErrorCount.ToString(CultureInfo.InvariantCulture)),
                new("TotalBytes", entry.Summary.TotalBytes.ToString(CultureInfo.InvariantCulture)),
                new("DurationMs", entry.Summary.DurationMs.ToString(CultureInfo.InvariantCulture))
            };

            AddField(fields, "Details", entry.Summary.Details);
            sections.Add(new LogSection("Summary", fields));
        }

        if (entry.Error is not null)
        {
            var fields = new List<LogField>();
            AddField(fields, "Type", entry.Error.Type);
            AddField(fields, "Code", entry.Error.Code);
            AddField(fields, "Message", entry.Error.Message);
            AddField(fields, "Stack", entry.Error.Stack);

            if (fields.Count > 0)
                sections.Add(new LogSection("Error", fields));
        }

        if (entry.Trace is not null)
        {
            var fields = new List<LogField>();
            AddField(fields, "Id", entry.Trace.Id);

            if (fields.Count > 0)
                sections.Add(new LogSection("Trace", fields));
        }

        if (entry.App is not null)
        {
            var fields = new List<LogField>();
            AddField(fields, "Name", entry.App.Name);
            AddField(fields, "Version", entry.App.Version);

            if (fields.Count > 0)
                sections.Add(new LogSection("App", fields));
        }

        if (entry.Host is not null)
        {
            var fields = new List<LogField>();
            AddField(fields, "Name", entry.Host.Name);
            AddField(fields, "User", entry.Host.User);
            AddField(fields, "Pid", entry.Host.Pid);

            if (fields.Count > 0)
                sections.Add(new LogSection("Host", fields));
        }

        return sections;
    }

    private static void AddField(List<LogField> fields, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        fields.Add(new LogField(label, value));
    }

    private static void AddField<T>(List<LogField> fields, string label, T? value) where T : struct
    {
        if (!value.HasValue)
            return;

        fields.Add(new LogField(label, value.Value.ToString() ?? string.Empty));
    }
}

public sealed class LogSection
{
    public LogSection(string title, IReadOnlyList<LogField> fields)
    {
        Title = title;
        Fields = fields;
    }

    public string Title { get; }
    public IReadOnlyList<LogField> Fields { get; }
}

public sealed class LogField
{
    public LogField(string label, string value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; }
    public string Value { get; }
}
