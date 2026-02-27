# Patterns de conception et choix d'architecture dans EasySave

Reference :
- Catalogue Refactoring.Guru: https://refactoring.guru/design-patterns/catalog

## 1. Factory

But:
- centraliser la creation des loggers et readers selon la configuration de log.

Fichiers et role:
- `src/EasySave.EasyLog/Factories/LoggerFactory.cs`
- `src/EasySave.EasyLog/Factories/LogReaderFactory.cs`
- `src/EasySave.App/Services/AppLogService.cs`

Pourquoi l'usage :
- les appelants ne connaissent pas les classes concretes (`DailyLogger`, `WebSocketLogger`, `SafeLogger`, `LocalFileLogReader`, `WebSocketLogReader`),
- la decision de creation est concentree dans une seule couche,
- la selection se fait a partir d'options metier (`LogFormat`, `LogStorageMode`) au lieu d'etre dispersee dans l'application.

Gain de maintenabilite:
- ajout d'un nouveau backend de log dans un point unique,
- reduction du couplage aux implementations concretes,
- changement de mode local/distant/mixte sans modifier les services metier.

Exemple de feature:
- ajouter `DatabaseLogger` et `DatabaseLogReader` en etendant les factories sans toucher aux consommateurs.

## 2. Builder

But:
- construire des `LogEntryDto` riches et coherents sans dupliquer la logique de construction.

Fichiers et role:
- `src/EasySave.Core/Logging/LogEntryBuilder.cs`
- utilise dans:
  - `src/EasySave.App/Services/BackupEngine.cs`
  - `src/EasySave.App/Services/BackupService.cs`
  - `src/EasySave.App/Repositories/AppConfigRepository.cs`
  - `src/EasySave.App/Services/JobService.cs`

Pourquoi l'usage :
- `Create(...)` impose un noyau minimal commun a toutes les entrees,
- les methodes `WithJob`, `WithFile`, `WithCrypto`, `WithSettings`, `WithSummary` ajoutent progressivement les parties utiles,
- `Fail(...)` normalise la representation des erreurs.

Ce que cela prouve dans le code:
- les services n'assemblent pas eux-memes a la main des sous-objets imbriques,
- la structure des logs reste homogene malgre des contextes differents,
- l'evolution du schema de log est localisee dans un seul composant.

Gain de maintenabilite:
- moins de duplication,
- moins de risque d'oublier un champ obligatoire ou un format d'erreur,
- evolution plus simple du DTO de log.

Exemple de feature:
- ajouter `WithCorrelationId(...)` ou `WithUserAction(...)` sans reecrire tous les call sites.

## 3. Strategy (serialisation)

But:
- changer le format de serialisation sans changer les loggers.

Fichiers et role:
- `src/EasySave.EasyLog/Interfaces/ILogSerializer.cs`
- `src/EasySave.EasyLog/Serialization/JsonSerializer.cs`
- `src/EasySave.EasyLog/Serialization/XmlSerializer.cs`
- injection de la strategie via `src/EasySave.EasyLog/Factories/LoggerFactory.cs`

Pourquoi l'usage :
- les loggers dependent de l'abstraction `ILogSerializer`,
- les implementations portent des algorithmes differents mais une meme responsabilite,
- le choix du format est resolu au moment de la creation, pas code en dur dans `DailyLogger` ou `WebSocketLogger`.

Gain de maintenabilite:
- ajout d'un nouveau format sans modifier le coeur des loggers,
- tests plus simples via fake serializer,
- reduction du couplage entre transport/stockage et format.

Exemple de feature:
- ajouter `CsvSerializer` sans toucher au contrat `ILogger<T>`.

## 4. Strategy (copie Full / Differential)

But:
- isoler la regle "copier ou ne pas copier" pour chaque fichier.

Fichiers et role:
- `src/EasySave.Core/Interfaces/IBackupCopyStrategy.cs`
- `src/EasySave.App/Services/FullCopyStrategy.cs`
- `src/EasySave.App/Services/DifferentialCopyStrategy.cs`
- orchestration dans `src/EasySave.App/Services/BackupEngine.cs`

Pourquoi l'usage :
- le moteur de backup ne code pas toutes les variantes dans une seule grosse boucle conditionnelle,
- la decision par fichier passe par une interface stable,
- chaque strategie exprime clairement sa regle:
  - `FullCopyStrategy`: copie toujours,
  - `DifferentialCopyStrategy`: compare l'etat source/cible avant de copier.

Gain de maintenabilite:
- ajout d'un nouveau mode de sauvegarde sans alourdir la boucle principale,
- responsabilites mieux separees,
- test unitaire plus simple de la regle de copie.

Exemple de feature:
- ajouter `MirrorCopyStrategy` ou `IncrementalCopyStrategy`.

## 5. Strategy (chiffrement)

But:
- changer la politique de chiffrement sans coupler le moteur a une implementation concrete.

