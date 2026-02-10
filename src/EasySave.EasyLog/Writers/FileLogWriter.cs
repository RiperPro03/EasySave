using System.Text;
using EasySave.EasyLog.Interfaces;

namespace EasySave.EasyLog.Writers
{
    /// <summary>
    /// Writes log messages to files on disk.
    /// </summary>
    internal sealed class FileLogWriter : ILogWriter
    {
        /// <summary>
        /// Appends a message to a file path.
        /// </summary>
        /// <param name="filepath">The destination file path.</param>
        /// <param name="message">The message to write.</param>
        /// <returns><c>true</c> when the write succeeds; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filepath"/> is empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
        public bool Write(string filepath, string message)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                throw new ArgumentException("File path is required.", nameof(filepath));
            }

            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string? directory = Path.GetDirectoryName(filepath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                // Cree le dossier si besoin avant l'ecriture.
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(filepath, message, Encoding.UTF8);
            return true;
        }
    }
}
