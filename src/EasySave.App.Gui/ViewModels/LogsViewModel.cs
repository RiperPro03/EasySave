using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EasySave.App.Gui.Models;
using EasySave.App.Services;

namespace EasySave.App.Gui.ViewModels
{
    public class LogsViewModel : ViewModelBase
    {
        private readonly LogReaderService _logReader;
        private List<LogEntryItem> _allLogsCache = new();

        public ObservableCollection<LogEntryItem> Logs { get; } = new ObservableCollection<LogEntryItem>();
        public ObservableCollection<string> EventTypes { get; } = new ObservableCollection<string>();

        private string _selectedEventType = "Tous";
        public string SelectedEventType
        {
            get => _selectedEventType;
            set
            {
                if (SetProperty(ref _selectedEventType, value))
                {
                    ApplyFilter();
                }
            }
        }

        public LogsViewModel() : this(new LogReaderService()) { }

        public LogsViewModel(LogReaderService logReader)
        {
            _logReader = logReader;
            LoadLogs();
        }

        public void RefreshLogs()
        {
            _selectedEventType = "Tous";
            OnPropertyChanged(nameof(SelectedEventType));
            LoadLogs();
        }

        private void LoadLogs()
        {
            var entries = _logReader.ReadAllEntries()
                .OrderByDescending(entry => entry.TimestampUtc)
                .Select(entry => new LogEntryItem(entry))
                .ToList();

            _allLogsCache = entries;

            EventTypes.Clear();
            EventTypes.Add("Tous");

            var names = _allLogsCache
                .Select(l => l.Entry.Event?.Name)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .OrderBy(n => n);

            foreach (var name in names)
            {
                EventTypes.Add(name);
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            Logs.Clear();
            var filtered = (_selectedEventType == "Tous")
                ? _allLogsCache
                : _allLogsCache.Where(l => l.Entry.Event?.Name == _selectedEventType);

            foreach (var log in filtered)
            {
                Logs.Add(log);
            }
        }
    }
}