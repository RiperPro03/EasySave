using System;
using EasySave.Core.DTO;

namespace EasySave.Core.Events;

public sealed class JobStateChangedEventArgs : EventArgs
{
    public JobStateDto State { get; }

    public JobStateChangedEventArgs(JobStateDto state)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
    }
}
