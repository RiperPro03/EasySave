# Design Patterns utilises dans EasySave

Ce document decrit les patterns identifies dans le projet, leur implementation concrete, leur interet, et des exemples de features faciles a ajouter grace a eux.

## 1. Factory

Objectif: centraliser la creation d'objets selon une configuration.

Fichiers impliques:
- `src/EasySave.EasyLog/Factories/LoggerFactory.cs`: construit `ILogger<T>` selon `LogFormat` et `LogStorageMode`.
- `src/EasySave.EasyLog/Factories/LogReaderFactory.cs`: construit `ILogReader<T>` selon le mode de stockage.
- `src/EasySave.App/Services/AppLogService.cs`: consomme la factory et recree le logger quand la config change.

Fonctionnement:
- Les `switch` dans les factories selectionnent les implementations concretes (JSON/XML, local/serveur/mixte).
- Le code appelant reste decouple des classes concretes.

Avantages:
- Ajout d'un nouveau mode sans modifier les appelants.
- Point unique de validation des options.

Exemple de nouvelle feature:
- Ajouter `LogFormat.Csv` avec `CsvSerializer` et le brancher uniquement dans `LoggerFactory`.

## 2. Builder

Objectif: construire des objets de log complexes de facon fluide.

Fichiers impliques:
- `src/EasySave.Core/Logging/LogEntryBuilder.cs`: API fluide `Create().WithJob().WithFile().WithCrypto().Build()`.
- `src/EasySave.App/Services/BackupEngine.cs`: construit les logs metier par etapes.
- `src/EasySave.App/Services/BackupService.cs`: construit les logs de controle utilisateur.
- `src/EasySave.App/Repositories/AppConfigRepository.cs`: construit les logs de sauvegarde des settings.

Fonctionnement:
- Le builder initialise les champs obligatoires.
- Les methodes `With...` enrichissent l'entree.
- `Fail(...)` applique un standard d'erreur coherent.

Avantages:
- Lisibilite des appels.
- Uniformite du schema de logs.

Exemple de nouvelle feature:
- Ajouter `WithCorrelation(string correlationId)` pour relier logs GUI/API/serveur.

## 3. Strategy (serialisation des logs)

Objectif: choisir l'algorithme de serialisation a runtime.

Fichiers impliques:
- `src/EasySave.EasyLog/Interfaces/ILogSerializer.cs`: contrat.
- `src/EasySave.EasyLog/Serialization/JsonSerializer.cs`: strategie JSON.
- `src/EasySave.EasyLog/Serialization/XmlSerializer.cs`: strategie XML.
- `src/EasySave.EasyLog/Factories/LoggerFactory.cs`: selection de la strategie.

Fonctionnement:
- Le logger depend de `ILogSerializer`, pas d'un format concret.
- La factory injecte la bonne implementation selon `LogOptions.Format`.

Avantages:
- Formats interchangeables sans toucher aux loggers.
- Tests simplifies via mock de `ILogSerializer`.

Exemple de nouvelle feature:
- Support `protobuf` pour les logs distants a faible bande passante.

## 4. Strategy (copie Full/Differential)

Objectif: separer la regle "doit-on copier ce fichier?".

Fichiers impliques:
- `src/EasySave.Core/Interfaces/IBackupCopyStrategy.cs`: contrat de decision.
- `src/EasySave.App/Services/FullCopyStrategy.cs`: copie toujours.
- `src/EasySave.App/Services/DifferentialCopyStrategy.cs`: compare presence/taille/date/hash.
- `src/EasySave.App/Services/BackupEngine.cs`: choisit la strategie selon `BackupType`.

Fonctionnement:
- Le moteur appelle `strategy.ShouldCopy(source, target)` pour chaque fichier.
- Le comportement change sans branchements disperses dans la boucle principale.

Avantages:
- Extension simple des types de backup.
- Cohesion de la logique de comparaison.

Exemple de nouvelle feature:
- Ajouter `MirrorCopyStrategy` pour supprimer les fichiers cibles orphelins.

## 5. Strategy (chiffrement)

Objectif: isoler le mecanisme de chiffrement.

Fichiers impliques:
- `src/EasySave.Core/Interfaces/ICryptoService.cs`: contrat.
- `src/EasySave.App/Services/CryptoSoftProcessService.cs`: chiffrement via process externe.
- `src/EasySave.App/Services/NoEncryptionService.cs`: fallback no-op.
- `src/EasySave.App/Services/BackupEngine.cs`: applique la strategie active.

Fonctionnement:
- Le moteur reste agnostique de l'implementation.
- La creation par defaut choisit `CryptoSoftProcessService` si binaire present, sinon fallback.

Avantages:
- Fiabilite du fallback.
- Evolution facile vers d'autres providers.

Exemple de nouvelle feature:
- Ajouter `AesEncryptionService` natif .NET sans changer le moteur.

## 6. Decorator

Objectif: ajouter un comportement autour d'un logger sans modifier son code.

Fichiers impliques:
- `src/EasySave.EasyLog/Loggers/SafeLogger.cs`: enveloppe un `ILogger<T>` et absorbe les exceptions.
- `src/EasySave.EasyLog/Factories/LoggerFactory.cs`: active le decorator via `UseSafeLogger`.

Fonctionnement:
- `SafeLogger.Write` appelle le logger interne dans un `try/catch`.
- Le pattern est optionnel et activable par configuration.

Avantages:
- Robustesse en production.
- Separation claire entre logique metier et politique de resilience.

Exemple de nouvelle feature:
- `RetryLogger` decorator pour retenter les ecritures distantes.

## 7. Repository

Objectif: encapsuler l'acces aux donnees persistantes.

