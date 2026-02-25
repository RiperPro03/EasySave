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

# Version 3.0

## 1. Nouveautés générales

La version EasySave 3.0 introduit plusieurs évolutions majeures destinées à améliorer les performances, la gestion des priorités et l’expérience utilisateur.
Cette version marque une rupture importante avec les précédentes grâce à l’introduction du mode parallèle, d’une gestion avancée des fichiers, et d’une centralisation des logs.

## 2. Sauvegarde en parallèle

EasySave 3.0 abandonne le mode séquentiel pour un fonctionnement parallèle : 
- Plusieurs travaux peuvent s'exécuter simultanément
- Chaque job peut traiter plusieurs fichiers en parallèle

## 3. Gestions des fichiers prioritaires

L'utilisateur peut désormais définir une **liste d'extensions prioritaires** dans les paramètres.
Tant qu'un fichier prioritaire est en attente dans au moins un job, aucun fichier non prioritaire ne peut être transféré.

## 4. Limitation des transferts simultanés pour les fichiers volumineux

Pour éviter la saturation réseau, EasySave 3.0 introduit un **seuil maximal (n Ko)** configurable par l'utilisateur.

**Règle** :
- Deux fichiers **supérieurs au seuil** ne peuvent pas être transférés en même temps
- Pendant le transfert d'un fichier volumineux, les autres jobs peuvent continuer à transférer des fichiers plus petits (si les règles de priorité le permettent)
Ce seuil est configurable dans les paramètres généraux.

## 5. Interaction en temps réel avec les travaux

L’utilisateur peut désormais contrôler chaque travail individuellement ou l’ensemble des travaux :
- **Pause**
- **Reprise**
- **Arrêt immédiat**
- **Suivi en temps réel** (progression, état, fichier en cours, etc.)
Cette fonctionnalité améliore la maîtrise et la visibilité des opérations.

## 6. Pause automatique en cas de logiciel métier actif

Si un logiciel métier défini par l’utilisateur est détecté :
- Tous les travaux passent automatiquement en pause
- Ils reprennent automatiquement lorsque le logiciel métier est fermé
Ce mécanisme garantit que les sauvegardes ne perturbent pas les applications critiques.

## 7. CryptoSoft Mono‑Instance

CryptoSoft est désormais **mono‑instance** :
- Impossible d’exécuter plusieurs instances simultanément

## 8. Centralisation des logs journaliers (Docker)

EasySave 3.0 permet la **centralisation des logs** via un serveur de logs dédié, déployé dans un conteneur Docker.

### 8.1. Prérequis
- Docker installé
- Accès au projet LogHub.Server
- Un port disponible (par défaut : 9696)
- Un dossier local sur le serveur pour stocker les logs (ex : /home/nas/loghub-logs)

### 8.2 Upload du projet sur le serveur 

Depuis le poste local :
```
scp -r .\src\LogHub.Server nas@192.168.74.137:/home/nas/easysave/
```
- nas = utilisateur du serveur
- 192.168.74.137 = IP du serveur
- /home/nas/easysave/ = dossier cible

### 8.3. Construction de l’image Docker
Dans le dossier contenant le Dockerfile :
```
docker build -t loghub-server:latest .
```

### 8.4. Lancement du conteneur

```
docker run -d \
  --name loghub-server \
  -p 9696:9696 \
  -e LogHub__Port=9696 \
  -v ~/loghub-logs:/app/logs \
  --restart unless-stopped \
  loghub-server:latest
```
- **--name loghub-server : nom du conteneur**

- **-e LogHub__Port=9696 : configure le port interne**

- **-p 9696:9696 : expose le port du serveur**

- **-v ~/loghub-logs:/app/logs : volume Docker persistant contenant les logs**

Les fichiers journaliers centralisés seront alors disponibles dans le dossier /var/easysave/logs du serveur.

### 8.5 Vérifier le fonctionnement

Voir les conteneurs actifs :
```
docker ps
```
Voir les logs du serveur :
```
docker logs -f loghub-server
```
### 8.6. Configuration côté EasySave
Dans les Settings d’EasySave :

- **Log storage mode :**
	- LocalOnly
	- ServerOnly
	- LocalAndServer
- **Log server host** : adresse du serveur
- **Log server port** : port exposé

EasySave enverra alors les logs en temps réel au serveur Docker via un **WebSocket**.
Le serveur expose l'URL suivante : ```ws://<host>:<port>/ws/logs``` 
Pour une version sécurisée (SSL), l'URL devient : ```wss://<host>:<port>/ws/logs```

## 9. Nouveaux paramètres 

- Liste des extensions prioritaires
- Seuil maximal de taille (Ko) pour les transferts simultanés
- Mode de centralisation des logs
- Gestion de CryptoSoft mono-instance