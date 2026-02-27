# Design Patterns utilises dans EasySave

## Introduction

Ce document sert a montrer pourquoi le code est maintenable:
- separer les responsabilites (metier, persistence, UI, transport),
- remplacer un composant sans casser les autres,
- ajouter des features par extension plutôt que par modification risquée,
- garder une base testable (interfaces, evenements, orchestration explicite).

Patterns decrits dans ce document et raison d'existence:
- `Factory`: créer la bonne implementation selon la configuration.
- `Builder`: construire des logs riches de maniere uniforme.
- `Strategy`: changer l'algorithme (copie, serialisation, chiffrement) sans changer l'orchestrateur.
- `Decorator`: ajouter un comportement de securite autour d'un logger sans dupliquer les loggers existants.
- `Repository`: isoler la persistence des regles metier.
- `Command` (MVVM): declencher des actions UI sans coupler la vue au metier.
- `Singleton`: partager une source unique de verite transversale.
- `Observer`: propager les changements d'etat vers plusieurs consommateurs.
- `Facade`: exposer une API metier simple au-dessus d'une orchestration complexe.
- `Producer-Consumer`: decoupler ingestion et ecriture IO pour la charge.
- `Pipeline` (architecture): chaine de traitement claire et extensible.
- `State` (etat implicite): transitions de statut centralisees.
- `Options Pattern` (.NET): centraliser les parametres runtime du serveur.

## 1. Factory

But:
- centraliser la creation de Reader/Writer de logs selon `LogFormat` et `LogStorageMode`.

Fichiers et role:
- `src/EasySave.EasyLog/Factories/LoggerFactory.cs`: cree `DailyLogger`, `WebSocketLogger`, `SafeLogger`, ou le mode mixte.
- `src/EasySave.EasyLog/Factories/LogReaderFactory.cs`: cree `LocalFileLogReader`, `WebSocketLogReader`, ou le mode mixte.
- `src/EasySave.App/Services/AppLogService.cs`: demande la creation et recrée le logger quand la config evolue.

Comment ca marche:
- une decision centralisee par `switch` sur les options.
- les appelants manipulent uniquement `ILogger<T>` et `ILogReader<T>`.

Avantage maintenabilite:
- ajout d'un backend en un seul endroit.
- reduction du couplage aux classes concretes.

Exemple de feature :
- ajout d'un backend `DatabaseLogger` + `DatabaseLogReader` sans toucher l'orchestration.

## 2. Builder

But:
- construire des `LogEntryDto` complexes sans duplication de code.

Fichiers et role:
- `src/EasySave.Core/Logging/LogEntryBuilder.cs`: API fluide de construction.
- utilise dans:
  - `src/EasySave.App/Services/BackupEngine.cs`
  - `src/EasySave.App/Services/BackupService.cs`
  - `src/EasySave.App/Repositories/AppConfigRepository.cs`

Comment ca marche:
- `Create(...)` initialise le minimum.
- `WithJob/WithFile/WithCrypto/WithSummary/...` enrichissent l'entree.
- `Fail(...)` applique un format d'erreur coherent.

Avantage maintenabilite:
- schema de logs homogene.
- evolution du DTO sans rework massif des callsites.

Exemple de feature :
- ajouter `WithCorrelationId(...)` pour tracer une operation de bout en bout.

## 3. Strategy (serialisation)

But:
- changer le format de serialisation sans changer le logger.

Fichiers et role:
- `src/EasySave.EasyLog/Interfaces/ILogSerializer.cs`
- `src/EasySave.EasyLog/Serialization/JsonSerializer.cs`
- `src/EasySave.EasyLog/Serialization/XmlSerializer.cs`
- selection dans `src/EasySave.EasyLog/Factories/LoggerFactory.cs`

Comment ca marche:
- le logger depend de l'interface `ILogSerializer`.
- la factory injecte la strategie concrete.

Avantage maintenabilite:
- format extensible.
- tests plus simples via mock/fake serializer.

Exemple de feature :
- ajouter `CsvSerializer` pour générer des log en format CSV.

## 4. Strategy (copie Full / Differential)

But:
- isoler la decision "copier ou non" par fichier.

Fichiers et role:
- `src/EasySave.Core/Interfaces/IBackupCopyStrategy.cs`
- `src/EasySave.App/Services/FullCopyStrategy.cs`
- `src/EasySave.App/Services/DifferentialCopyStrategy.cs`
- orchestration dans `src/EasySave.App/Services/BackupEngine.cs`

Comment ca marche:
- le moteur appelle `ShouldCopy(source, target)`.
- la regle varie selon le type de backup.

Avantage maintenabilite:
- ajout d'un nouveau mode sans complexifier la boucle principale.

Exemple de feature :
- `MirrorCopyStrategy` avec purge des fichiers cibles orphelins.

## 5. Strategy (chiffrement)

But:
- isoler la technologie de chiffrement du moteur de backup.

