# EasySave project architecture

## Objective
This document describes the architecture of the **EasySave** project, the responsibilities of each project
and the main flows. It aims to provide a clear and up-to-date view of the actual code:
- separation of responsibilities,
- testability,
- interchangeable UI interfaces,
- explicit dependencies.

---

## Overview
The solution is organized into 5 projects and an external program for encryption:
```
EasySave.Core        -> domain, DTO, contracts
EasySave.App         -> services and persistence
EasySave.EasyLog     -> logging library
EasySave.App.Console -> console interface
EasySave.App.Gui     -> graphical interface (Avalonia)
CryptoSoft           -> external program for encryption
```
---

## Dependencies (current state)
```
EasySave.App.Gui       -|
EasySave.App.Console    |-> EasySave.App -> EasySave.Core
                        |-> EasySave.Core

EasySave.App  ---------> EasySave.EasyLog
EasySave.Core ---------> EasySave.EasyLog (LogFormat)
```

Notes :
- `EasySave.Core` reference `EasySave.EasyLog.Options.LogFormat` via `AppConfig`.
- `EasySave.App.Console` instantiates `AppConfigRepository` (App) to load config.
- `EasySave.App.Gui` is connected to EasySave.App services to manage unlimited jobs, user parameters (Settings) and backup execution via the graphical interface.
- `CryptoSoft` is an external program; no direct dependency on EasySave.

---

## Project description

### 1 EasySave.Core
**Role:** domain and contracts exposed to upper layers.

Main content:
- **Models**: `BackupJob`, `AppConfig`.
- **DTO**: `BackupJobDto`, `BackupResultDto`, `JobStateDto`, `LogEntryDto`, `ResultDto`, `AppStateDto`.
- **Enums**: `BackupType`, `JobStatus`, `Language`.
- **Contracts**: `IBackupEngine`, `IBackupService`, `IJobService`, `IJobRepository`,
 `IBackupCopyStrategy`, `IPathProvider`, `IStateWriter`, `IFileSystem`.
- **Events** : `JobStateChangedEventArgs`.
- **Commons**: `Guard`, `Localization`.

Constraints :
- no UI dependency,
- no infrastructure code,
- exposes testable interfaces.

---

### 2. EasySave.App
**Role:** concrete application implementations.

**Services**
- `BackupEngine` : job execution, progress, logs.
- `BackupService` : orchestration + global snapshot `state.json`.
- `JobService`: CRUD operations on jobs.
- `PathProvider`: application paths (AppData/ProSoft/EasySave).
- `StateWriter`: global snapshot writing.
- `FullCopyStrategy`: full copy.
- `DifferentialCopyStrategy`: copy if different files (size, date, hash).
- `SettingsService`: Management of `setting.json` (language, log format, extensions to be encrypted, key, business software)
- `AppLogService` : Log centralization
- `LogReaderService` : Reading / parsing of logs
- `CryptoSoftProcessService` : Launch CryptoSoft to encrypt files
- `NoEncryptionService`: Job processing without encryption (files not affected by encryption rules)
- `JobExecutionControl`: Execution control / Play / Pause / Stop

**Repositories**
- `JobRepository`: persistence in `jobs.json` (limit to 5 jobs).
- `AppConfigRepository` : persistence in `setting.json` (language, log format).

**Tools :**
- `BusinessSoftwareDetector` : business software detection 
- `UncResolver` : UNC path management
---

### 3. EasySave.EasyLog
**Role:** logging independent and reusable.

Components:
- **Interfaces** : `ILogger<T>`, `ILogSerializer`, `ILogWriter`.
- **Loggers**: `DailyLogger<T>`, `SafeLogger<T>`.
- **Serialization**: JSON, XML.
- **Options**: `LogOptions`, `LogFormat`.
- **Factory**: `LoggerFactory`.
- **Tools**: `DailyFileHelper`.
- **Writers** : `FileLogWriter`

Features :
- daily writing,
- JSON/XML formats,
- `UseSafeLogger` option to absorb exceptions.

---

### 4 EasySave.App.Console
**Role:** CLI interface.

Responsibilities :
- menus and navigation,
- launch job or batch,
- display results.

