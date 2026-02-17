using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EasySave.App.Gui.Models;
using EasySave.App.Services;

namespace EasySave.App.Gui.ViewModels
{
    /// <summary>
    /// ViewModel gérant l'affichage et le filtrage des logs dans l'interface utilisateur.
    /// Suit le pattern MVVM (Model-View-ViewModel).
    /// </summary>
    public class LogsViewModel : ViewModelBase
    {
        private readonly LogReaderService _logReader;

        private List<LogEntryItem> _allLogsCache = new();

        public ObservableCollection<LogEntryItem> Logs { get; } = new();

        public ObservableCollection<string> EventTypes { get; } = new();

        private string _selectedEventType = "Tous";

        /// <summary>
        /// Type d'événement actuellement sélectionné par l'utilisateur.
        /// Déclenche automatiquement le filtrage lors de sa modification.
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

        public LogsViewModel() : this(new LogReaderService()) { }

        /// <summary>
        /// Constructeur principal avec injection du service de lecture de logs.
        /// </summary>
        /// <param name="logReader">Instance du service LogReaderService.</param>
        public LogsViewModel(LogReaderService logReader)
        {
            _logReader = logReader;
            LoadLogs();
        }

        /// <summary>
        /// Réinitialise les filtres et recharge les données depuis le disque.
        /// </summary>
        public void RefreshLogs()
        {
            SelectedEventType = "Tous";
            LoadLogs();
        }

        /// <summary>
        /// Charge les logs, les trie par date décroissante et met à jour la liste des types d'événements disponibles.
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

        /// <summary>
        /// Filtre la collection de cache selon le type sélectionné et met à jour la collection observable.
        /// </summary>
        private void ApplyFilter()
        {
            Logs.Clear();

            // Si "Tous" est sélectionné, on prend tout, sinon on filtre par nom d'événement
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