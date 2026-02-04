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

        public DailyLogger(string logDirectory)
        {
            this.logDirectory = logDirectory;
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public bool Write(T entry)
        {
            try
            {

                return true; 
            }
            catch { return false; }
        }
    }
}