Fichiers impliques:
- `src/EasySave.Core/Interfaces/IJobRepository.cs`: abstraction.
- `src/EasySave.App/Repositories/JobRepository.cs`: persistance JSON des jobs.
- `src/EasySave.App/Repositories/AppConfigRepository.cs`: persistance JSON de la config.
- `src/EasySave.App/Services/JobService.cs`: depend de `IJobRepository`.
- `src/EasySave.App/Services/SettingsService.cs`: depend de `AppConfigRepository`.

Fonctionnement:
- Le service metier ne connait pas les details de fichier/JSON.
- Les repositories gerent verrouillage, serialisation et migration defensive.

Avantages:
- Testabilite de la couche metier.
- Possibilite de changer le backend de persistance.

Exemple de nouvelle feature:
- Remplacer la persistance JSON des jobs par SQLite avec une nouvelle implementation repository.

## 8. Command (MVVM)

Objectif: encapsuler les actions UI dans des commandes bindables.

Fichiers impliques:
- `src/EasySave.App.Gui/ViewModels/ExecutionViewModel.cs`
- `src/EasySave.App.Gui/ViewModels/JobsViewModel.cs`
- `src/EasySave.App.Gui/ViewModels/MainWindowViewModel.cs`
- `src/EasySave.App.Gui/ViewModels/SettingsViewModel.cs`

Fonctionnement:
- Les methodes marquees `[RelayCommand]` sont exposees comme commandes Avalonia.
- La vue declenche des intentions, pas des appels metier directs.

Avantages:
- UI decouplee et testable.
- Gestion uniforme des actions synchrones/asynchrones.

Exemple de nouvelle feature:
- Ajouter une commande `RetryFailedJobsCommand` dans `ExecutionViewModel`.

## 9. Singleton

Objectif: partager une instance unique de service transversal.

Fichiers impliques:
- `src/EasySave.App.Gui/Localization/Loc.cs`: `public static Loc Instance { get; } = new();`

Fonctionnement:
- Toute la GUI consomme la meme instance de localisation.
- Le changement de langue notifie tous les bindings via `PropertyChanged`.

Avantages:
- Source unique de verite pour la culture.
- Pas de propagation manuelle des instances.

Exemple de nouvelle feature:
- Ajouter un cache de traductions dynamiques dans `Loc.Instance`.

## 10. Observer (events)

Objectif: diffuser les changements d'etat sans couplage fort.

Fichiers impliques:
- `src/EasySave.Core/Interfaces/IBackupService.cs`: evenement `StateChanged`.
- `src/EasySave.App/Services/BackupEngine.cs`: publie les transitions de job.
- `src/EasySave.App/Services/BackupService.cs`: relaie l'evenement et persiste l'instantane.
- `src/EasySave.App.Gui/ViewModels/ExecutionViewModel.cs`: s'abonne et met a jour l'ecran live.
- `src/EasySave.App.Gui/ViewModels/DashboardViewModel.cs`: s'abonne pour les widgets dashboard.
- `src/EasySave.App.Gui/ViewModels/MainWindowViewModel.cs`: s'abonne pour notifications globales.

Fonctionnement:
- Producteur: moteur de backup.
- Intermediaire: service applicatif.
- Consommateurs: ViewModels.

Avantages:
- Multiples consommateurs sans dependances croisees.
- Ajout de nouveaux listeners sans toucher au moteur.

Exemple de nouvelle feature:
- Ajouter un listener qui pousse les events sur SignalR pour supervision distante.

## 11. Facade (service applicatif)

Objectif: exposer une API metier simple et cacher la complexite interne.

Fichier implique:
- `src/EasySave.App/Services/BackupService.cs`

Fonctionnement:
- `BackupService` fournit `Run/Pause/Resume/Stop/CanStartSequence`.
- En interne, il orchestre moteur, snapshots, logs, synchronisation, protection anti double lancement.

Avantages:
- Point d'entree unique pour GUI/CLI.
- Evolution interne sans impact public.

Exemple de nouvelle feature:
- Ajouter `RunBatch(IEnumerable<string> jobIds)` uniquement dans la facade.

## 12. Producer-Consumer

Objectif: decoupler production de messages et traitement IO.

Fichiers impliques:
- `src/LogHub.Server/WebSockets/WebSocketEndpoint.cs`: producteur (enqueue des ecritures).
- `src/LogHub.Server/Infrastructure/Queueing/ChannelLogQueue.cs`: buffer borne.
- `src/LogHub.Server/Workers/LogIngestWorker.cs`: consommateur en fond.
- `src/LogHub.Server/Infrastructure/Storage/DailyFileLogWriter.cs`: ecriture disque.

Fonctionnement:
- Les requetes WS n'ecrivent pas directement sur disque.
- Elles placent une tache en queue, puis le worker traite sequentiellement.

Avantages:
- Meilleure tenue en charge.
- Isolation des latences disque vis-a-vis des clients WS.

Exemple de nouvelle feature:
- Ajouter une politique de priorite de messages (erreurs > infos) dans la queue.

## Architecture

## A. Pipeline

Observation:
- Pipeline de traitement net: ingress WS -> validation -> queue -> worker -> storage.

Fichiers:
- `src/LogHub.Server/WebSockets/WebSocketEndpoint.cs`
- `src/LogHub.Server/Infrastructure/Queueing/ChannelLogQueue.cs`
- `src/LogHub.Server/Workers/LogIngestWorker.cs`
- `src/LogHub.Server/Infrastructure/Storage/DailyFileLogWriter.cs`

Exemple de feature:
- Inserer une etape de sanitization RGPD avant l'ecriture.
