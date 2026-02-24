using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly SynchronizationContext? _uiContext;
        private int _loadVersion;

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
                // SetProperty gere le INotifyPropertyChanged.
                if (SetProperty(ref _selectedEventType, value))
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
            _uiContext = SynchronizationContext.Current;
            _logReader = logReader;
            LoadLogs();
        }

        /// <summary>
        /// Reloads log data from disk and resets the active event-type filter to "Tous".
        /// </summary>
        public void RefreshLogs()
        {
            SelectedEventType = "Tous";
            LoadLogs();
        }

        /// <summary>
        /// Reads all log entries, rebuilds the cache sorted by descending timestamp,
        /// updates the available event-type list, and reapplies the current filter.
        /// </summary>
        private void LoadLogs()
        {
            int loadVersion = Interlocked.Increment(ref _loadVersion);
            _ = Task.Run(() =>
            {
                List<LogEntryItem> cache;
                List<string> namesFound;

                try
                {
                    // Lecture/parsing hors thread UI pour eviter les freezes reseau/IO.
                    var rawEntries = _logReader.ReadAllEntries();
                    cache = rawEntries
                        .OrderByDescending(entry => entry.TimestampUtc)
                        .Select(entry => new LogEntryItem(entry))
                        .ToList();

                    namesFound = cache
                        .Select(l => l.Entry.Event?.Name)
                        .Where(n => !string.IsNullOrEmpty(n))
                        .Distinct()
                        .OrderBy(n => n)
                        .ToList()!;
                }
                catch
                {
                    cache = new List<LogEntryItem>();
                    namesFound = new List<string>();
                }

                if (loadVersion != Volatile.Read(ref _loadVersion))
                    return;

                void Apply()
                {
                    if (loadVersion != Volatile.Read(ref _loadVersion))
                        return;

                    _allLogsCache = cache;

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

                    foreach (var name in namesFound.Where(name => !EventTypes.Contains(name)))
                    {
                        EventTypes.Add(name);
                    }

                    OnPropertyChanged(nameof(SelectedEventType));
                    ApplyFilter();
                }

                if (_uiContext != null)
                {
                    _uiContext.Post(_ => Apply(), null);
                }
                else
                {
                    Apply();
                }
            });
        }

        /// <summary>
        /// Applies the current event-type filter to the cached entries
        /// and refreshes the observable collection bound to the UI.
        /// </summary>
        private void ApplyFilter()
        {
            Logs.Clear();

            // Si "Tous" est selectionne, on prend tout, sinon on filtre par nom d'evenement.
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
