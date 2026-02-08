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

- [FEAT] Console : Exécution mono + séquentielle  
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

- Définir l’architecture du projet  
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

### Ajouté
- Application Console EasySave (.NET)
- Création de travaux de sauvegarde (jusqu'à 5)
- Propriétés d’un travail :
  - Nom
  - Répertoire source
  - Répertoire cible
  - Type de sauvegarde (Complète / Différentielle)
- Support multilingue (Français / Anglais)
- Menu principal interactif
- Exécution d’un travail par sélection
- Exécution séquentielle de tous les travaux
- Exécution via ligne de commande :
  - EasySave.exe 1-3
  - EasySave.exe 1;3
- Fichier log journalier au format JSON
- Écriture des logs en temps réel
- Bibliothèque dédiée EasyLog.dll
- Fichier d’état temps réel `state.json`
- Tests unitaires

### Limitations connues
- Application en ligne de commande uniquement
- Maximum 5 travaux de sauvegarde
- Sauvegarde séquentielle uniquement