Fichiers et role:
- `src/EasySave.Core/Interfaces/ICryptoService.cs`
- `src/EasySave.App/Services/CryptoSoftProcessService.cs`
- `src/EasySave.App/Services/NoEncryptionService.cs`
- selection par defaut dans `src/EasySave.App/Services/BackupEngine.cs`

Pourquoi l'usage :
- le moteur depend de `ICryptoService`,
- `CryptoSoftProcessService` applique le chiffrement reel,
- `NoEncryptionService` est une implementation no-op qui respecte le meme contrat.

Pourquoi la combinaison est interessante:
- le cote `Strategy` permet de remplacer le fournisseur de chiffrement,
- le cote `Null Object` evite des tests `if (cryptoService != null)` partout dans le moteur,
- le fallback reste explicite et robuste quand l'outil externe n'est pas disponible.

Gain de maintenabilite:
- moteur plus simple,
- moins de branches conditionnelles,
- remplacement plus facile du provider de chiffrement.

Exemple de feature:
- ajouter `AesEncryptionService` ou un provider distant sans changer la boucle de backup.

## 6. Decorator

But:
- ajouter un comportement de securite autour d'un logger existant sans dupliquer les loggers concrets.

Fichiers et role:
- `src/EasySave.EasyLog/Loggers/SafeLogger.cs`
- branchement dans `src/EasySave.EasyLog/Factories/LoggerFactory.cs`

Pourquoi l'usage :
- `SafeLogger<T>` implemente la meme interface que l'objet enveloppe (`ILogger<T>`),
- il recoit un logger existant en constructeur,
- il ajoute un comportement transversal: interception des exceptions.

Gain de maintenabilite:
- resilience optionnelle sans duplication de code dans `DailyLogger` et `WebSocketLogger`,
- meilleur respect du principe ouvert/ferme,
- possibilite d'ajouter d'autres decorators sur le meme contrat.

Exemple de feature:
- ajouter un `RetryLogger` ou un `MetricsLogger` autour d'un logger existant.

## 7. Command (MVVM)

But:
- representer les actions utilisateur sous forme de commandes bindables et testables.

Fichiers et role:
- `src/EasySave.App.Gui/ViewModels/ExecutionViewModel.cs`
- `src/EasySave.App.Gui/ViewModels/JobsViewModel.cs`
- `src/EasySave.App.Gui/ViewModels/MainWindowViewModel.cs`
- `src/EasySave.App.Gui/ViewModels/SettingsViewModel.cs`
- `src/EasySave.App.Gui/ViewModels/JobEditorViewModel.cs`

Pourquoi l'usage :
- les actions UI sont exposees via `[RelayCommand]`,
- la vue ne pilote pas directement les services metier,
- le declenchement passe par des commandes, ce qui s'aligne bien avec le modele MVVM.

Ce que cela apporte concretemement:
- la logique d'action reste dans le ViewModel,
- les interactions peuvent etre testees sans manipuler directement la vue Avalonia,
- l'UI reste plus declarative et moins couplee au code metier.

Gain de maintenabilite:
- separation plus nette entre XAML et logique applicative,
- moins de code-behind metier,
- comportements utilisateur plus faciles a faire evoluer.

Exemple de feature:
- ajouter une commande `RetryFailedJobs` ou `OpenLogFolder` sans casser la structure existante.

## 8. Observer

But:
- propager les changements d'etat a plusieurs consommateurs sans couplage fort.

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

Pourquoi l'usage est defensable:
- le moteur publie `StateChanged`,
- le service relaye une copie defensive de l'etat vers les consommateurs,
- plusieurs observateurs reagissent chacun selon leur responsabilite:
  - affichage live,
  - dashboard,
  - notifications UI.

Gain de maintenabilite:
- ajout de nouveaux observateurs sans modifier la logique du moteur,
- meilleur decouplage entre execution et presentation,
- diffusion d'etat centralisee et testable.

Exemple de feature:
- brancher un observateur de telemetrie ou un tableau de bord distant.

## 9. Facade

But:
- exposer une API metier simple au-dessus d'une orchestration plus complexe.

Fichier et role:
- `src/EasySave.App/Services/BackupService.cs`

Pourquoi l'usage :
- `BackupService` presente des operations simples (`Run`, `Pause`, `Resume`, `Stop`, `CanStartSequence`),
- les consommateurs GUI/CLI n'ont pas a connaitre:
  - l'engine,
  - le snapshot global,
  - les verrous internes,
  - la logique de propagation d'etat,
  - les details de journalisation.

Ce que cela masque vraiment:
- lancement asynchrone,
- suivi d'execution global,
- synchronisation des etats,
- relai des evenements du moteur,
- orchestration avec `JobService`, `StateWriter` et la couche de log.

Gain de maintenabilite:
- point d'entree unique pour les clients de l'application,
- reduction des dependances directes vers les composants internes,
- orchestration metier plus simple a faire evoluer.

Exemple de feature:
- ajouter `RunBatch(IEnumerable<string> ids)` ou une politique de sequence sans exposer l'engine.

## 10. Singleton

But:
- partager une instance unique de localisation dans l'UI.

