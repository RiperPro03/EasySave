# CONTRIBUTING

Ce document décrit les règles de contribution au projet **EasySave**.  
L’objectif est de garantir un développement structuré, collaboratif et conforme aux bonnes pratiques professionnelles.

Toute contribution implique l’acceptation et le respect des règles ci-dessous.

---

## 1. Principes généraux

- La branche `main` est protégée et doit rester stable.
- Aucun push direct sur `main` n’est autorisé.
- Toute modification passe obligatoirement par une Pull Request.
- Chaque Pull Request doit être liée à une Issue.
- L’intégration continue (CI) doit être validée avant tout merge.
- Le projet suit une architecture claire séparant le métier, l’infrastructure et l’interface.

---

## 2. Gestion des Issues

- Le travail est organisé via GitHub Issues.
- Une Issue correspond à une fonctionnalité, une tâche technique ou une correction.
- Les grandes fonctionnalités peuvent être découpées en sous-Issues (relation parent/enfant).
- Chaque Issue doit contenir :
  - un objectif clair
  - une liste de tâches
  - des critères d’acceptation

---

## 3. Création des branches

- Les branches sont créées depuis les Issues.
- Convention de nommage recommandée : `<id-issue>-description-courte`

Exemple : 12-run-all-jobs


- Une branche ne doit traiter qu’une seule Issue.

---

## 4. Développement et commits

- Les commits doivent être fréquents et explicites.
- Le message de commit doit décrire clairement l’action réalisée.
Exemple : `feat: implémentation de l’exécution complète des jobs`

- Les fichiers générés automatiquement (bin/, obj/, .vs/) ne doivent jamais être commités.

---

## 5. Pull Requests

- Les Pull Requests doivent cibler la branche `main`.
- Le template de Pull Request doit être rempli intégralement.
- La description de la PR doit contenir la référence à l’Issue associée : `Closes #<id-issue>`


- Une Pull Request ne doit traiter qu’une seule Issue.

---

## 6. Revue de code

- Toute Pull Request doit être relue avant d’être fusionnée.
- Le mainteneur du projet est responsable de la validation finale.
- Une Pull Request peut être refusée si :
- la CI est en échec
- le template n’est pas respecté
- l’architecture définie n’est pas respectée
- le périmètre dépasse l’Issue associée

---

## 7. Intégration Continue (CI)

- La CI s’exécute automatiquement sur chaque Pull Request.
- Si la Pull Request modifie du code :
- les tests unitaires sont exécutés
- le build est vérifié
- Si la Pull Request ne modifie que de la documentation :
- les étapes de build et de test sont ignorées
- la pipeline est néanmoins validée

Une Pull Request ne peut être fusionnée que si la CI est validée.

---

## 8. Architecture du projet

- `src/` : code source de l’application
- `tests/` : tests unitaires
- `docs/` : documentation, UML et schémas
- L’interface utilisateur ne doit contenir aucune logique métier.
- Les règles métier doivent être testables indépendamment de l’interface.

---

## 9. Documentation

- Toute fonctionnalité significative doit être documentée.
- Les diagrammes UML et documents techniques sont stockés dans le dossier `docs/`.
- Les modifications de comportement doivent être reflétées dans la documentation.

---

## 10. Signalement des problèmes

- Les bugs, questions ou propositions doivent être déclarés via une Issue.
- Les labels appropriés doivent être utilisés (`bug`, `question`, `enhancement`, etc.).
- Aucun correctif ne doit être intégré sans Issue associée.

---

## 11. Objectif de qualité

Ces règles ont pour but :
- d’assurer la stabilité de la branche principale
- de faciliter le travail en équipe
- de garantir la lisibilité et la maintenabilité du projet
- de démontrer une méthode de travail professionnelle

Le non-respect répété de ces règles peut entraîner le refus des contributions concernées.


