# EasySave - Manuel Utilisateur (v1.0)

## 1. Pr�sentation 

EasySave est un logiciel de sauvegarde de donn�es en ligne de commande d�velopp� en C# / .NET.
Il permet de cr�er et d'ex�cuter des travaux de sauvegarde selon deux types de sauvegarde : 
	
- **Sauvegarde compl�te** : copie l'ensemble des fichiers
- **Sauvegarde diff�rentielle** : copie uniquement les fichiers modifi�s depuis la derni�re sauvegarde compl�te

L'application est multilingue (fran�ais et anglais) et offre une interface utilisateur simple et intuitive.

## 2. Pr�requis 

	- Windows 
	- .NET SDK 10.0
	- Visual Studio 2022+ ou Rider
	- Git 

V�rifier l'installation : **dotnet --version**

## 3. Lancement de l'application 

Depuis la racine du projet : **dotnet run --project src/EasySave.App.Console**

Alternatives :

 - Double-clic sur EasySave.App.Console.exe
 - Ligne de commande avec arguments (ID des travaux de sauvegarde) :

```bash
 .\EasySave.App.Console.exe 1
```

```bash
 .\EasySave.App.Console.exe '1;3'
```

```bash
 .\EasySave.App.Console.exe 1-2
```

## 4. Menu principal

Au lancement, l'�cran suivant appara�t : 
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

- **Manage backup job** : cr�er, modifier, lister et supprimer des travaux de sauvegarde 
- **Run backup job** : ex�cuter des sauvegardes 
- **Settings** : acc�der aux param�tres de l'application (langue, format de journalisation)
- **Exit** : quitter l'application

## 5. Gestion des travaux de sauvegarde

Menu : 
- **1** - List jobs
- **2** - Create job
- **3** - Update job
- **4** - Delete job
- **0** - Back

### Description

- **List jobs** : afficher tous les jobs existants
- **Create job** : cr�er un nouveau travail de sauvegarde
- **Update job** : modifier un travail de sauvegarde existant
- **Delete job** : supprimer un travail de sauvegarde
- **Back** : revenir au menu principal

Informations demand�es lors de la cr�ation :  
ID, Nom, Chemin source, Chemin cible, Type de sauvegarde.

**Modification d'un job**

Champs modifiables : 

	1 - Name
	2 - Source path
	3 - Target path
	4 - Type 
	5 - Active / Inactive
	0 - Back

## 6. Ex�cution des sauvegardes

Menu :

	1 - Run one job  
	2 - Run all jobs  
	0 - Back

### Description

- **Run one job** : ex�cute un travail s�lectionn� par son ID  
- **Run all jobs** : ex�cute tous les travaux existants

## 7. Settings

Menu :
	1 - English
	2 - French
	3 - Log format: JSON
	4 - Log format: XML
	0 - Back

### Description

- **English** : basculer l'interface en anglais  
- **French** : basculer l'interface en fran�ais  
- **Log format: JSON** : configurer le format de journalisation en JSON  
- **Log format: XML** : configurer le format de journalisation en XML  
- **Back** : revenir au menu principal

## 8. Limitations version 1.0

	- Application console uniquement 
	- Maximum 5 jobs 
	- Ex�cution s�quentielle 
	- Interface graphique pr�vue en v2