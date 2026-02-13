using System.Collections.ObjectModel;
using EasySave.App.Gui.Services;

namespace EasySave.App.Gui.ViewModels
{
    public class LogsViewModel : ViewModelBase
    {
        private readonly LogReaderService _logReader;
        public ObservableCollection<LogEntry> Logs { get; } = new ObservableCollection<LogEntry>();

        public LogsViewModel()
        {
            _logReader = new LogReaderService();
            LoadLogs();
        }

        private void LoadLogs()
        {
            Logs.Clear();
            foreach (var logEntry in _logReader.GetAllLogs())
            {
                Logs.Add(logEntry);
            }
        }
    }
}