Fichier et role:
- `src/EasySave.App.Gui/Localization/Loc.cs`

Pourquoi l'usage :
- `Loc.Instance` fournit un point d'acces unique,
- la langue courante est partagee par les vues et view models,
- la classe publie un `PropertyChanged` qui permet de rafraichir les bindings.

Pourquoi ce singleton :
- la responsabilite est petite et transversale,
- l'objet represente un etat global de presentation,
- l'usage reste cantonne a la localisation GUI.

Gain de maintenabilite:
- source unique de verite pour la langue de l'interface,
- comportement coherent sur toutes les vues,
- cout de changement faible lors d'un switch de langue.

## 11. Repository

But:
- isoler la persistence JSON des regles metier.

Fichiers et role:
- `src/EasySave.Core/Interfaces/IJobRepository.cs`
- `src/EasySave.App/Repositories/JobRepository.cs`
- `src/EasySave.App/Repositories/AppConfigRepository.cs`
- `src/EasySave.App/Services/JobService.cs`
- `src/EasySave.App/Services/SettingsService.cs`

Pourquoi c'est utile:
- la logique metier manipule des operations de haut niveau (`GetAll`, `Add`, `Update`, `Remove`) au lieu de manipuler directement le systeme de fichiers,
- les details de serialisation, verrouillage et persistence sont concentres dans les repositories,
- un changement de stockage coute moins cher.

Precision:
- `JobRepository` illustre le mieux ce pattern,
- `AppConfigRepository` est surtout un composant de persistence de configuration.

## 12. MVVM (GUI Avalonia)

But:
- separer vue, etat de presentation et actions utilisateur.

Fichiers et role:
- `src/EasySave.App.Gui/ViewModels/ViewModelBase.cs`
- l'ensemble des `ViewModels` dans `src/EasySave.App.Gui/ViewModels`
- les vues Avalonia dans `src/EasySave.App.Gui/Views`

Pourquoi c'est utile:
- les vues consomment des proprietes bindables,
- les ViewModels portent les commandes et la logique de presentation,
- la maintenance de l'UI est plus propre qu'avec du code-behind metier.

## 13. Pattern MVC (du mode console)

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

## 14. Producer-Consumer

But:
- decoupler la reception reseau des ecritures disque dans LogHub.

Fichiers et role:
- producteur: `src/LogHub.Server/WebSockets/WebSocketEndpoint.cs`
- queue: `src/LogHub.Server/Infrastructure/Queueing/ChannelLogQueue.cs`
- consommateur: `src/LogHub.Server/Workers/LogIngestWorker.cs`
- stockage: `src/LogHub.Server/Infrastructure/Storage/DailyFileLogWriter.cs`

Pourquoi c'est utile:
- le endpoint WebSocket ne fait pas l'ecriture disque synchrone dans le chemin reseau,
- la file absorbe les pics de charge,
- le worker conserve un point unique de persistence.

## 15. Options Pattern (.NET)

But:
- centraliser et typer la configuration runtime du serveur LogHub.

Fichiers et role:
- `src/LogHub.Server/Options/LogHubOptions.cs`
- `src/LogHub.Server/Program.cs`
- consommateurs:
  - `src/LogHub.Server/Infrastructure/Queueing/ChannelLogQueue.cs`
  - `src/LogHub.Server/Infrastructure/Storage/DailyFileLogWriter.cs`
  - `src/LogHub.Server/WebSockets/WebSocketEndpoint.cs`

Pourquoi c'est utile:
- binding explicite de la configuration vers un type dedie,
- dependances plus lisibles que des lectures directes de configuration un peu partout,
- evolution plus propre des parametres serveur.

## 16. State

But:
- centraliser les transitions de statut d'un job.

Fichiers et role:
- `src/EasySave.Core/Enums/JobStatus.cs`
- `src/EasySave.App/Services/BackupEngine.cs`
- `src/EasySave.App/Services/JobExecutionControl.cs`

Pourquoi c'est utile:
- les transitions `Idle -> Running -> Paused -> Running -> Completed/Error` sont visibles et traceables,
- la logique `Pause/Resume/Stop` est concentree dans le moteur et le controle d'execution,
- le comportement reste previsible sans multiplier les etats caches.

## Preuve de maintenabilite (synthese)

Ce que ces patterns et choix d'architecture prouvent concretement:
- variation controlee: les strategies et la simple factory permettent d'ajouter des comportements sans reouvrir tout le code,
- couplage reduit: facade, observer, repository et MVVM separent mieux UI, metier, persistence et transport,
- code plus evolutif: builder et decorator evitent la duplication et limitent les modifications transversales,
- code plus robuste: `SafeLogger`, `NoEncryptionService` et `Producer-Consumer` reduisent l'impact des pannes locales,
- code plus testable: interfaces, commandes, evenements et responsabilites mieux decoupees facilitent les tests unitaires.

Conclusion:
- EasySave n'utilise pas "tous les patterns possibles",
- en revanche, les patterns retenus sont observes dans le code, utiles a la structure du projet, et directement relies a la maintenabilite.
