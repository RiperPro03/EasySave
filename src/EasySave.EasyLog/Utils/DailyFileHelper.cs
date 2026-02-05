namespace EasySave.EasyLog.Utils
{
    internal static class DailyFileHelper
    {
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
