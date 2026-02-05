using System;
using System.IO;
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

            this._logSerializer = logSerializer ?? throw new ArgumentNullException(nameof(logSerializer));
            this._logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
            this._dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            this._logDirectory = logDirectory;

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

            string text = _logSerializer.Serialize(entry!);

            return _logWriter.Write(filePath, text);
        }
    }
}
