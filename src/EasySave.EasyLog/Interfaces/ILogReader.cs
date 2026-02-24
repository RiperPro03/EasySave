namespace EasySave.EasyLog.Interfaces
{
    /// <summary>
    /// Reads log entries from a configured source.
    /// </summary>
    /// <typeparam name="T">The log entry type.</typeparam>
    public interface ILogReader<T>
    {
        /// <summary>
        /// Gets the configured local log directory.
        /// </summary>
        string LogDirectory { get; }

        /// <summary>
        /// Lists supported local log files sorted by descending file name.
        /// </summary>
        /// <param name="maxFiles">Optional file-count limit.</param>
        /// <returns>A read-only list of log file paths.</returns>
        IReadOnlyList<string> GetLogFiles(int? maxFiles = null);

        /// <summary>
        /// Reads entries from a limited number of files.
        /// </summary>
        /// <param name="maxFiles">Maximum number of files to parse.</param>
        /// <returns>A read-only list of parsed entries.</returns>
        IReadOnlyList<T> ReadEntries(int maxFiles = 7);

        /// <summary>
        /// Reads entries from all supported files.
        /// </summary>
        /// <returns>A read-only list of parsed entries.</returns>
        IReadOnlyList<T> ReadAllEntries();

        /// <summary>
        /// Reads entries from a specific file.
        /// </summary>
        /// <param name="filePath">Path to a supported log file.</param>
        /// <returns>A read-only list of parsed entries.</returns>
        IReadOnlyList<T> ReadEntriesFromFile(string filePath);
    }
}
