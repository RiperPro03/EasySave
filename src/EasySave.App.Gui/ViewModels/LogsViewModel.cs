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

        public ObservableCollection<LogEntryItem> Logs { get; } = new();
        public ObservableCollection<string> EventTypes { get; } = new();

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
            SelectedEventType = "Tous";
            LoadLogs();
        }

        private void LoadLogs()
        {
            var rawEntries = _logReader.ReadAllEntries();

            _allLogsCache = rawEntries
                .OrderByDescending(entry => entry.TimestampUtc)
                .Select(entry => new LogEntryItem(entry))
                .ToList();

            var namesFound = _allLogsCache
                .Select(l => l.Entry.Event?.Name)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            if (!EventTypes.Contains("Tous"))
            {
                EventTypes.Clear();
                EventTypes.Add("Tous");
            }

            for (int i = EventTypes.Count - 1; i >= 1; i--)
            {
                if (!namesFound.Contains(EventTypes[i]))
                    EventTypes.RemoveAt(i);
            }

            foreach (var name in namesFound)
            {
                if (!EventTypes.Contains(name))
                    EventTypes.Add(name);
            }

            OnPropertyChanged(nameof(SelectedEventType));
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            Logs.Clear();

            var filtered = (_selectedEventType == "Tous" || string.IsNullOrEmpty(_selectedEventType))
                ? _allLogsCache
                : _allLogsCache.Where(l => l.Entry.Event?.Name == _selectedEventType);

            foreach (var log in filtered)
            {
                Logs.Add(log);
            }
        }
    }
}