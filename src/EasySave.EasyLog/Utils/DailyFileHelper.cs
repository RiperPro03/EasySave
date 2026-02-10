namespace EasySave.EasyLog.Utils
{
    /// <summary>
    /// Classe utilitaire statique pour centraliser la gestion des noms de fichiers de logs journaliers
    /// </summary>
    internal static class DailyFileHelper
    {
        /// <summary>
        /// Génère le chemin complet vers le fichier de log
        /// </summary>
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
