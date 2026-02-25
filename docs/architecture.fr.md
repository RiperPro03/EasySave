# Architecture du projet EasySave

## Objectif
Ce document decrit l architecture du projet **EasySave**, les responsabilites de chaque projet
et les flux principaux. Il vise a garder une vision claire et a jour du code reel :
- separation des responsabilites,
- testabilite,
- interfaces UI interchangeables,
- dependances explicites.

---

## Vue d ensemble
La solution est organisee en 5 projets et un programme externe pour le chiffrement:
```
EasySave.Core        -> domaine, DTO, contrats
EasySave.App         -> services et persistance
EasySave.EasyLog     -> bibliotheque de logging
EasySave.App.Console -> interface console
EasySave.App.Gui     -> interface graphique (Avalonia)
CryptoSoft           -> programme externe pour chiffrement
LogHub.Server        -> serveur Docker pour centralisation des logs (WebSocket)
```

---

## Dependances (etat actuel)
```
EasySave.App.Gui       -|
EasySave.App.Console    |-> EasySave.App -> EasySave.Core
                        |-> EasySave.Core

EasySave.App  ---------> EasySave.EasyLog
EasySave.Core ---------> EasySave.EasyLog (LogFormat)
```

Notes :
- `EasySave.Core` reference `EasySave.EasyLog.Options.LogFormat` via `AppConfig`.
- `EasySave.App.Console` instancie `AppConfigRepository` (App) pour charger la config.
- `EasySave.App.Gui` est connecté aux services de EasySave.App pour gérer les jobs illimités, les paramètres utilisateur (Settings) et l’exécution des sauvegardes via l’interface graphique.
- `CryptoSoft` est un programme externe; aucune dépendance directe avec EasySave.

---

## Description des projets

### 1. EasySave.Core
**Role :** domaine et contrats exposes aux couches superieures.

Contenu principal :
- **Modeles** : `BackupJob`, `AppConfig`.
- **DTO** : `BackupJobDto`, `BackupResultDto`, `JobStateDto`, `LogEntryDto`, `ResultDto`, `AppStateDto`.
- **Enums** : `BackupType`, `JobStatus`, `Language`.
- **Contrats** : `IBackupEngine`, `IBackupService`, `IJobService`, `IJobRepository`,
  `IBackupCopyStrategy`, `IPathProvider`, `IStateWriter`, `IFileSystem`.
- **Events** : `JobStateChangedEventArgs`.
- **Common** : `Guard`, `Localization`.

Contraintes :
- aucune dependance UI,
- pas de code d infrastructure,
- expose des interfaces testables.

---

### 2. EasySave.App
**Role :** implementations concretes de l'application.

**Services**
- `BackupEngine` : execution d un job, progression, logs.
- `BackupService` : orchestration + snapshot global `state.json`.
- `JobService` : operations CRUD sur les jobs.
- `PathProvider` : chemins applicatifs (AppData/ProSoft/EasySave).
- `StateWriter` : ecriture du snapshot global.
- `FullCopyStrategy` : copie totale.
- `DifferentialCopyStrategy` : copie si fichiers differents (taille, date, hash).
- `SettingsService` : Gestion de `setting.json` (langue, log format, extensions à chiffrer, clé, logiciel métier)
- `AppLogService` : Centralisation des logs
- `LogReaderService` : Lecture / parsing des logs
- `CryptoSoftProcessService` : Lancement de CryptoSoft pour chiffrer fichiers
- `NoEncryptionService` : Traitement des jobs sans chiffrement (fichiers non concernés par les règles de cryptage)
- `JobExecutionControl` : Contrôle exécution / Play / Pause / Stop
- `PriorityMonitor`: gestion des extensions prioritaires
- `LargeFileTransferLimiter`: gestion des fichiers volumineux

**Repositories**
- `JobRepository` : persistance dans `jobs.json` (limite a 5 jobs).
- `AppConfigRepository` : persistance `setting.json` (langue, format log).

**Utils :**
- `BusinessSoftwareDetector` : détection logiciel métier  
- `UncResolver` : gestion chemins UNC  

---

### 3. EasySave.EasyLog
**Role :** logging independant et reutilisable.

Composants :
- **Interfaces** : `ILogger<T>`, `ILogSerializer`, `ILogWriter`.
- **Loggers** : `DailyLogger<T>`, `SafeLogger<T>`.
- **Serialization** : JSON, XML.
- **Options** : `LogOptions`, `LogFormat`.
- **Factory** : `LoggerFactory`.
- **Utils** : `DailyFileHelper`.
- **Writers** : `FileLogWriter`

Caracteristiques :
- ecriture journaliere,
- formats JSON/XML,
- option `UseSafeLogger` pour absorber les exceptions,
- envoi temps réel via WebSocket.

---

### 4. EasySave.App.Console
**Role :** interface CLI.

Responsabilites :
- menus et navigation,
- lancement d un job ou d un batch,
- affichage des resultats.

