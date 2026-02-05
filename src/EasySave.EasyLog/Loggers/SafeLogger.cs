using EasySave.EasyLog.Interfaces;

namespace EasySave.EasyLog.Loggers
{
    public sealed class SafeLogger<T> : ILogger<T>
    {
        private readonly ILogger<T> _innerLogger;

        public SafeLogger(ILogger<T> innerLogger)
        {
            _innerLogger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
        }

        public bool Write(T entry)
        {
            try
            {
                return _innerLogger.Write(entry);
            }
            catch
            {
                return false;
            }
        }
    }
}