Fichiers et role:
- `src/EasySave.Core/Interfaces/ICryptoService.cs`
- `src/EasySave.App/Services/CryptoSoftProcessService.cs`
- `src/EasySave.App/Services/NoEncryptionService.cs`
- selection/fallback dans `src/EasySave.App/Services/BackupEngine.cs`

Comment ca marche:
- le moteur appelle `ICryptoService`.
- fallback automatique vers no-op si l'outil externe est absent.

Avantage maintenabilite:
- robustesse (pas de crash si chiffrement indisponible).
- remplacement de provider sans toucher la logique metier.

Exemple de feature :
- `AesEncryptionService` comme nouvelle strategie de chiffrement.

## 6. Decorator

But:
- ajouter un comportement de securite autour d'un logger existant.

Fichiers et role:
- `src/EasySave.EasyLog/Loggers/SafeLogger.cs`
- branchement dans `src/EasySave.EasyLog/Factories/LoggerFactory.cs`

Comment ca marche:
- `SafeLogger` enveloppe un `ILogger<T>` et capte les exceptions.

Avantage maintenabilite:
- resilience optionnelle sans dupliquer les loggers.

Exemple de feature :
- `RetryLogger` (decorator de retry pour le distant).

## 7. Repository

But:
- encapsuler la persistence et proteger la logique metier des details IO.

Fichiers et role:
- `src/EasySave.Core/Interfaces/IJobRepository.cs`
- `src/EasySave.App/Repositories/JobRepository.cs`
- `src/EasySave.App/Repositories/AppConfigRepository.cs`
- services consommateurs:
  - `src/EasySave.App/Services/JobService.cs`
  - `src/EasySave.App/Services/SettingsService.cs`

Comment ca marche:
- les services appellent des operations metier (`GetAll`, `Add`, `Update`, etc.).
- les repositories gerent JSON, verrous, validation defensive.

Avantage maintenabilite:
- migration de stockage possible sans impacter les services.

Exemple de feature :
- migration jobs JSON -> SQLite.

## 8. Command (MVVM)

But:
- representer les actions UI en commandes bindables/testables.

Fichiers et role:
- view models avec `[RelayCommand]`:
  - `src/EasySave.App.Gui/ViewModels/ExecutionViewModel.cs`
  - `src/EasySave.App.Gui/ViewModels/JobsViewModel.cs`
  - `src/EasySave.App.Gui/ViewModels/MainWindowViewModel.cs`
  - `src/EasySave.App.Gui/ViewModels/SettingsViewModel.cs`

Comment ca marche:
- la vue declenche des commandes, pas des appels metier directs.

Avantage maintenabilite:
- meilleure separation Vue / ViewModel / Services.

Exemple de feature :
- commande `RetryFailedJobs` pour relancer tous les jobs en erreur d'un coup.

## 9. Singleton

But:
- partager une instance globale coherente pour la localisation GUI.

Fichiers et role:
- `src/EasySave.App.Gui/Localization/Loc.cs` (`Loc.Instance`)

Comment ca marche:
- changement de langue central.
- notification via `PropertyChanged`.

Avantage maintenabilite:
- source unique de configuration de langue.

## 10. Observer

But:
- diffuser les changements d'etat aux consommateurs sans couplage fort.

Fichiers et role:
- contrats:
  - `src/EasySave.Core/Interfaces/IBackupEngine.cs`
  - `src/EasySave.Core/Interfaces/IBackupService.cs`
- publication:
  - `src/EasySave.App/Services/BackupEngine.cs`
  - `src/EasySave.App/Services/BackupService.cs`
- abonnements:
  - `src/EasySave.App.Gui/ViewModels/ExecutionViewModel.cs`
  - `src/EasySave.App.Gui/ViewModels/DashboardViewModel.cs`
  - `src/EasySave.App.Gui/ViewModels/MainWindowViewModel.cs`

Comment ca marche:
- `StateChanged` est publie par le moteur et relaie par le service.
- plusieurs view models reagissent independamment.

Avantage maintenabilite:
- ajout de nouveaux observateurs sans modification du moteur.

Exemple de feature :
- observateur de telemetrie temps reel vers un dashboard distant.

## 11. Facade

But:
- presenter une API metier simple au-dessus d'une orchestration complexe.

Fichier et role:
- `src/EasySave.App/Services/BackupService.cs`

Comment ca marche:
- expose `Run`, `Pause`, `Resume`, `Stop`, `CanStartSequence`.
- masque la coordination moteur, snapshots, logs, synchronisation.

Avantage maintenabilite:
- point d'entrée unique pour GUI et CLI.

Exemple de feature :
- `RunBatch(IEnumerable<string> ids)`.

## 12. Producer-Consumer

But:
- absorber les pics d'entrees WS et decoupler ecriture disque.

Fichiers et role:
- producteur:
  - `src/LogHub.Server/WebSockets/WebSocketEndpoint.cs`
