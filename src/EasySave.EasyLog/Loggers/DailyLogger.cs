using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Utils;

namespace EasySave.EasyLog.Loggers
{
    internal sealed class DailyLogger<T> : ILogger<T>
    {
        private readonly string _logDirectory;
        private readonly ILogSerializer _logSerializer;
        private readonly ILogWriter _logWriter;
        private readonly Func<DateTime> _dateTimeProvider;

        public DailyLogger(string logDirectory, ILogSerializer logSerializer, ILogWriter logWriter)
            : this(logDirectory, logSerializer, logWriter, () => DateTime.Now)
        {
        }

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
                    // Initialisation du fichier avec la balise racine
                    string initialContent = $"<logs>\n{serializedEntry}\n</logs>";
                    return _logWriter.Write(filePath, initialContent);
                }
                else
                {
                    // Le fichier existe : on insère l'entrée avant la balise </logs>
                    // On utilise un FileStream pour manipuler la fin du fichier sans tout charger en mémoire
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        // On se place juste avant </logs> (7 caractères : </logs>)
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
            }
            catch
            {
                return false;
            }
        }
    }
}
