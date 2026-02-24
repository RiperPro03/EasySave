# Changelog (v1)

Toutes les modifications notables de ce projet sont documentées dans ce fichier.
Ce projet respecte le versionnement sémantique.

## [1.0.0] - Version initiale

### Features

- [FEAT] Gestion des chemins (Logs / State / Config)  
  https://github.com/RiperPro03/EasySave/issues/42
- [FEAT] Service de sauvegarde "Complete"  
  https://github.com/RiperPro03/EasySave/issues/40
- [FEAT] Service de sauvegarde "Differential"  
  https://github.com/RiperPro03/EasySave/issues/41
- [FEAT] FR / EN (console)  
  https://github.com/RiperPro03/EasySave/issues/48
- [FEAT] Modèle métier des jobs + enums  
  https://github.com/RiperPro03/EasySave/issues/38
- [FEAT] Gestion des jobs (limite à 5)  
  https://github.com/RiperPro03/EasySave/issues/39
- [FEAT] Menus CRUD jobs  
  https://github.com/RiperPro03/EasySave/issues/45
- [FEAT] Console : exécution mono + séquentielle  
  https://github.com/RiperPro03/EasySave/issues/46
- [FEAT] CLI : parsing 1-3 et 1;3  
  https://github.com/RiperPro03/EasySave/issues/47
- [FEAT] state.json : état temps réel  
  https://github.com/RiperPro03/EasySave/issues/44
- [FEAT] Log journalier JSON (EasyLog.dll)  
  https://github.com/RiperPro03/EasySave/issues/43
- [FEAT] Amélioration console  
  https://github.com/RiperPro03/EasySave/issues/60

### Architecture

- Définir l'architecture du projet  
  https://github.com/RiperPro03/EasySave/issues/2
- Initialisation GitOps du projet  
  https://github.com/RiperPro03/EasySave/issues/1
- Initialisation du projet en .NET  
  https://github.com/RiperPro03/EasySave/issues/6

### Documentation

- Analyse fonctionnelle EasySave  
  https://github.com/RiperPro03/EasySave/issues/3
- Docs livrable v1.0 (user 1 page + support + changelog + UML)  
  https://github.com/RiperPro03/EasySave/issues/49

### Limitations connues

- Application en ligne de commande uniquement
- Maximum 5 travaux de sauvegarde
- Sauvegarde séquentielle uniquement

## [2.0.0]

### Features

- [FEAT] App GUI navigation minimale  
  https://github.com/RiperPro03/EasySave/issues/85
- [FEAT] Add Folder Pickers (Source/Target) with UI Validation  
  https://github.com/RiperPro03/EasySave/issues/90
- [FEAT] Backup Job View  
  https://github.com/RiperPro03/EasySave/issues/91
- [FEAT] Live Execution View  
  https://github.com/RiperPro03/EasySave/issues/92
- [FEAT] Setting View  
  https://github.com/RiperPro03/EasySave/issues/95
- [FEAT] Repository Jobs : passer à illimité  
  https://github.com/RiperPro03/EasySave/issues/97
- [FEAT] Integrate CryptoSoft  
  https://github.com/RiperPro03/EasySave/issues/99
- [FEAT] Log : ajouter "temps de crytage (ms)"  
  https://github.com/RiperPro03/EasySave/issues/100
- [FEAT] Détection logiciel métier (process) paramétrable  
  https://github.com/RiperPro03/EasySave/issues/101
- [FEAT] Interdire lancement job si logiciel métier actif (et loguer l'arrêt)  
  https://github.com/RiperPro03/EasySave/issues/102
- [FEAT] Séquentiel : si logiciel métier apparaît -> terminer fichier courant puis stopper  
  https://github.com/RiperPro03/EasySave/issues/103

### Documentation

- Docs livrable v2.0 (user + support + changelog + UML)  
  https://github.com/RiperPro03/EasySave/issues/98
