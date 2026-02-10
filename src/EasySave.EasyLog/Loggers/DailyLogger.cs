using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Utils;

namespace EasySave.EasyLog.Loggers
{
    /// <summary>
    /// Cette classe gère la création de logs quotidiens de manière organisée
    /// </summary>
    internal sealed class DailyLogger<T> : ILogger<T>
    {
        private readonly string _logDirectory;
        private readonly ILogSerializer _logSerializer;
        private readonly ILogWriter _logWriter;
        private readonly Func<DateTime> _dateTimeProvider;

        /// <summary>
        /// Constructeur standard qui utilise l'heure actuelle du système par défaut
        /// </summary>
        public DailyLogger(string logDirectory, ILogSerializer logSerializer, ILogWriter logWriter)
            : this(logDirectory, logSerializer, logWriter, () => DateTime.Now)
        {
        }

        /// <summary>
        /// Constructeur complet permettant d'injecter toutes les dépendances, utile pour les tests
        /// </summary>
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
        /// Méthode qui exécute l'écriture d'une entrée de log
        /// </summary>

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
                
                // Le fichier existe : on insere l'entree avant la balise </logs>
                // On utilise un FileStream pour manipuler la fin du fichier sans tout charger en memoire
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    // On se place juste avant </logs> (7 caracteres : </logs>)
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

