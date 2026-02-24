# EasySave v1.0 support 

## 1. Prerequisites

- Windows
- .NET SDK 10.0
- Visual Studio 2026+ or Rider
- Git

Check installation: **dotnet --version**

## 2. File location 

### 2.1 - Log files 

EasySave uses the EasyLog.dll library to generate logs.

**Features** : 

- One log file per day (yyyy-MM-dd.json or .xml)
- JSON or XML format depending on configuration
- Real-time writing (append) at runtime
- Readable JSON (indentation + line breaks)
- NDJSON capability (1 JSON object per line)
- No imposed business model: EasyLog accepts any T 
	
**Emplacement** : %APPDATA%\Roaming\ProSoft\EasySave\Logs

### 2.2 - State file (state.json)

EasySave maintains a single file representing the global state of all jobs in real time.

**Features**:

- Global snapshot updated at runtime
- Contains the state of each job as well as the global state of the application.

**Emplacement** : \%APPDATA%\Roaming\ProSoft\EasySave\State

**Content** : 

- Job status (Idle, Running, Paused, Completed, Error)
- Total number of files and total size 
- Files / size already copied
- Percentage of completion 
- Timestamp last action 
- Error message if necessary

### 2.3 - Settings files

EasySave uses a settings file to store application parameters.

**Emplacement** : %APPDATA%\Roaming\ProSoft\EasySave\Config

Contains persistent parameters such as :
- Interface language (English / French)
- Logging format (JSON / XML)

Modifications are saved automatically when changed via the Settings menu.

## 3. Application configuration

Settings can be accessed via the menu : **Settings

Options available:

- Choice of language :

	- English
	- French

- Logging format :
	- JSON 
	- XML

Changes are saved automatically.

## 4. Support

For technical support, please check :

- .NET 10 SDK is installed
- the following command is running: **dotnet run --project src/EasySave.App.Console**

In the event of a problem, check the locations of the logs and state.json files.

## 5. Simplified tree
```
src/
├── EasySave.App.Console      (console interface v1)
├── EasySave.App              (engine, infrastructure)
├── EasySave.Core             (core business)
├── EasySave.EasyLog          (DLL dedicated to logging)
├── EasySave.App.Gui          (graphical user interface v2)
└── tests/
    └── EasySave.Tests		  (Unit Tests)
```
## 6. Version 1.0 limitations

- Console application only
- Maximum 5 jobs
- Sequential execution only
- Graphical interface planned for version 2.0

## Tests

The open source tool used for testing is **xUnit**.  
Unit tests guarantee the reliability of the code and limit regressions during future evolutions.

The tests cover :
	- Validation of source and target paths
	- Selection of files to be copied 
	- Complete backup logic 
	- Differential backup logic
	- JSON file generation 

# Version 2.0

## 1. New parameters in `setting.json`.

- Extensions to encrypt
- Encryption key
- Name of business software

These parameters are configurable via **Settings**.

## 2. Encryption via CryptoSoft

**CryptoSoft** is an external program for encrypting files.

**How it works**:
```
 EasySave  
-> launches CryptoSoft.exe  
-> passes it the file and key  
-> retrieves the return code 
```
- The **encryption time** is retrieved and added to the logs (`encryptionTime`)
- `>0` → encryption time in ms
- 0 → no encryption
- <0 → error code

**Note**: Only files whose extension and business software correspond to the values defined by the user in the parameters will be encrypted.

## 3. Business software management

The user can define a **business software to be monitored**.

**Behavior** :

- Impossible to start a job if the business software is active.  
- A backup in progress stops after the current file.
- A log is generated indicating the stop

## 4. Other v2.0 evolutions

- Graphical interface (Avalonia)
- Unlimited number of jobs
- Integrated encryption parameters and business software
- Logs enriched with encryption time

# Version 3.0

## 1. General new features

EasySave version 3.0 introduces several major evolutions designed to improve performance, priority management and user experience.
This version marks a major departure from previous versions, with the introduction of parallel mode, advanced file management and centralized logging.

## 2. Parallel backup

EasySave 3.0 abandons sequential mode for parallel operation: 
- multiple jobs can run simultaneously
- Each job can process several files in parallel

## 3. Priority file management

Users can now define a **priority extension list** in the parameters.
As long as a priority file is pending in at least one job, no non-priority files can be transferred.

## 4. Limit simultaneous transfers for large files

To avoid network saturation, EasySave 3.0 introduces a user-configurable **maximum threshold (n KB)**.

**Rule** :
- Two files **above the threshold** cannot be transferred at the same time
- While a large file is being transferred, other jobs can continue to transfer smaller files (if priority rules allow)
This threshold can be configured in the general parameters.

## 5. real-time interaction with jobs

The user can now control each job individually or all jobs together:
- **Pause**
- **Resume**
- **Immediate stop** Real-time monitoring
- **Real-time monitoring** (progress, status, file in progress, etc.)
This feature improves control and visibility of operations.

## 6. Automatic pause on active business software

If user-defined business software is detected :
- All jobs are automatically paused
- They resume automatically when the business software is closed
This mechanism ensures that backups do not disrupt critical applications.

## 7. CryptoSoft Mono-Instance

CryptoSoft is now **mono-instance**:
- Impossible to run multiple instances simultaneously

## 8. Centralization of daily logs (Docker)