# StudyMatch — étape 1

Solution .NET 8 à trois projets : MVC (`StudyMatch.Mvc`), composants Blazor en Razor Class Library (`StudyMatch.Components`) et modèles (`StudyMatch.Models`). Données conservées en mémoire et initialisées en dur dans `StudyMatchStore`. Aucune base de données ni authentification.

## Démarrage

Installer le SDK .NET 8, puis lancer depuis ce dossier :

```bash
dotnet run --project StudyMatch.Mvc
```

Ouvrir l'URL HTTPS indiquée. La page `/groups` affiche les groupes; `/match` démarre l'assistant. Les composants de la RCL sont intégrés dans la vue MVC avec `<component>` et le rendu `ServerPrerendered`.

## Scénarios à essayer

- Cours `420-TWS`, lundi 9 h–11 h : rejoindre Alice et Bob; le local B-201 est conservé.
- Cours `420-BD`, jeudi 9 h–11 h : David est compatible; le groupe reçoit B-202.
- Cours `420-ALG`, mardi 9 h–11 h : rejoindre Chloé; B-202 est assigné car B-201 est occupé au même créneau.
- Un étudiant seul reste sans local jusqu'à l'arrivée d'un deuxième étudiant.

La mémoire est remise à zéro à chaque redémarrage; l'accès concurrent est protégé par un verrou. Deux groupes ne reçoivent jamais le même local au même créneau.
