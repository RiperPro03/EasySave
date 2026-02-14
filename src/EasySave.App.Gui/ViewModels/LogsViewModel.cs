using System.Collections.ObjectModel;
using EasySave.App.Services;

namespace EasySave.App.Gui.ViewModels
{
    public class LogsViewModel : ViewModelBase
    {
        private readonly LogReaderService _logReader;
        public ObservableCollection<LogFileEntry> Logs { get; } = new ObservableCollection<LogFileEntry>();

        public LogsViewModel()
        {
            _logReader = new LogReaderService();
            LoadLogs();
        }

        public LogsViewModel(LogReaderService logReader)
        {
            _logReader = logReader;
            LoadLogs();
        }

        public void RefreshLogs()
        {
            LoadLogs();
        }

        private void LoadLogs()
        {
            Logs.Clear();
            foreach (var logEntry in _logReader.ReadLogFiles())
            {
                Logs.Add(logEntry);
            }
        }
    }
}
