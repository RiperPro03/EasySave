using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave.EasyLog.Interfaces;

namespace EasySave.EasyLog.Loggers
{
    public class DailyLogger<T> : ILogger<T>
    {
        private readonly string logDirectory;
        private readonly ILogSerializer logSerializer;
        private readonly ILogWriter logWriter;

        public DailyLogger(string logDirectory, ILogSerializer logSerializer, ILogWriter logWriter)
        {
            this.logDirectory = logDirectory;
            this.logSerializer = logSerializer;
            this.logWriter = logWriter;

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public bool Write(T entry)
        {
            string filePath = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.{logSerializer.FileExtension}");

            string text = logSerializer.Serialize(entry);

            return logWriter.Write(filePath, text);
        }
    }
}