Composants :
- **Controllers** : `MenuController`, `JobController`, `BackupController`, `SettingsController`.
- **Views** : `ConsoleView`, `JobView`, `BackupView`.
- **Input** : `ConsoleInput`, `ArgsParser`.
- **Bootstrap** : `Program`.

---

### 5. EasySave.App.Gui
**Role :** interface graphique Avalonia.

**Structure :**

- **ViewModels** : `MainWindowViewModel`, `DashboardViewModel`, `ExecutionViewModel`, `JobEditorViewModel`, `SettingsViewModel`  
- **Views** : `MainWindow.axaml`, `DashboardView.axaml`, `ExecutionView.axaml`, `JobEditorDialog.axaml`, `LogsView.axaml`, `SettingsView.axaml`, `AboutView.axaml`  
- **Models** : `ExecutionJobItem.cs`, `LogEntryItem.cs`, `RecentActivityItem.cs`  
- **Converters** : `LogLevelToBrushConverter.cs`, `StatusToTextConverter.cs`, `TypeToTextConverter.cs`  
- **Assets** : logo, icônes  
- **App.axaml** + **Program.cs** + **ViewLocator.cs** 
- Intègre l’appel à CryptoSoft, gestion jobs illimités, logiciels métier et extensions à chiffrer  

---

### 6. CryptoSoft
**Rôle :** Programme externe chargé de chiffrer des fichiers via XOR.

**Fichiers :**

- **CryptoSoft.csproj** 
- **Program.cs**: exécution CLI  
- **FileManager.cs** : lecture, chiffrement, mesure temps (ms)  

**Fonctionnement :**
- EasySave.App lance CryptoSoft.exe via un process externe
- Passe en argument le fichier et la clé
- Récupère le code retour (temps de chiffrement en ms)
- Temps retourné dans les logs :  
  - 0 si pas de chiffrement  
  - `>0` temps de chiffrement en ms  
  - <0 erreur de chiffrement  
---

### 7. LogHub.Server (Docker)
**Rôle :**
- serveur WebSocket recevant les logs EasySave,
- stockage des logs dans un volume Docker persistant,
- un fichier journalier unique pour tous les utilisateurs.

**Caractéristiques :**
- WebSocket : `ws://<host>:<port>/ws/logs`
- Volume Docker : `/app/logs`
- Persistance : `-v ~/loghub-logs:/app/logs`
- Redémarrage automatique : `--restart unless-stopped`
---
## Flux principaux

### Lancement de l application (Console)
```
Program
  -> PathProvider
  -> AppConfigRepository.Load()
  -> JobService
  -> BackupService (logDirectory + logFormat)
  -> Controllers + Views
```

### Execution d un job
```
UI (Console)
  -> BackupService.Run(job)
     -> BackupEngine.Run(job)
        -> Strategie (Full/Differential)
        -> System.IO (enumeration + copie)
        -> EasyLog (resume)
     -> JobService.MarkExecuted(job.Id)
  -> Affichage resultat
```

### Snapshot d etat global (state.json)
```
BackupEngine (event StateChanged)
  -> BackupService
     -> StateWriter.Write(AppStateDto)
```

---

## Donnees persistantes

Repertoire racine (via `PathProvider`) :
```
%APPDATA%\ProSoft\EasySave
  Logs\        (daily logs)
  State\       (state.json)
  Config\      (jobs.json, setting.json)
```

Fichiers :
- `jobs.json` : liste de jobs (`BackupJobDto`), limite 5 jobs.
- `state.json` : snapshot global (`AppStateDto`).
- `setting.json` : configuration (`Language`, `LogFormat`).

---

## Observabilite de l'etat

Le suivi temps reel repose sur :
- `IBackupEngine.StateChanged`,
- relaye par `BackupService`,
- agrege en `AppStateDto` via `StateWriter`.

Structure logique :
```
AppStateDto
  - GeneratedAtUtc
  - TotalJobs
  - GlobalStatus
  - ActiveJobIds
  - Jobs[] (JobStateDto)
```

---

## Tests

Tests existants :
- `EasySave.Tests` couvre Core, App, EasyLog, Console.
- tests unitaires sur `BackupEngine`, `BackupService`, `JobRepository`,
  `JobService`, `PathProvider`, `StateWriter`, `ArgsParser`.

---

## Diagrammes UML

Les diagrammes a jour sont dans `docs/uml` :
- `docs/uml/Core/DiagrammeClass_Core.puml`
- `docs/uml/App/DiagrammeClass_App.puml`
- `docs/uml/Console/DiagrammeClass_Console.puml`
- `docs/uml/DiagrammeSequence_RunJob_General.puml`
- `docs/uml/DiagrammeSequence_RunBatch_Args.puml`
- `docs/uml/DiagrammeSequence_Backup_Differential.puml`
- `docs/uml/DiagrammeUseCase_General.puml`
- `docs/uml/DiagrammeActivite_General.mmd`

---

## Points d attention

- Extensions prioritaires
- Seuil fichiers volumineux
- Pause automatique si logiciel métier actif
- Chiffrement CryptoSoft mono-instance
- Centralisation des logs via WebSocket
- Host/Port choisis par l’entreprise