- queue:
  - `src/LogHub.Server/Infrastructure/Queueing/ChannelLogQueue.cs`
- consommateur:
  - `src/LogHub.Server/Workers/LogIngestWorker.cs`
- stockage:
  - `src/LogHub.Server/Infrastructure/Storage/DailyFileLogWriter.cs`

Comment ca marche:
- le endpoint enfile les ecritures.
- le worker dépile et persiste en arriere-plan.

Avantage maintenabilite:
- meilleure tenue en charge et isolation IO.

Exemple de feature :
- priorité de messages (erreur > info).

## 13. State (etat implicite / machine a etats)

But:
- gerer explicitement les transitions d'un job.

Fichiers et role:
- `src/EasySave.Core/Enums/JobStatus.cs`
- `src/EasySave.App/Services/BackupEngine.cs`
- `src/EasySave.App/Services/JobExecutionControl.cs`

Comment ca marche:
- transitions `Idle -> Running -> Paused -> Running -> Completed/Error`.
- controles `Pause/Resume/Stop` centralises.

Avantage maintenabilite:
- comportement previsible et transitions traceables.

Exemple de feature :
- ajout de l'etat `Cancelling`.

## 14. Pipeline (architecture)

But:
- decrire une chaine de traitement claire et decoupee.

Fichiers et role:
- `src/LogHub.Server/WebSockets/WebSocketEndpoint.cs`
- `src/LogHub.Server/Infrastructure/Queueing/ChannelLogQueue.cs`
- `src/LogHub.Server/Workers/LogIngestWorker.cs`
- `src/LogHub.Server/Infrastructure/Storage/DailyFileLogWriter.cs`

Comment ca marche:
- reception -> validation -> enqueue -> ecriture.

Avantage maintenabilite:
- ajout d'etapes intermediaires sans casser le flux.

Exemple de feature :
- étape d'authentification avant l'enqueue.

## 15. Options Pattern (.NET)

But:
- centraliser et typer la configuration du serveur LogHub.

Fichiers et role:
- `src/LogHub.Server/Options/LogHubOptions.cs`
- `src/LogHub.Server/Program.cs` (`Configure<LogHubOptions>`)
- consommateurs:
  - `src/LogHub.Server/Infrastructure/Queueing/ChannelLogQueue.cs`
  - `src/LogHub.Server/Infrastructure/Storage/DailyFileLogWriter.cs`
  - `src/LogHub.Server/WebSockets/WebSocketEndpoint.cs`

Comment ca marche:
- binding config -> objet options.
- injection `IOptions<LogHubOptions>` dans les services.

Avantage maintenabilite:
- configuration centralisee, validee, et facile a faire evoluer.

Exemple de feature :
- ajouter `MaxPayloadBytes` dans `LogHubOptions` et l'appliquer au endpoint WS.

## 16. Pattern MVVM (Avalonia UI)

But :
-Découpler totalement l'interface graphique (XAML) de la logique métier pour permettre une testabilité accrue et une maintenance facilitée de l'UI.

Comment ça marche :
- Data Binding : La View "écoute" les propriétés du ViewModel qu'elle a via le moteur de liaison d'Avalonia.
- Notification : Le ViewModel implémente ObservableObject (ou INotifyPropertyChanged) pour avertir la View d'un changement de donnée.
- Commandes : Les interactions utilisateur (clics) appellent des ICommand définies dans le ViewModel.

Avantage maintenabilité :
- On peut modifier entièrement le design (XAML) sans toucher à une seule ligne de code C#.
- Les tests unitaires s'effectuent sur le ViewModel sans même avoir besoin de lancer l'interface graphique.

## 17. Pattern MVC (du mode console)

But :
- Structurer l'application CLI pour séparer le flux d'entrée utilisateur, le traitement logique et le formatage de sortie.

Fichiers :
- Model : `src/EasySave.App.Console/Models`
- View : `src/EasySave.App.Console/Views`
- Controller : `src/EasySave.App.Console/Controllers`

Comment ça marche :
- Input : Le Program.cs transmet les arguments au Controller.
- Action : Le Controller interroge les services (Infrastructure) et met à jour le Model.
- Output : Le Controller sélectionne la View appropriée pour afficher le résultat à l'utilisateur.

Avantage maintenabilité :
- Modifier l’interface n’impacte pas la logique métier, et inversement. 

## Preuve de maintenabilite (synthese)

Ce que ces patterns prouvent concretement:
- extensibilite: ajout de formats/strategies sans refonte globale.
- isolations nettes: UI, metier, persistence, transport restent separables.
- testabilite: interfaces et injection permettent de tester chaque couche.
- resilience: decorators, fallback strategies, producer-consumer limitent les pannes en cascade.
- evolutivite: facade + factories + repositories reduisent le cout des changements. Builder assure une construction de logs robuste face a l'evolution du schema.
