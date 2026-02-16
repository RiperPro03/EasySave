# EasySave - User Manual (v1.0)

## 1. Overview 

EasySave is a command-line data backup software developed in C# / .NET.
It allows you to create and run backup jobs according to two backup types: 
	
- **Full backup**: copies all files
- **Differential backup**: copies only files modified since the last full backup.

The application is multilingual (French and English) and offers a simple, intuitive user interface.

## 2. Prerequisites 

	- Windows 
	- .NET SDK 10.0
	- Visual Studio 2026+ or Rider
	- Git 

Check installation: **dotnet --version**

## 3. Launch application 

From project root: **dotnet run --project src/EasySave.App.Console**

Alternatives :

 - Double-click on EasySave.App.Console.exe
 - Command line with arguments (backup job IDs):

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

## Additional settings

- EncryptionEnabled
- EncryptionKey
- ExtensionsToEncrypt
- BusinessSoftwareProcessName

## Encryption 

Files with a matching extension are automatically encrypted. 

## Business software 

When business software is active : 

- Unable to start a job 
- A backup in progress stops cleanly