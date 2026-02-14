using System.Collections.ObjectModel;
using System.Linq;
using EasySave.App.Gui.Models;
using EasySave.App.Services;

namespace EasySave.App.Gui.ViewModels
{
    public class LogsViewModel : ViewModelBase
    {
        private readonly LogReaderService _logReader;
        public ObservableCollection<LogEntryItem> Logs { get; } = new ObservableCollection<LogEntryItem>();

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
            var entries = _logReader.ReadAllEntries()
                .OrderByDescending(entry => entry.TimestampUtc)
                .Select(entry => new LogEntryItem(entry));

            foreach (var logEntry in entries)
            {
                Logs.Add(logEntry);
            }
        }
    }
}
