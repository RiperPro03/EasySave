using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EasySave.App.Gui.Models;
using EasySave.App.Services;

namespace EasySave.App.Gui.ViewModels
{
    /// <summary>
    /// Provides log data and filtering state for the logs view.
    /// Maintains a cached list of all entries and exposes a filtered observable collection for the UI.
    /// </summary>
    public class LogsViewModel : ViewModelBase
    {
        private readonly LogReaderService _logReader;

        private List<LogEntryItem> _allLogsCache = new();

        public ObservableCollection<LogEntryItem> Logs { get; } = new();

        public ObservableCollection<string> EventTypes { get; } = new();

        private string _selectedEventType = "Tous";

        /// <summary>
        /// Gets or sets the event-type filter selected by the user.
        /// Updating this value reapplies the filter to <see cref="Logs"/>.
        /// </summary>
        public string SelectedEventType
        {
            get => _selectedEventType;
            set
            {
                // SetProperty gère le INotifyPropertyChanged
                if (SetProperty(ref _selectedEventType, value))
                {
                    ApplyFilter();
                }
            }
        }

        private string _searchIdText = string.Empty;

        /// <summary>
        /// Gets or sets the ID search text (Job ID or Trace ID). 
        /// Updates and reapplies filters to <see cref="Logs"/>.
        /// </summary>
        public string SearchIdText
        {
            get => _searchIdText;
            set
            {
                if (SetProperty(ref _searchIdText, value))
                {
                    ApplyFilter();
                }
            }
        }

        public LogsViewModel() : this(new LogReaderService()) { }

        /// <summary>
        /// Initializes a new instance of the view model using the specified log reader service.
        /// Loads the initial log data and available filter values.
        /// </summary>
        /// <param name="logReader">Service used to read log entries from storage.</param>
        public LogsViewModel(LogReaderService logReader)
        {
            _logReader = logReader;
            LoadLogs();
        }

        /// <summary>
        /// Reloads log data from disk and resets the active filters.
        /// </summary>
        public void RefreshLogs()
        {
            _searchIdText = string.Empty;
            OnPropertyChanged(nameof(SearchIdText));
            SelectedEventType = "Tous";
            LoadLogs();
        }

        /// <summary>
        /// Reads all log entries, rebuilds the cache sorted by descending timestamp,
        /// updates the available event-type list, and reapplies the current filter.
        /// </summary>
        private void LoadLogs()
        {
            // Récupération des données brutes via le service
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

            // CORRECTION ERREUR INDEX : Utilisation d'une liste temporaire
            var typesToRemove = EventTypes.Where(t => t != "Tous" && !namesFound.Contains(t)).ToList();
            foreach (var type in typesToRemove)
            {
                EventTypes.Remove(type);
            }

            foreach (var name in namesFound)
            {
                if (name != null && !EventTypes.Contains(name))
                {
                    EventTypes.Add(name);
                }
            }

            OnPropertyChanged(nameof(SelectedEventType));
            ApplyFilter();
        }

        /// <summary>
        /// Applies the current event-type filter and ID search to the cached entries.
        /// </summary>
        private void ApplyFilter()
        {
            Logs.Clear();

            var filtered = _allLogsCache.AsEnumerable();

            // Filtre par Type d'événement
            if (!string.IsNullOrEmpty(_selectedEventType) && _selectedEventType != "Tous")
            {
                filtered = filtered.Where(l => l.Entry.Event?.Name == _selectedEventType);
            }

            // Filtre par ID (Vérifie si Job.Id ou Trace.Id COMMENCE par la saisie)
            if (!string.IsNullOrWhiteSpace(_searchIdText))
            {
                filtered = filtered.Where(l =>
                {
                    var jobId = l.Entry.Job?.Id;
                    var traceId = l.Entry.Trace?.Id;

                    // Utilisation de StartsWith pour chercher uniquement le début de la chaîne
                    return (jobId != null && jobId.StartsWith(_searchIdText, StringComparison.OrdinalIgnoreCase)) ||
                           (traceId != null && traceId.StartsWith(_searchIdText, StringComparison.OrdinalIgnoreCase));
                });
            }

            foreach (var log in filtered)
            {
                Logs.Add(log);
            }
        }
    }
}