# Architecture du projet EasySave

## Objectif
Ce document décrit l'architecture logicielle du projet **EasySave**, ses principes de conception et la répartition des responsabilités entre les différents projets de la solution.

L'objectif est de garantir :
* une **séparation claire des responsabilités**,
* une **maintenabilité élevée**,
* la possibilité de **faire évoluer ou remplacer les interfaces** sans modifier le cœur métier.

---

## Vue d'ensemble
L'architecture repose sur une approche **Clean Architecture** organisée autour de **5 projets principaux** :
```
EasySave.Core        ← cœur métier
EasySave.App         ← moteur / infrastructure
EasySave.EasyLog     ← DLL dédiée au logging
EasySave.App.Console ← interface console
EasySave.App.Gui     ← interface graphique (Avalonia)
```

Les interfaces (Console / GUI) sont totalement découplées de la logique métier.

---

## Règles de dépendances
Les dépendances entre projets respectent les règles suivantes :
```
EasySave.App.Gui       ─┐
EasySave.App.Console    ├──> EasySave.App ───> EasySave.Core
                        └──> EasySave.Core
                             
EasySave.App ──────────────> EasySave.EasyLog
```

* **EasySave.Core** ne dépend d'aucun autre projet
* **EasySave.EasyLog** est une DLL indépendante (peut être utilisée par d'autres projets)
* **EasySave.App** dépend de `EasySave.Core` et `EasySave.EasyLog`
* Les interfaces (Console / GUI) dépendent de `Core` et `App`

---

## Description des projets

### 1. EasySave.Core
**Rôle :** cœur métier et contrats de l'application.

Contient :
* les **modèles métier** (ex. `BackupJob`, `BackupMode`, `JobState`),
* les **règles métier** et structures de données,
* les **interfaces (contrats)** utilisées par le reste de l'application,
* les **DTO (Data Transfer Objects)** destinés aux interfaces.

Exemples d'interfaces :
* `IBackupService`
* `IJobRepository`
* `IFileSystem`
* `ICryptoService`

Contraintes :
* aucune dépendance UI,
* aucun accès direct au système (fichiers, processus, réseau),
* logique pure et testable.

---

### 2. EasySave.App
**Rôle :** moteur concret de l'application.

Ce projet implémente les interfaces définies dans `EasySave.Core`.

Organisation interne :

#### Services/
* Implémentent la **logique applicative concrète**
* Orchestration des sauvegardes, chiffrement, détection de processus

Exemples :
* `BackupService`
* `FileSystemService`
* `CryptoService`
* `ProcessDetectorService`

#### Repositories/
* Gestion de la **persistance des données**
* Lecture / écriture des jobs, états, configurations (JSON/XML/fichiers)

Exemples :
* `JobRepository`
* `ConfigRepository`
* `StateRepository`

#### Factories/
* Centralisation de la création des services
* Évite la création d'objets (`new`) dans les interfaces utilisateur

Exemple :
* `EasySaveFactory.CreateBackupService()`
* `EasySaveFactory.CreateLogWriter()`

---

### 3. EasySave.EasyLog
**Rôle :** DLL dédiée au système de logging.

**Caractéristiques :**
* Bibliothèque **indépendante et réutilisable**
* Support multi-formats (JSON NDJSON, XML)
* Logging synchrone avec écriture journalière
* Configuration via options (chemin, format, safe logger)

**Fonctionnalités :**
* Écriture de logs journaliers
* Sérialisation via stratégie JSON/XML
* Décorateur SafeLogger pour éviter les crashs

**Interfaces exposées :**
* `ILogger<T>` - Interface principale de logging
* `ILogSerializer` - Stratégies JSON/XML
* `ILogWriter` - Écriture physique des logs

**Exemples d'utilisation :**
```csharp
// Dans EasySave.App
var options = new LogOptions
{
    LogDirectory = "C:/EasySave/Logs",
    Format = LogFormat.Json,
    UseSafeLogger = true
};

var logger = LoggerFactory.Create<LogEntry>(options);
logger.Write(new LogEntry
{
    JobName = "Documents",
    Message = "Backup started",
    Timestamp = DateTime.Now
});
```

**Avantages :**
* Découplage total du logging
* Testabilité (mock de `ILogger`)
* Réutilisabilité dans d'autres projets ProSoft
* Évolution indépendante (ajout de nouveaux formats)


### 4. EasySave.App.Console
**Rôle :** interface utilisateur en ligne de commande.

Responsabilités :
* Affichage des menus,
* Interaction utilisateur (sélection de jobs, lancement, arrêt),
* Affichage des logs et de la progression.

Ce projet :
* ne contient aucune logique métier,
* utilise exclusivement les interfaces exposées par `EasySave.Core`,
* utilise `EasySave.EasyLog` via les services de `EasySave.App`.

---

### 5. EasySave.App.Gui
**Rôle :** interface graphique.

Technologie :
* **Avalonia MVVM** (cross-platform : Windows, macOS, Linux)

Organisation :
* **Views/** : fichiers XAML (affichage)
* **ViewModels/** : logique d'interface (bindings, commandes)
* **Models/** : modèles de présentation (si nécessaire)

Principes :
* Respect strict du **pattern MVVM**,
* Aucun accès direct au système,
* Aucun traitement métier dans les ViewModels,
* Communication avec le moteur uniquement via les interfaces du Core.

**Structure de l'interface (v2.0+) :**

| Onglet | Rôle |
|--------|------|
| Dashboard | Vue globale & statut système |
| Backup Jobs | CRUD des jobs de sauvegarde |
| Live Execution | Suivi temps réel des sauvegardes |
| Logs | Consultation des journaux (JSON/XML) |
| Settings | Configuration globale |
| About | Informations produit & support |

---

## Évolution par versions

### v1.0 - Console MVP
* Interface console uniquement
* Maximum 5 jobs
* Sauvegarde complète et différentielle
* Logs JSON uniquement
* Fichier `state.json` temps réel
* Support FR/EN

### v1.1 - Logs Legacy
* Ajout support XML pour clients legacy
* Configuration format de log (JSON/XML)
* **Introduction d'EasySave.EasyLog**

### v2.0 - Interface Graphique
* GUI Avalonia MVVM
* Jobs illimités
* Intégration CryptoSoft (chiffrement)
* Détection et blocage du logiciel métier
* Architecture multi-projets stabilisée

### v3.0 - Parallélisme Avancé
* Exécution parallèle des jobs
* Système de priorités par extension
* Contrôles Play/Pause/Stop par job
* Logs centralisés (Docker)
* Mode mono-instance CryptoSoft

---

## Bénéfices de l'architecture

✅ **Séparation claire des responsabilités**
✅ **Testabilité élevée du cœur métier**
✅ **Interfaces interchangeables** (Console, GUI, API future)
✅ **Facilité de maintenance et d'évolution**
✅ **Réutilisabilité** (EasyLog utilisable ailleurs)
✅ **Conformité aux principes SOLID et Clean Architecture**

---

## Flux de données

### Exemple : Création d'un job de sauvegarde
```
GUI (View)
    ↓ Command
ViewModel
    ↓ IBackupService
BackupService (App)
    ↓ IJobRepository
JobRepository (App)
    ↓ File System
jobs.json
```

### Exemple : Exécution d'une sauvegarde
```
GUI/Console
    ↓ IBackupService.RunJob()
BackupService
    ├─→ IFileSystem (copie fichiers)
    ├─→ ICryptoService (chiffrement)
    ├─→ IProcessDetector (vérification logiciel métier)
    └─→ ILogger (EasyLog)
            ↓
        state.json + daily_log.json
```

---

## Diagramme de dépendances
```
┌─────────────────────────────────────────────┐
│          EasySave.App.Gui (MVVM)            │
│          EasySave.App.Console (CLI)         │
└──────────────┬──────────────────────────────┘
               │
               ↓
┌──────────────────────────────────────────────┐
│           EasySave.App (Services)            │
│  - BackupService                             │
│  - FileSystemService                         │
│  - CryptoService                             │
│  - JobRepository                             │
└──────┬───────────────────────────────┬───────┘
       │                               │
       ↓                               ↓
┌──────────────────┐         ┌─────────────────┐
│ EasySave.Core    │         │ EasySave.EasyLog│
│ (Interfaces +    │         │ (DLL Logging)   │
│  Domain Models)  │         └─────────────────┘
└──────────────────┘
```

---

## Conventions de nommage

### Projets
* `EasySave.Core` - Domaine métier
* `EasySave.App` - Application / Infrastructure
* `EasySave.EasyLog` - Bibliothèque de logging
* `EasySave.App.Console` - Interface CLI
* `EasySave.App.Gui` - Interface graphique

### Namespaces
```csharp
EasySave.Core.Models
EasySave.Core.Interfaces
EasySave.App.Services
EasySave.App.Repositories
EasySave.EasyLog
EasySave.App.Gui.ViewModels
EasySave.App.Gui.Views
```

### Interfaces
Préfixe `I` obligatoire : `IBackupService`, `ILogger`, `IJobRepository`

---

## Gestion de la qualité

| Aspect | Mise en œuvre |
|--------|---------------|
| Versioning | Tags Git (`vX.Y.Z`) |
| CI/CD | Tests automatisés, build, packaging |
| UML | Diagrammes livrés avant chaque livrable |
| Documentation | Manuel utilisateur, doc support, changelog |
| Langues | Anglais (code) / Français (docs utilisateur) |
| Tests | Tests unitaires (Core), tests d'intégration (App) |

---

## Points d'attention

⚠️ **Les ViewModels ne doivent jamais** :
* Appeler directement `File.WriteAllText()` ou `Process.Start()`
* Implémenter de la logique métier
* Créer des instances de services avec `new`

⚠️ **Le Core ne doit jamais** :
* Référencer System.IO, System.Diagnostics, ou UI
* Contenir du code d'infrastructure
* Dépendre d'un projet externe
