using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Utils;

namespace EasySave.EasyLog.Loggers
{
    // Cette classe gère la création de logs quotidiens de manière organisée
    internal sealed class DailyLogger<T> : ILogger<T>
    {
        private readonly string _logDirectory;
        private readonly ILogSerializer _logSerializer;
        private readonly ILogWriter _logWriter;
        private readonly Func<DateTime> _dateTimeProvider;

        // Constructeur standard qui utilise l'heure actuelle du système par défaut
        public DailyLogger(string logDirectory, ILogSerializer logSerializer, ILogWriter logWriter)
            : this(logDirectory, logSerializer, logWriter, () => DateTime.Now)
        {
        }

        // Constructeur complet permettant d'injecter toutes les dépendances, utile pour les tests
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

        // Méthode qui exécute l'écriture d'une entrée de log
        public bool Write(T entry)
        {
            string filePath = DailyFileHelper.GetLogFilePath(
                _logDirectory,
                _logSerializer.FileExtension,
                _dateTimeProvider());

            string text = _logSerializer.Serialize(entry!);

            return _logWriter.Write(filePath, text);
        }
    }
}
