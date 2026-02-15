using System.Diagnostics;
namespace EasySave.App.Utils
{
    /// <summary>
    /// Utility to detect if a business software process is currently running.
    /// </summary>

    public static class BusinessSoftwareDetector
    {
        /// <summary>
        /// Checks if the specified process is running.
        /// </summary>
        /// <param name="processName">Process name to detect (from config)</param>
        /// <returns>True if process is active, false otherwise.</returns>
        public static bool IsRunning(string? processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return false;

            var processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        /// <summary>
        /// Validates that the specified process is not running, throwing an exception if it is.
        /// </summary>
        /// <param name="processName">Name of the business software process to check.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the specified process is running, to prevent launching the backup job.
        /// </exception>
        public static void ValidateNotRunning(string? processName)
        {
            if (IsRunning(processName))
                throw new InvalidOperationException(
                    $"Business software '{processName}' is currently running. Cannot start work.");
        }
    }
}
