# Support EasySave v1.0 

## 1. Prérequis

- Windows
- .NET SDK 10.0
- Visual Studio 2022+ ou Rider
- Git

Vérifier l'installation : **dotnet --version**

## 2. Emplacement des fichiers 

### 2.1 - Journaux logs 

EasySave utilise la bibliothèque EasyLog.dll pour générer des logs.

**Caractéristiques** : 

- Un fichier de log par jour (yyyy-MM-dd.json ou .xml)
- Format JSON ou XML selon configuration
- Écriture en temps réel (append) au fil de l’exécution
- JSON lisible (indentation + retours à la ligne)
- Possibilité NDJSON (1 objet JSON par ligne)
- Aucun modèle métier imposé : EasyLog accepte n'importe quel objet T 
	
**Emplacement** : %APPDATA%\Roaming\ProSoft\EasySave\src\EasySave.EasyLog

### 2.2 - Fichier état (state.json)

EasySave maintient un fichier unique représentant l’état global de tous les jobs en temps réel.

**Caractéristiques**:

- Snapshot global mis à jour pendant l’exécution
- Contient l’état de chaque job ainsi que l’état global de l’application

**Emplacement** : \%APPDATA%\Roaming\ProSoft\EasySave\State

**Contenu** : 

- Statut du job (Idle, Running, Paused, Completed, Error)
- Nombre total de fichiers et taille totale 
- Fichiers / taille déjà copiés
- Pourcentage d'avancement 
- Timestamp dernière action 
- Message d'erreur si nécessaire

### 2.3 - Fichiers de configuration (Settings)

EasySave utilise un fichier de configuration pour stocker les paramètres de l'application.

**Emplacement** : %APPDATA%\Roaming\ProSoft\EasySave\Config

Contient les paramètres persistants tels que :
- Langue de l’interface (English / French)
- Format de journalisation (JSON / XML)

Les modifications sont sauvegardées automatiquement lors du changement via le menu Settings.

## 3. Configuration de l'application

Les paramètres sont accessibles via le menu : **Settings**

Options disponibles :

- Choix de la langue :

	- English
	- French

- Format de journalisation :
	- JSON				- XML

Les modifications sont sauvegardées automatiquement.

## 4. Support

Pour le support technique, vérifier :

- Que le SDK .NET 10 est installé
- Que la commande suivante fonctionne : **dotnet run --project src/EasySave.App.Console**

En cas de problème, vérifier les emplacements des fichiers logs et state.json.

## 5. Arborescence simplifié
```
src/
├── EasySave.App.Console      (interface console v1)
├── EasySave.App              (moteur, infrastructure)
├── EasySave.Core             (coeur métier)
├── EasySave.EasyLog          (DLL dédiée au logging)
├── EasySave.App.Gui          (interface graphique v2)
└── tests/
    └── EasySave.Tests		  (Tests Unitaires)
```

## 6. Limitations de la version 1.0

- Application console uniquement
- Maximum 5 jobs
- Exécution séquentielle uniquement
- Interface graphique prévue pour la version 2.0