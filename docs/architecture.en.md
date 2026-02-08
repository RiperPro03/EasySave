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
The solution is organized into 5 projects:
```
EasySave.Core        -> domain, DTO, contracts
EasySave.App         -> services and persistence
EasySave.EasyLog     -> logging library
EasySave.App.Console -> console interface
EasySave.App.Gui     -> graphical interface (Avalonia)
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
- `EasySave.App.Gui` is an Avalonia skeleton (no business logic connected).

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

**Strategies**
- `FullCopyStrategy`: full copy.
- `DifferentialCopyStrategy`: copy if different files (size, date, hash).

**Repositories**
- `JobRepository`: persistence in `jobs.json` (limit to 5 jobs).
- `AppConfigRepository` : persistence in `setting.json` (language, log format).

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

Current state:
- `MainWindow` + minimal `MainWindowViewModel`.
- no connection to `EasySave.App` services for now.

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

- `EasySave.App.Gui` is not yet connected to the app services.