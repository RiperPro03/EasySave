# Changelog (v1)

Toutes les modifications notables de ce projet sont documentées dans ce fichier.  
Ce projet respecte le versionnement sémantique.

## [1.0.0] - Version initiale

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

### Architecture
- Séparation des projets :
  - EasySave.Core
  - EasySave.App
  - EasySave.EasyLog
  - EasySave.App.Console
- Architecture pensée pour accueillir une interface graphique en version future (EasySave.App.Gui)

### Limitations connues
- Application en ligne de commande uniquement
- Maximum 5 travaux de sauvegarde
- Sauvegarde séquentielle uniquement