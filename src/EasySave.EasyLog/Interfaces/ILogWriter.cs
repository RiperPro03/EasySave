namespace EasySave.EasyLog.Interfaces
{
    /// <summary>
    /// Writes log messages to a target destination.
    /// </summary>
    public interface ILogWriter
    {
        /// <summary>
        /// Writes a message to a file path.
        /// </summary>
        /// <param name="filepath">The destination file path.</param>
        /// <param name="message">The message to write.</param>
        /// <returns><c>true</c> when the write succeeds; otherwise <c>false</c>.</returns>
        bool Write(string filepath, string message);
    }
}
