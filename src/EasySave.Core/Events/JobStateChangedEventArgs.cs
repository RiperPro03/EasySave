using System;
using EasySave.Core.DTO;

namespace EasySave.Core.Events;

/// <summary>
/// Provides data for a job state change event.
/// </summary>
public sealed class JobStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the updated job state.
    /// </summary>
    public JobStateDto State { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JobStateChangedEventArgs"/> class.
    /// </summary>
    /// <param name="state">The updated job state.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    public JobStateChangedEventArgs(JobStateDto state)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
    }
}
