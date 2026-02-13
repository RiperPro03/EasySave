using System;
using EasySave.Core.DTO;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Application logging facade that can publish log events.
/// </summary>
public interface IAppLogService
{
    event EventHandler? LogWritten;

    void Write(LogEntryDto entry);
}