Components:
- **Controllers** : `MenuController`, `JobController`, `BackupController`, `SettingsController`.
- **Views**: `ConsoleView`, `JobView`, `BackupView`.
- **Input**: `ConsoleInput`, `ArgsParser`.
- **Bootstrap**: `Program`.

---

### 5. EasySave.App.Gui
**Role:** Avalonia graphical interface.

**Structure:**

- **ViewModels**: `MainWindowViewModel`, `DashboardViewModel`, `ExecutionViewModel`, `JobEditorViewModel`, `SettingsViewModel`.  
- **Views**: `MainWindow.axaml`, `DashboardView.axaml`, `ExecutionView.axaml`, `JobEditorDialog.axaml`, `LogsView.axaml`, `SettingsView.axaml`, `AboutView.axaml`  
- **Models**: `ExecutionJobItem.cs`, `LogEntryItem.cs`, `RecentActivityItem.cs`.  
- **Converters**: `LogLevelToBrushConverter.cs`, `StatusToTextConverter.cs`, `TypeToTextConverter.cs`.  
- **Assets**: logo, icons  
- **App.axaml** + **Program.cs** + **ViewLocator.cs** 
- Includes CryptoSoft call, unlimited job management, business software and encrypted extensions  

---

### 6. CryptoSoft
**Role:** External program for encrypting files via XOR.

**Files:**

- **CryptoSoft.csproj** 
- **Program.cs**: CLI execution  
- **FileManager.cs**: read, encrypt, measure time (ms)  

**Operation:**
- EasySave.App launches CryptoSoft.exe via an external process
- Passes file and key as arguments
- Retrieves return code (encryption time in ms)
- Time returned in logs :  
  - 0 if no encryption  
  - `>0` encryption time in ms  
  - <0 encryption error  
---

## Main flows

### Launch application (Console)
```
Program
  -> PathProvider
  -> AppConfigRepository.Load()
  -> JobService
  -> BackupService (logDirectory + logFormat)
  -> Controllers + Views
```
### Run a job
```
 UI (Console)
  -> BackupService.Run(job)
     -> BackupEngine.Run(job)
        -> Strategy (Full/Differential)
        -> System.IO (enumeration + copy)
        -> EasyLog (resume)
     -> JobService.MarkExecuted(job.Id)
  -> Display result
```

### Snapshot of global state (state.json)
```
BackupEngine (event StateChanged)
  -> BackupService
     -> StateWriter.Write(AppStateDto)
```

---

## Persistent data

Root directory (via `PathProvider`):
```
%APPDATA%\ProSoft\EasySave
 Logs\ (daily logs)
 State\ (state.json)
 Config\ (jobs.json, setting.json)
```

Files :
- `jobs.json`: job list (`BackupJobDto`), limit 5 jobs.
- `state.json`: global snapshot (`AppStateDto`).
- `setting.json`: configuration (`Language`, `LogFormat`).

---

## Status observability

Real-time monitoring is based on :
- `IBackupEngine.StateChanged`,
- relayed by `BackupService`,
- aggregated into `AppStateDto` via `StateWriter`.

Logical structure:
```
 AppStateDto
  - GeneratedAtUtc
  - TotalJobs
  - GlobalStatus
  - ActiveJobIds
  - Jobs[] (JobStateDto)
````

---

## Tests

Existing tests:
- `EasySave.Tests` covers Core, App, EasyLog, Console.
- unit tests on `BackupEngine`, `BackupService`, `JobRepository`,
 `JobService`, `PathProvider`, `StateWriter`, `ArgsParser`.

---

## UML diagrams

The latest diagrams are in `docs/uml` :
- `docs/uml/Core/DiagrammeClass_Core.puml`
- `docs/uml/App/DiagrammeClass_App.puml`
- `docs/uml/Console/DiagrammeClass_Console.puml`
- `docs/uml/DiagrammeSequence_RunJob_General.puml`
- `docs/uml/DiagrammeSequence_RunBatch_Args.puml`
- `docs/uml/DiagrammeSequence_Backup_Differential.puml`
- `docs/uml/DiagrammeUseCase_General.puml`
- `docs/uml/DiagrammeActivite_General.mmd`

---

## Points of attention

- Unlimited job management  
- Automatic file encryption via CryptoSoft
- Job detection and blocking if business software active (configurable)
- GUI now fully connected to App services for execution and parameterization
- Enhanced logging, with integrated encryption time