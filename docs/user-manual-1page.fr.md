# EasySave - Manuel Utilisateur (v3.0)

## 1. Présentation 

EasySave est un logiciel de sauvegarde développé en C# / .NET.
Il permet de créer et d'exécuter des travaux de sauvegarde selon deux types de sauvegarde : 
	
- **Sauvegarde complète** : copie l'ensemble des fichiers
- **Sauvegarde différentielle** : copie uniquement les fichiers modifiés depuis la dernière sauvegarde complète

L'application est multilingue (français et anglais) et propose une interface GUI (Avalonia), avec une CLI historique toujours disponible.

## 2. Prérequis 

	- Windows 
	- .NET SDK 10.0
	- Visual Studio 2026+ ou Rider
	- Git 

Vérifier l'installation : **dotnet --version**

## 3. Lancement de l'application 

Depuis la racine du projet (GUI recommandée) : **dotnet run --project src/EasySave.App.Gui**

Alternatives :

 - Lancer la version CLI : **dotnet run --project src/EasySave.App.Console**
 - Double-clic sur `EasySave.App.Gui.exe` (version publiée)
 - Ligne de commande avec arguments (ID des travaux de sauvegarde, CLI) :

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

Au lancement, l'écran suivant apparaît : 
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

- **Manage backup job** : créer, modifier, lister et supprimer des travaux de sauvegarde 
- **Run backup job** : exécuter des sauvegardes 
- **Settings** : accéder aux paramètres de l'application (langue, format de journalisation)
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
- **Create job** : créer un nouveau travail de sauvegarde
- **Update job** : modifier un travail de sauvegarde existant
- **Delete job** : supprimer un travail de sauvegarde
- **Back** : revenir au menu principal

Informations demandées lors de la création :  
ID, Nom, Chemin source, Chemin cible, Type de sauvegarde.

**Modification d'un job**

Champs modifiables : 

	1 - Name
	2 - Source path
	3 - Target path
	4 - Type 
	5 - Active / Inactive
	0 - Back

## 6. Exécution des sauvegardes

Menu :

	1 - Run one job  
	2 - Run all jobs  
	0 - Back

### Description

- **Run one job** : exécute un travail sélectionné par son ID  
- **Run all jobs** : exécute tous les travaux existants

## 7. Settings

Menu :

	1 - English
	2 - French
	3 - Log format: JSON
	4 - Log format: XML
	0 - Back

### Description

- **English** : basculer l'interface en anglais  
- **French** : basculer l'interface en français  
- **Log format: JSON** : configurer le format de journalisation en JSON  
- **Log format: XML** : configurer le format de journalisation en XML  
- **Back** : revenir au menu principal

## 8. Limitations version 1.0

	- Application console uniquement 
	- Maximum 5 jobs 
	- Exécution séquentielle 
	- Interface graphique prévue en v2

# Version 2.0 

## Nouveaux paramètres

- EncryptionEnabled
- EncryptionKey
- ExtensionsToEncrypt
- BusinessSoftwareProcessName

## Chiffrement 

Les fichiers dont l'extension et le logiciel métier correspondent aux valeurs définies par l’utilisateur dans les paramètres seront chiffrés.

## Logiciel métier 

L'utilisateur peut définir un logiciel métier à surveiller dans les paramètres généraux.  
Lorsque le logiciel métier est actif : 

- Impossible de lancer un job 
- Une sauvegarde en cours s'arrête proprement

# Version 3.0 

## 1. Sauvegarde en parallèle

EasySave 3.0 exécute désormais les sauvegardes en parallèle.

- Plusieurs travaux peuvent fonctionner en même temps
- Les performances sont améliorées
- Aucun réglage particulier n’est nécessaire : tout est automatique

## 2. Fichiers prioritaires

L’utilisateur peut définir une **liste d’extensions prioritaires** dans les paramètres.

**Règle** :  
Tant qu’un fichier prioritaire est en attente dans un travail, les fichiers non prioritaires ne sont pas traités.
Cela garantit que les fichiers importants passent en premier.

## 3. Limitation des fichiers volumineux

Un nouveau paramètre permet de définir un **seuil maximal (en Ko)**.

**Fonctionnement** :
- Deux fichiers plus gros que ce seuil ne peuvent pas être transférés en même temps
- Les fichiers plus petits continuent normalement
- Le seuil est modifiable dans les paramètres (Settings)

## 4. Contrôle en temps réel des travaux

Depuis l’interface graphique, l’utilisateur peut :

- **Mettre en pause** un travail (la pause prend effet après le fichier en cours)
- **Reprendre** un travail
- **Arrêter** un travail immédiatement
- **Suivre la progression** (pourcentage, état, fichier en cours)

Ces actions sont disponibles pour un travail individuel ou pour tous les travaux.

## 5. Pause automatique (logiciel métier)

Si un logiciel métier défini dans les paramètres est détecté :

- Tous les travaux passent automatiquement en pause
- Ils reprennent automatiquement lorsque le logiciel métier est fermé

Ce mécanisme évite que les sauvegardes perturbent les applications critiques.

## 6. CryptoSoft mono-instance

CryptoSoft ne peut plus être lancé plusieurs fois en même temps.

- EasySave attend automatiquement que CryptoSoft soit disponible
- Les éventuelles attentes sont visibles dans les logs

## 7 Centralisation des logs (Docker)

EasySave 3.0 permet d’envoyer les journaux de sauvegarde vers un serveur centralisé fonctionnant dans un conteneur Docker.
Cette fonctionnalité est utile lorsque plusieurs utilisateurs ou plusieurs machines utilisent EasySave dans une même entreprise.

### 7.1. Modes de stockage disponibles
L’utilisateur peut choisir entre trois modes :

- Local uniquement : Les logs sont enregistrés uniquement sur la machine de l’utilisateur.
- Centralisé uniquement : Les logs sont envoyés uniquement au serveur Docker.
- Local + centralisé : Les logs sont enregistrés localement et envoyés au serveur Docker.

### 7.2. Paramètres à renseigner
Si vous choisissez un mode impliquant le serveur (Centralisé ou Local + centralisé), au minimum ces informations doivent être renseignées :

- Host : l’adresse IP du serveur Docker
- Port : port configuré sur le serveur 

EasySave construit ensuite l’URL WebSocket avec le chemin par défaut : `/ws/logs`.

Ces paramètres sont accessibles dans le menu Settings.

### 7.3. Caractéristiques de la centralisation
- Un fichier journalier centralisé est généré sur le serveur (partagé par les utilisateurs connectés à la même instance LogHub).
- Chaque entrée de log contient l’identité de l’utilisateur et de la machine.
- Envoi des logs en temps réel via WebSocket
- Stockage persistant grâce au volume Docker (/app/logs).
- Les logs restent disponibles même après un redémarrage du conteneur.
- Endpoint WebSocket par défaut : `ws://<host>:<port>/ws/logs`

## 8. Nouveaux paramètres dans Settings

- Liste des extensions prioritaires
- Seuil maximal pour les fichiers volumineux
- Mode de centralisation des logs