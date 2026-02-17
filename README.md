<div align="center">
  
<img width="250" height="250" alt="Gemini_Generated_Image_qx5t7xqx5t7xqx5t" src="https://github.com/user-attachments/assets/7ce9f6c2-0094-46b7-a06d-4bd3b8745394" />

# EasySave

### Logiciel de sauvegarde professionnel développé en C# / .NET

[![C#](https://img.shields.io/badge/Language-C%23-purple.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![.NET 10](https://img.shields.io/badge/Framework-.NET%2010-darkblue.svg)](https://learn.microsoft.com/fr-fr/dotnet/core/whats-new/dotnet-10/overview)
[![SonarQube](https://img.shields.io/badge/Code%20Quality-SonarQube-4E9BCD.svg)](https://www.sonarsource.com/)
[![Avalonia](https://img.shields.io/badge/UI%20Framework-Avalonia-blue.svg)](https://avaloniaui.net/)

</div>

---

## Présentation

**EasySave** est un logiciel de sauvegarde développé en C# / .NET dans le cadre du projet fil rouge **ProSoft** (CESI).

Le projet est conçu pour évoluer par versions successives (v1 → v3) en respectant les principes de :
- Qualité logicielle
- Maintenabilité
- Architecture propre

---

## Documentation

### Manuels Utilisateur


<table>
  <tr>
    <td align="center" width="50%">
      <img src="https://img.shields.io/badge/Langue-Français-blue" alt="Français"><br>
      <a href="docs/user-manual-1page.fr.md">Manuel Utilisateur</a>
    </td>
    <td align="center" width="50%">
      <img src="https://img.shields.io/badge/Language-English-red" alt="English"><br>
      <a href="docs/user-manual-1page.en.md">User Manual</a>
    </td>
  </tr>
</table>

### Support Technique

<table>
  <tr>
    <td align="center" width="50%">
      <img src="https://img.shields.io/badge/Langue-Français-blue" alt="Français"><br>
      <a href="docs/support.fr.md">Guide de Support</a>
    </td>
    <td align="center" width="50%">
      <img src="https://img.shields.io/badge/Language-English-red" alt="English"><br>
      <a href="docs/support.en.md">Support Guide</a>
    </td>
  </tr>
</table>

### Documentation Technique

<table>
  <tr>
    <td align="center" width="50%">
      <img src="https://img.shields.io/badge/Langue-Français-blue" alt="Français"><br>
      <a href="docs/architecture.fr.md">Architecture</a>
    </td>
    <td align="center" width="50%">
      <img src="https://img.shields.io/badge/Language-English-red" alt="English"><br>
      <a href="docs/architecture.en.md">Architecture</a>
    </td>
  </tr>
</table>

---

## Fonctionnalités (Version 1.0)

<table>
  <tr>
    <td><strong>Interface</strong></td>
    <td>Application console en .NET</td>
  </tr>
  <tr>
    <td><strong>Travaux de sauvegarde</strong></td>
    <td>Jusqu'à 5 travaux configurables</td>
  </tr>
  <tr>
    <td><strong>Types de sauvegarde</strong></td>
    <td>Complète & Différentielle</td>
  </tr>
  <tr>
    <td><strong>Modes d'exécution</strong></td>
    <td>Unitaire • Séquentielle • Ligne de commande (<code>1-3</code>, <code>1;3</code>)</td>
  </tr>
  <tr>
    <td><strong>Langues</strong></td>
    <td>Français & Anglais</td>
  </tr>
  <tr>
    <td><strong>Logs</strong></td>
    <td>Fichier log journalier (JSON) • Fichier d'état (<code>state.json</code>)</td>
  </tr>
  <tr>
    <td><strong>DLL dédiée</strong></td>
    <td><code>EasyLog.dll</code> pour la gestion des logs</td>
  </tr>
</table>

---

## Fonctionnalités (Version 2.0)

<table>
  <tr>
    <td><strong>Interface</strong></td>
    <td>Application graphique Avalonia</td>
  </tr>
  <tr>
    <td><strong>Travaux de sauvegarde</strong></td>
    <td>Nombre illimité de travaux configurables</td>
  </tr>
  <tr>
    <td><strong>Types de sauvegarde</strong></td>
    <td>Complète & Différentielle</td>
  </tr>
  <tr>
    <td><strong>Modes d'exécution</strong></td>
    <td>Unitaire • Séquentielle • Ligne de commande (identique à la version 1.0)</td>
  </tr>
  <tr>
    <td><strong>Langues</strong></td>
    <td>Français & Anglais, aucune chaîne en dur dans le code</td>
  </tr>
  <tr>
    <td><strong>Logs</strong></td>
    <td>
    Fichier log journalier (JSON ou XML)<br>
    • Temps de cryptage ajouté (ms)<br>
    • 0 = pas de cryptage<br>
    • &gt;0 = durée du cryptage<br>
    • &lt;0 = code erreur
    </td>
  </tr>
  <tr>
    <td><strong>Format du fichier log</strong></td>
    <td>Choix utilisateur : JSON ou XML (hérité de la version 1.1)</td>
  </tr>
  <tr>
    <td><strong>Fichier d'état</strong></td>
    <td><code>state.json</code></td>
  </tr>
  <tr>
    <td><strong>Cryptage</strong></td>
    <td>Intégration du logiciel externe <strong>CryptoSoft</strong> (cryptage conditionnel selon extensions définies)</td>
  </tr>
  <tr>
    <td><strong>Logiciel métier</strong></td>
    <td>
    Détection d’un logiciel métier<br>
    • Interdiction de lancer un travail<br>
    • En mode séquentiel : termine le fichier en cours puis stoppe<br>
    • Arrêt consigné dans le log
    </td>
  </tr>
  <tr>
    <td><strong>DLL dédiée</strong></td>
    <td><code>EasyLog.dll</code> pour la gestion des logs</td>
  </tr>
</table>

---

## Architecture

### Arborescence du projet

```
EasySave/
├── src/
│   ├── EasySave.Core           # Cœur métier, DTOs, interfaces
│   ├── EasySave.App            # Services, infrastructure, persistance
│   ├── EasySave.EasyLog        # DLL de logging
│   ├── EasySave.App.Console    # Interface console
│   ├── EasySave.App.Gui        # Interface graphique
│   └── CryptoSoft              # Chiffrement XOR
│
└── tests/
    └── EasySave.Tests          # Tests unitaires
```
---

## Équipe de Développement

<table>
  <tr>
    <td align="center">
      <strong>Christopher ASIN</strong><br>
      Développeur
    </td>
    <td align="center">
      <strong>Shayna ROSIER</strong><br>
      Développeuse
    </td>
  </tr>
  <tr>
    <td align="center">
      <strong>Mathis VOGEL</strong><br>
      Développeur
    </td>
    <td align="center">
      <strong>Maxime LANDEAU</strong><br>
      Développeur
    </td>
  </tr>
</table>

---

## Prérequis

| Composant | Version | / |
|-----------|---------|--------------|
| **Windows** | 10+ | Obligatoire |
| **.NET SDK** | 10.0+ | `dotnet --version` |
| **IDE** | Visual Studio 2026+ ou Rider | Recommandé |
| **Git** | 2.49.0 | `git --version` |

---

## Installation et Lancement

### 1. Cloner le dépôt

```bash
git clone https://github.com/RiperPro03/EasySave.git
cd EasySave
```

### 2. Générer l'executable dans 'EasySave\out\EasySave'

```bash
powershell -ExecutionPolicy Bypass -File .\scripts\publish-flat.ps1
```

### 3. Exécuter les tests unitaires

```bash
dotnet test
```

### À noter, vous pouvez lancer directement l'application Gui

```bash
dotnet run --project src/EasySave.App.Gui
```


---

## Emplacements des Fichiers

| Type de fichier | Emplacement | Description |
|----------------|-------------|-------------|
| **Logs journaliers** | `%APPDATA%\ProSoft\EasySave\Logs` | Fichiers JSON/XML avec l'historique des opérations |
| **Fichier d'état** | `%APPDATA%\ProSoft\EasySave\State\state.json` | Snapshot en temps réel de l'état global |
| **Configuration** | Dossier utilisateur système | Paramètres persistants |

---

## Licence

Ce projet est développé dans le cadre d'un projet académique **CESI**.

MIT License

Copyright &copy; 2026 — Projet académique CESI

## Diagrammes UML

### EasyLog - Système de Logging

<div align="center">

Le module **EasyLog** est une DLL dédiée au logging indépendant et réutilisable, il est de fait inchangé pour cette v2.

![Diagramme de classes EasyLog](docs/uml/EasyLog/DiagrammeClass_EasyLog.svg)

<details>
<summary><strong>Voir les détails du module EasyLog</strong></summary>

**Composants :**
- **Interfaces** : `ILogger<T>`, `ILogSerializer`, `ILogWriter`
- **Loggers** : `DailyLogger<T>`, `SafeLogger<T>`
- **Sérialisation** : JSON, XML
- **Options** : `LogOptions`, `LogFormat`
- **Factory** : `LoggerFactory`
- **Utilitaires** : `DailyFileHelper`

**Caractéristiques :**
- Écriture journalière automatique
- Support formats JSON/XML
- Option `UseSafeLogger` pour absorber les exceptions
- Horodatage et formatage des entrées

</details>

</div>

---

### Gui - Interface Utilisateur

<div align="center">

L'interface **Console** offre une expérience utilisateur en ligne de commande (CLI), elle aussi est inchangé depuis la v1, cette dernière étant "abandonné" au profit de l'interface Gui

![Diagramme de classes Console](docs/uml/Console/DiagrammeClass_Console_(ConsoleUi).svg)

<details>
<summary><strong>Voir les détails du module Console</strong></summary>

**Composants :**
- **Controllers** : `MenuController`, `JobController`, `BackupController`, `SettingsController`
- **Views** : `ConsoleView`, `JobView`, `BackupView`
- **Input** : `ConsoleInput`, `ArgsParser`
- **Bootstrap** : `Program`

**Responsabilités :**
- Affichage des menus interactifs
- Gestion des commandes utilisateur
- Lancement d'un job ou d'un batch
- Internationalisation FR/EN
- Affichage des résultats en temps réel

</details>

</div>

---

L'interface **Gui** permet un affichage graphique

![Diagramme de classes Console](docs/uml/Gui/DiagrammeClass_Gui_v2.svg)

<details>
<summary><strong>>Voir les détails du module Gui</strong></summary>

Composants :

Program : Point d'entrée, initialise le builder Avalonia.

App : Gère le cycle de vie, l'injection des services (Log, Job, Backup) et la configuration initiale.

ViewLocator : Mécanisme de résolution automatique pour lier les ViewModels aux Views correspondantes.

Responsabilités :

Initialisation du framework Avalonia.

Injection de dépendances (DI) manuelle des services globaux.

Configuration du contexte de logging et chargement des paramètres.

</details>

<details>
<summary><strong>ViewModels (Logique de présentation)</strong></summary>

Composants :

MainWindowViewModel : Orchestrateur principal, gère la navigation et le statut global.

DashboardViewModel : Gère le résumé des activités, les statistiques de jobs et le statut système.

JobsViewModel & JobEditorViewModel : Gestion CRUD des travaux de sauvegarde et interface d'édition.

ExecutionViewModel : Pilotage en temps réel des sauvegardes (Play, Pause, Stop, Progress).

LogsViewModel : Visualisation et filtrage de l'historique des opérations.

SettingsViewModel : Gestion des préférences (Langue, Chiffrement, Processus métier).

Responsabilités :

Liaison de données (Data Binding) avec les vues.

Appel des services métier (IJobService, IBackupService).

Transformation des données brutes en données affichables.

</details>

<details>
<summary><strong>Views & Converters (Interface Utilisateur)</strong></summary>

Composants :

Windows/UserControls : MainWindow, DashboardView, JobsView, ExecutionView, etc.

Dialogs : JobEditorDialog pour la création/modification.

Converters : LogLevelToBrush, StatusToText, TypeToEmoji, L10nFormat.

Responsabilités :

Rendu visuel en XAML.

Conversion de types complexes en ressources graphiques (Couleurs, Icônes, Libellés traduits).

Capture des interactions utilisateur.

</details>

<details>
<summary><strong>Models & Localization</strong></summary>

Composants :

Models : ExecutionJobItem, LogEntryItem, RecentActivityItem (Wrappers UI).

Loc (Localization) : Gestionnaire de ressources multilingues (FR/EN).

Enums : NavigationTab pour le routage interne.

Responsabilités :

Fournir des structures de données optimisées pour la vue.

Gérer le changement de langue à la volée sans redémarrage.

Formater les entrées de log pour une lecture humaine (LogSection/LogField).

</details>

<details>
<summary><strong>Dépendances Externes</strong></summary>

Services injectés :

IJobService / IBackupService : Cœur métier de l'application.

SettingsService : Persistance des configurations.

LogReaderService : Accès aux fichiers de logs physiques.

Responsabilités :

Découpler l'interface utilisateur de la logique métier.

Permettre la testabilité des ViewModels via des mocks.

</details>
</div>

---

### App - Logique Applicative

<div align="center">

Le module **App** contient les implémentations concrètes des services métier.

![Diagramme de classes App](docs/uml/App/DiagrammeClass_App_Implementation_v2.svg)

<details>
<summary><strong>Voir les détails du module App</strong></summary>

**Services :**
- `BackupEngine` : Exécution d'un job, progression, logs
- `BackupService` : Orchestration et snapshot global `state.json`
- `JobService` : Opérations CRUD sur les jobs
- `PathProvider` : Gestion des chemins applicatifs (AppData/ProSoft/EasySave)
- `StateWriter` : Écriture du snapshot d'état global

**Stratégies de copie :**
- `FullCopyStrategy` : Copie totale des fichiers
- `DifferentialCopyStrategy` : Copie sélective (taille, date, hash)

**Repositories :**
- `JobRepository` : Persistance dans `jobs.json` (limite 5 jobs)
- `AppConfigRepository` : Persistance `setting.json` (langue, format log)

</details>

</div>

---

### Core - Couche Métier

<div align="center">

Le module **Core** définit le domaine métier, les DTOs et les contrats de l'application.

![Diagramme de classes Core](docs/uml/Core/DiagrammeClass_Core_v2.svg)

<details>
<summary><strong>Voir les détails du module Core</strong></summary>

**Responsabilités :**
- **Modèles métier** : `BackupJob`, `AppConfig`
- **DTOs** : `BackupJobDto`, `BackupResultDto`, `JobStateDto`, `LogEntryDto`, `ResultDto`, `AppStateDto`
- **Énumérations** : `BackupType`, `JobStatus`, `Language`
- **Interfaces** : `IBackupEngine`, `IBackupService`, `IJobService`, `IJobRepository`, `IBackupCopyStrategy`, `IPathProvider`, `IStateWriter`, `IFileSystem`
- **Événements** : `JobStateChangedEventArgs`
- **Utilitaires** : `Guard`, `Localization`

**Contraintes :**
- Aucune dépendance UI
- Pas de code d'infrastructure
- Interfaces testables uniquement

</details>

</div>

---

## Diagrammes Généraux

### Diagramme d'Activité

<div align="center">

Vue d'ensemble du flux d'exécution des sauvegardes dans l'application.

![Diagramme d'activité général](docs/uml/DiagrammeActivite_General_v2.svg)

<details>
<summary><strong>Voir les détails du diagramme d'activité</strong></summary>

**Représente :**
- Flux de création d'un travail de sauvegarde
- Processus d'exécution (complète/différentielle)
- Gestion des erreurs et des logs
- Mise à jour de l'état en temps réel

</details>

</div>

**Version alternative :**

<div align="center">

![Diagramme d'activité général - Mermaid](docs/uml/DiagrammeActivite_General_mermaid_v2.svg)

</div>

---

### Diagramme de Cas d'Utilisation

<div align="center">

Interactions entre les acteurs (utilisateurs) et le système EasySave.

![Diagramme de cas d'utilisation](docs/uml/DiagrammeUseCase_General_v2.svg)

<details>
<summary><strong>Voir les détails du diagramme de cas d'utilisation</strong></summary>

**Acteurs et cas d'usage :**
- Utilisateur : Créer, configurer, exécuter des sauvegardes
- Système : Gérer les logs, surveiller l'état, gérer les fichiers
- Relations : Include, Extend entre les cas d'usage

</details>

</div>

---

## Diagrammes de Séquence

### Lancement d'un Job de Sauvegarde

<div align="center">

Séquence d'exécution complète d'un travail de sauvegarde.

![Diagramme de séquence - Lancement d'un job](docs/uml/DiagrammeSequence_de_lancement_d_un_job_v2.svg)

<details>
<summary><strong>Voir les détails du diagramme</strong></summary>

**Interactions :**
- Initialisation du job par l'utilisateur
- Vérification des paramètres et de l'état
- Démarrage du processus de sauvegarde
- Notification de progression
- Finalisation et mise à jour de l'état

</details>

</div>

---

### Sauvegarde Différentielle

<div align="center">

Processus détaillé d'une sauvegarde différentielle.

![Diagramme de séquence - Sauvegarde différentielle](docs/uml/DiagrammeSequence_sauvegarde_differentielle_v2.svg)

<details>
<summary><strong>Voir les détails du diagramme</strong></summary>

**Processus :**
- Comparaison avec la dernière sauvegarde complète
- Identification des fichiers modifiés
- Copie sélective des fichiers différents
- Mise à jour des métadonnées
- Génération des logs différentiels

</details>

</div>

---

### Lancement d'un Batch via Arguments

<div align="center">

Exécution de plusieurs sauvegardes en mode batch via ligne de commande.

![Diagramme de séquence - Batch via arguments](docs/uml/DiagrammeSequence_lancement_d_un_batch_via_arguments_v2.svg)

<details>
<summary><strong>Voir les détails du diagramme</strong></summary>

**Workflow :**
- Parsing des arguments en ligne de commande
- Validation de la syntaxe (ex: `1-3`, `1;3`)
- Exécution séquentielle des jobs
- Gestion des erreurs par job
- Rapport global de l'exécution

</details>

</div>

---

### Processus de Journalisation

<div align="center">

Mécanisme de création et mise à jour des logs en temps réel.

![Diagramme de séquence - Journalisation](docs/uml/DiagrammeSequence_de_journalisation_v2.svg)

<details>
<summary><strong>Voir les détails du diagramme</strong></summary>

**Flux de journalisation :**
- Capture des événements de sauvegarde
- Formatage en JSON/XML
- Écriture dans le fichier log journalier
- Mise à jour du fichier d'état global
- Horodatage et traçabilité

</details>

</div>
