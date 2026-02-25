# EasySave - User Manual (v3.0)

## 1. Overview 

EasySave is a backup software developed in C# / .NET.
It allows you to create and run backup jobs according to two backup types: 
	
- **Full backup**: copies all files
- **Differential backup**: copies only files modified since the last full backup.

The application is multilingual (French and English) and provides a GUI (Avalonia) with a legacy CLI still available.

## 2. Prerequisites 

	- Windows 
	- .NET SDK 10.0
	- Visual Studio 2026+ or Rider
	- Git 

Check installation: **dotnet --version**

## 3. Launch application 

From project root (recommended GUI): **dotnet run --project src/EasySave.App.Gui**

Alternatives :

 - Run the CLI version: **dotnet run --project src/EasySave.App.Console**
 - Double-click on `EasySave.App.Gui.exe` (published build)
 - Command line with arguments (backup job IDs, CLI):

```bash
 .\EasySave.App.Console.exe 1
```

```bash
 .\EasySave.App.Console.exe '1;3'
```

```bash
 .\EasySave.App.Console.exe 1-2
```

## 4. Main menu

On launch, the following screen appears:
```
=================================
 EasySave - Backup Software
=================================

Main menu
1 - Manage backup job
2 - Run backup job
3 - Settings
0 - Exit
```

### Description 

- **Manage backup job**: create, modify, list and delete backup jobs 
- **Run backup job**: run backup jobs 
- **Settings**: access application settings (language, logging format)
- **Exit**: quit the application

## 5. Backup job management

Menu : 
- **1** - List jobs
- **2** - Create job
- **3** - Update job
- **4** - Delete job
- **0** - Back

### Description

- **List jobs**: display all existing jobs
- **Create job**: create a new backup job
- **Update job**: modify an existing backup job
- **Delete job**: delete a backup job
- **Back**: return to main menu

Information requested on creation: 
ID, Name, Source path, Target path, Backup type.

**Modifying a job**

Modifiable fields : 

	1 - Name
	2 - Source path
	3 - Target path
	4 - Type 
	5 - Active / Inactive
	0 - Back

## 6. Running backups

Menu :

	1 - Run one job 
	2 - Run all jobs 
	0 - Back

### Description

- **Run one job**: runs a job selected by its ID  
- **Run all jobs**: runs all existing jobs

## 7. Settings

Menu :

	1 - English
	2 - French
	3 - Log format: JSON
	4 - Log format: XML
	0 - Back

### Description

- **English**: switch interface to English  
- **French**: toggle French interface  
- **Log format: JSON**: set logging format to JSON  
- **Log format: XML**: set logging format to XML  
- **Back**: return to main menu

## 8. Version 1.0 limitations

	- Console application only 
	- Maximum 5 jobs 
	- Sequential execution 
	- Graphical interface planned for v2 

# Version 2.0 

## New parameters

- EncryptionEnabled
- EncryptionKey
- ExtensionsToEncrypt
- BusinessSoftwareProcessName

## Encryption 

Files whose extension and business software correspond to the values defined by the user in the parameters will be encrypted.

## Business software 

When business software is active : 

- Unable to start a job 
- A backup in progress stops cleanly

# Version 3.0 

## 1. Parallel backup

EasySave 3.0 now runs backups in parallel.

- Multiple jobs can run at the same time
- Improved performance
- No special settings required: everything is automatic

## 2. Priority files

The user can define a **priority extensions list** in the parameters.

**Rule**: 
As long as a priority file is pending in a job, non-priority files are not processed.
This ensures that important files come first.

## 3. Limiting large files

A new parameter lets you define a **maximum threshold (in Kb)**.

**How it works** :
- Two files larger than this threshold cannot be transferred at the same time.
- Smaller files continue as normal
- The threshold can be modified in Settings.

## 4. Real-time job control

From the graphical interface, the user can:

- **Pause** a job (the pause takes effect after the current file)
- **Resume** a job
- **Stop** a job immediately
- **Monitor progress** (percentage, status, current file)

These actions are available for an individual job or for all jobs.

## 5. Automatic pause (job software)

If a job software defined in the parameters is detected :

- All jobs are automatically paused
- They resume automatically when the business software is closed.

This mechanism prevents backups from disrupting critical applications.

## 6. CryptoSoft mono-instance

CryptoSoft can no longer be launched several times at the same time.

- EasySave automatically waits until CryptoSoft is available.
- Any waits are visible in the logs

## 7 Log centralization (Docker)

EasySave 3.0 enables backup logs to be sent to a centralized server running in a Docker container.
This feature is useful when several users or machines are using EasySave in the same company.

### 7.1. Available storage modes
Users can choose between three modes:
- **Local only** : Logs are stored only on the user's machine.
- **Centralized only** : Logs are sent only to the Docker server.
- **Local + centralized** : logs are saved locally and sent to the Docker server.

### 7.2. Parameters to be entered
If you choose a mode involving the server (Centralized or Local + Centralized), at least these values must be entered:

- Host: IP address of the Docker server
- Port: port configured on the server 

EasySave then builds the WebSocket endpoint using the default path: `/ws/logs`.

These parameters can be accessed via the Settings menu.

### 7.3. Centralization features
- A centralized daily log file is generated on the server (shared by users connected to the same LogHub instance).
- Each log entry contains the identity of the user and the machine.
- Sending logs in real time via WebSocket.
- Persistent storage thanks to the Docker volume (/app/logs).
- Logs remain available even after a container restart.
- Default WebSocket endpoint: `ws://<host>:<port>/ws/logs`

## 8. New settings in Settings

- List of priority extensions
- Maximum threshold for large files
- Log centralization mode