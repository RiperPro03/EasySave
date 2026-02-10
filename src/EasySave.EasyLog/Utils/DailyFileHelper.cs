namespace EasySave.EasyLog.Utils
{
    /// <summary>
    /// Builds daily log file paths.
    /// </summary>
    internal static class DailyFileHelper
    {
        /// <summary>
        /// Returns the log file path for a given date.
        /// </summary>
        /// <param name="logDirectory">The base log directory.</param>
        /// <param name="fileExtension">The log file extension.</param>
        /// <param name="date">The date used for file naming.</param>
        /// <returns>The full log file path.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="logDirectory"/> or <paramref name="fileExtension"/> is missing.
        /// </exception>
        public static string GetLogFilePath(string logDirectory, string fileExtension, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                throw new ArgumentException("Log directory is required.", nameof(logDirectory));
            }

            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                throw new ArgumentException("File extension is required.", nameof(fileExtension));
            }

            string normalizedExtension = fileExtension.TrimStart('.');
            string fileName = $"{date:yyyy-MM-dd}.{normalizedExtension}";

            return Path.Combine(logDirectory, fileName);
        }
    }
}
