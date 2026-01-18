# Architecture du projet EasySave

## Objectif

Ce document décrit l’architecture logicielle du projet **EasySave**, ses principes de conception et la répartition des responsabilités entre les différents projets de la solution.

L’objectif est de garantir :

* une **séparation claire des responsabilités**,
* une **maintenabilité élevée**,
* la possibilité de **faire évoluer ou remplacer les interfaces** sans modifier le cœur métier.

---

## Vue d’ensemble

L’architecture repose sur une approche **Clean Architecture** organisée autour de **4 projets principaux** :

```
EasySave.Core        ← cœur métier
EasySave.App         ← moteur / infrastructure
EasySave.App.Console ← interface console
EasySave.App.Gui     ← interface graphique (Avalonia ou WPF)
```

Les interfaces (Console / GUI) sont totalement découplées de la logique métier.

---

## Règles de dépendances

Les dépendances entre projets respectent les règles suivantes :

```
EasySave.App.Gui       ─┐
EasySave.App.Console    ├──> EasySave.App ───> EasySave.Core
                        └──> EasySave.Core
```

* **EasySave.Core** ne dépend d’aucun autre projet
* **EasySave.App** dépend uniquement de `EasySave.Core`
* Les interfaces (Console / GUI) dépendent de `Core` et `App`

---

## Description des projets

### 1. EasySave.Core

**Rôle :** cœur métier et contrats de l’application.

Contient :

* les **modèles métier** (ex. `BackupJob`, `BackupMode`, `JobState`),
* les **règles métier** et structures de données,
* les **interfaces (contrats)** utilisées par le reste de l’application,
* les **DTO (Data Transfer Objects)** destinés aux interfaces.

Exemples d’interfaces :

* `IBackupService`
* `IJobRepository`
* `IFileSystem`
* `ILogWriter`
* `ICryptoService`

Contraintes :

* aucune dépendance UI,
* aucun accès direct au système (fichiers, processus, réseau),
* logique pure et testable.

---

### 2. EasySave.App

**Rôle :** moteur concret de l’application.

Ce projet implémente les interfaces définies dans `EasySave.Core`.

Organisation interne :

#### Services/

* Implémentent la **logique applicative concrète**
* Orchestration des sauvegardes, chiffrement, détection de processus, logs

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

#### Factories/

* Centralisation de la création des services
* Évite la création d’objets (`new`) dans les interfaces utilisateur

Exemple :

* `EasySaveFactory.CreateBackupService()`

---

### 3. EasySave.App.Console

**Rôle :** interface utilisateur en ligne de commande.

Responsabilités :

* affichage des menus,
* interaction utilisateur (sélection de jobs, lancement, arrêt),
* affichage des logs et de la progression.

Ce projet :

* ne contient aucune logique métier,
* utilise exclusivement les interfaces exposées par `EasySave.Core`.

---

### 4. EasySave.App.Gui

**Rôle :** interface graphique.

Technologie :

* Avalonia MVVM.

Organisation :

* **Views** : fichiers XAML (affichage)
* **ViewModels** : logique d’interface (bindings, commandes)

Principes :

* respect strict du **pattern MVVM**,
* aucun accès direct au système,
* aucun traitement métier dans les ViewModels,
* communication avec le moteur uniquement via les interfaces du Core.

---

## Bénéfices de l’architecture

* séparation claire des responsabilités
* testabilité élevée du cœur métier
* interfaces interchangeables (Console, GUI, WPF, Avalonia)
* facilité de maintenance et d’évolution
* architecture conforme aux principes **SOLID** et **Clean Architecture**