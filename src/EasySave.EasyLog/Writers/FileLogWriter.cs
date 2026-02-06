using System.Text;
using EasySave.EasyLog.Interfaces;

namespace EasySave.EasyLog.Writers
{
    internal sealed class FileLogWriter : ILogWriter
    {
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
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(filepath, message, Encoding.UTF8);
            return true;
        }
    }
}
