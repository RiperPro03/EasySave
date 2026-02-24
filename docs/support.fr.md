# Support EasySave v1.0 

## 1. Prérequis

- Windows
- .NET SDK 10.0
- Visual Studio 2026+ ou Rider
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
	
**Emplacement** : %APPDATA%\Roaming\ProSoft\EasySave\Logs

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

	- JSON			
	- XML

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

## Tests

Pour la partie tests, l'outil open source utilisé est **xUnit**.  
Les tests unitaires permettent de garantir la fiabilité du code et de limiter les régressions lors des évolutions futures.

Les tests couvrent :
- Validation des chemins source et cible
- Sélection des fichiers à copier 
- Logique de la sauvegarde complète 
- Logique de la sauvegarde différentielle
- Génération des fichiers JSON 


# Version 2.0

## 1. Nouveaux paramètres dans `setting.json`

- Extensions à chiffrer
- Clé de chiffrement
- Nom du logiciel métier

Ces paramètres sont configurables via **Settings**.

## 2. Chiffrement via CryptoSoft

**CryptoSoft** est un programme externe chargé du chiffrement des fichiers.

**Fonctionnement** :
```
EasySave  
-> lance CryptoSoft.exe  
-> lui passe le fichier et la clé  
-> récupère le code retour  
```
- Le **temps de chiffrement** est récupéré et ajouté aux logs (`encryptionTime`)
- `>0` → temps de chiffrement en ms
- 0 → pas de cryptage
- <0 → code erreur

**Remarque** : Seuls les fichiers dont l’extension et le logiciel métier correspondent aux valeurs définies par l’utilisateur dans les paramètres seront chiffrés.

## 3. Gestion du logiciel métier

L’utilisateur peut définir un **logiciel métier à surveiller**.

**Comportement** :

- Impossible de démarrer un job si le logiciel métier est actif  
- Une sauvegarde en cours s’arrête après le fichier courant
- Un log est généré indiquant l'arrêt

## 4. Autres évolutions v2.0

- Interface graphique (Avalonia)
- Nombre de jobs illimité
- Paramètres de cryptage et logiciel métier intégrés
- Logs enrichis avec le temps de chiffrement
