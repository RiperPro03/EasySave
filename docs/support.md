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

- Un fichier de log par jour
- Format JSON ou XML selon configuration
- Écriture au fil de l’exécution (append)
- JSON lisible (indentation + retours à la ligne)
- Possibilité NDJSON (1 objet JSON par ligne)
- Aucun modèle métier imposé : EasyLog accepte n'importe quel objet T 


### 2.2 - Fichier état (state.json)

EasySave maintient un fichier unique state.json représentant l’état global de l’application en temps réel.

**Caractéristiques**:

- Snapshot global en temps réel de tous les jobs
- Mis à jour pendant l’exécution
- Emplacement : ...

**Contenu** : 

- Statut du job (Idle, Running, Paused, Completed, Error)
- Nombre total de fichiers et taille totale 
- Fichiers / taille déjà copiés
- Pourcentage d'avancement 
- Timestamp dernière action 
- Message d'erreur si nécessaire

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

- Vérifier que le SDK .NET 10 est installé 
- Vérifier la commande : **dotnet run --project src/EasySave.App.Console**
- 


## 5. Arborescence simplifié
```
src/
├── EasySave.App.Console      (interface console)
├── EasySave.App              (moteur, infrastructure)
├── EasySave.Core             (coeur métier)
├── EasySave.EasyLog          (DLL dédiée au logging)
├── EasySave.App.Gui          (interface graphique)
└── tests/
    └── EasySave.Tests		  (Tests Unitaires)
```