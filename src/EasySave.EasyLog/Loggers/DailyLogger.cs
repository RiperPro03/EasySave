using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Utils;

namespace EasySave.EasyLog.Loggers
{
    /// <summary>
    /// Writes daily log files using a serializer and writer.
    /// </summary>
    /// <typeparam name="T">The log entry type.</typeparam>
    internal sealed class DailyLogger<T> : ILogger<T>
    {
        private readonly string _logDirectory;
        private readonly ILogSerializer _logSerializer;
        private readonly ILogWriter _logWriter;
        private readonly Func<DateTime> _dateTimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyLogger{T}"/> class.
        /// </summary>
        /// <param name="logDirectory">The directory where logs are stored.</param>
        /// <param name="logSerializer">The serializer to use.</param>
        /// <param name="logWriter">The writer to use.</param>
        public DailyLogger(string logDirectory, ILogSerializer logSerializer, ILogWriter logWriter)
            : this(logDirectory, logSerializer, logWriter, () => DateTime.Now)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyLogger{T}"/> class.
        /// </summary>
        /// <param name="logDirectory">The directory where logs are stored.</param>
        /// <param name="logSerializer">The serializer to use.</param>
        /// <param name="logWriter">The writer to use.</param>
        /// <param name="dateTimeProvider">Provides the current time for file naming.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="logDirectory"/> is null or whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any dependency is null.
        /// </exception>
        public DailyLogger(
            string logDirectory,
            ILogSerializer logSerializer,
            ILogWriter logWriter,
            Func<DateTime> dateTimeProvider)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                throw new ArgumentException("Log directory is required.", nameof(logDirectory));
            }

            _logSerializer = logSerializer ?? throw new ArgumentNullException(nameof(logSerializer));
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _logDirectory = logDirectory;

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        /// <summary>
        /// Writes a log entry to the daily file.
        /// </summary>
        /// <param name="entry">The entry to write.</param>
        /// <returns><c>true</c> when the write succeeds; otherwise <c>false</c>.</returns>
        public bool Write(T entry)
        {
            string filePath = DailyFileHelper.GetLogFilePath(
                _logDirectory,
                _logSerializer.FileExtension,
                _dateTimeProvider());

            string serializedEntry = _logSerializer.Serialize(entry!);
    
            try
            {
                if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
                {
                    // Initialisation du fichier avec la balise racine.
                    string initialContent = $"<logs>\n{serializedEntry}\n</logs>";
                    return _logWriter.Write(filePath, initialContent);
                }
                
                // Le fichier existe : on insere l'entree avant la balise </logs>.
                // On utilise un FileStream pour manipuler la fin du fichier sans tout charger en memoire.
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    // On se place juste avant </logs> (7 caracteres : </logs>).
                    if (fs.Length > 7)
                    {
                        fs.SetLength(fs.Length - 7);
                        fs.Position = fs.Length;
                    }

                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(serializedEntry);
                        sw.Write("</logs>");
                    }
                }

                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}

