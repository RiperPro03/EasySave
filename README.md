<section align="center">
  <h1>
    EasySave
  </h1>
  <p>
    The <strong>EasySave</strong> project.
  </p>

  <div>
    <a href="https://learn.microsoft.com/en-us/dotnet/csharp/" target="_blank">
      <img src="https://img.shields.io/badge/Language-C%23-purple.svg" alt="C# Documentation">
    </a>
    <a href="https://learn.microsoft.com/fr-fr/dotnet/core/whats-new/dotnet-10/overview" target="_blank">
      <img src="https://img.shields.io/badge/Framework-.NET%2010-darkblue.svg" alt=".NET 10 Documentation">
    </a>
    <a href="https://www.sonarsource.com/" target="_blank">
      <img src="https://img.shields.io/badge/Code%20Quality-SonarQube-4E9BCD.svg" alt="SonarQube">
    </a>
    <a href="https://avaloniaui.net/" target="_blank">
      <img src="https://img.shields.io/badge/UI%20Framework-Avalonia-blue.svg" alt="Avalonia">
    </a>
  </div>
</section>

## Presentation
EasySave est un logiciel de sauvegarde developpe en C# / .NET, dans le cadre du projet fil rouge ProSoft (CESI).
Le projet est concu pour evoluer par versions successives (v1 -> v3) en respectant les principes de qualite logicielle,
maintenabilite et architecture propre.

## Fonctionnalites (Version 1.0)
- Application console en .NET
- Creation de jusqu'a 5 travaux de sauvegarde
- Sauvegarde : complete, differentielle
- Execution : unitaire, sequentielle, via ligne de commande (ex: `1-3`, `1;3`)
- Support Francais / Anglais
- Generation en temps reel :
  - Fichier log journalier (JSON)
  - Fichier state (`state.json`) snapshot global de l'etat d'avancement
- DLL dediee pour les logs : `EasyLog.dll`

## Architecture (arborescence simplifiee)
```
EasySave/
├── src/
│  ├── EasySave.Core          (coeur metier, objets d’echange de donnees, interfaces)
│  ├── EasySave.App           (services, orchestration, persistance)
│  ├── EasySave.EasyLog       (DLL de logging)
│  ├── EasySave.App.Console   (interface console)
│  └── EasySave.App.Gui       (interface graphique - v2)
│
└── tests/
   └── EasySave.Tests
```

## Equipe
- Christopher ASIN
- Shayna ROSIER
- Mathis VOGEL
- Maxime

## Prerequis
- Windows
- .NET SDK 10.0
- Visual Studio 2022+ ou Rider
- Git

## Verification
`dotnet --version`

## Initialisation du projet
```bash
git clone https://github.com/RiperPro03/EasySave.git
cd EasySave
```

## Lancer l'application (Console)
```bash
dotnet run --project src/EasySave.App.Console
```

## Tests unitaires
```bash
dotnet test
```

## Emplacements des fichiers
- Logs journaliers : dossier systeme utilisateur (ex: `%APPDATA%\ProSoft\EasySave\Logs`)
- Fichier state : fichier unique mis a jour en temps reel (ex: `%APPDATA%\ProSoft\EasySave\State\state.json`)
- Format : JSON / XML (logs)

## Internationalisation
- FR / EN
- Textes centralises
- Aucune chaine metier en dur dans le code
