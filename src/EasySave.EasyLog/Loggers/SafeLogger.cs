using EasySave.EasyLog.Interfaces;

namespace EasySave.EasyLog.Loggers
{
    /// <summary>
    /// Wraps a logger and swallows exceptions.
    /// </summary>
    /// <typeparam name="T">The log entry type.</typeparam>
    public sealed class SafeLogger<T> : ILogger<T>
    {
        private readonly ILogger<T> _innerLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeLogger{T}"/> class.
        /// </summary>
        /// <param name="innerLogger">The logger to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="innerLogger"/> is null.</exception>
        public SafeLogger(ILogger<T> innerLogger)
        {
            _innerLogger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
        }

        /// <summary>
        /// Writes a log entry and suppresses any exception.
        /// </summary>
        /// <param name="entry">The entry to write.</param>
        /// <returns><c>true</c> when the write succeeds; otherwise <c>false</c>.</returns>
        public bool Write(T entry)
        {
            try
            {
                return _innerLogger.Write(entry);
            }
            catch
            {
                return false;
            }
        }
    }
}
