namespace EasySave.EasyLog.Interfaces
{
    /// <summary>
    /// Defines a logger for a specific entry type.
    /// </summary>
    /// <typeparam name="T">The log entry type.</typeparam>
    public interface ILogger<T>
    {
        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <param name="entry">The entry to write.</param>
        /// <returns><c>true</c> when the entry was written; otherwise <c>false</c>.</returns>
        bool Write(T entry);
    }
}
