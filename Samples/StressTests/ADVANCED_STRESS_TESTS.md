# Documentation — Advanced ArmoniK Stress Tests


## 1) Vue d'ensemble et objectifs
Les nouveaux stress tests permettent d'exécuter une suite complète de scénarios sur une plateforme ArmoniK (API Unified) afin de mesurer :
- le throughput (tâches / s) en soumission et en exécution,
- la latence / durée d'exécution,
- le taux de succès / échecs / tâches manquantes,
- et de produire des rapports JSON détaillés par exécution (format machine-friendly).

Ils visent à être reproductibles, paramétrables depuis un script shell (`run-stress-tests.sh`) et robustes : comptage thread-safe, timeout et reporting centralisé.

---

## 2) Composants clés
- `tools/tests/run-stress-tests.sh`
  - Runner bash : parse les options, construit le projet (.NET), configure l'endpoint, lance `dotnet run` vers le client et gère la création de fichier de rapport (option `--report`).
  - Commandes : `basic` (un scénario simple) et `advanced` (suite complète multi-scénarios).

- `Samples/StressTests/ArmoniK.Samples.StressTests.Client/AdvancedStressTests.cs`
  - Suite avancée orchestrant les scénarios, soumissions via `Service.Submit(...)`, collecte des résultats via `AdvancedResultHandler`, et génération de rapports JSON.

- `Samples/StressTests/ArmoniK.Samples.StressTests.Client/StressTests.cs`
  - Fonctions utilitaires et primitives de soumission (payload helper, potentiellement réutilisées par `AdvancedStressTests`).

- Fichiers de sortie
  - `./stress-test-reports/<timestamp>/comprehensive-report.json`
  - `./stress-test-reports/<timestamp>/test-<Scenario>-<id>.json`

Note importante : la version actuelle du runner `advanced` **ignore les options par-scenario** passées en ligne de commande (les scénarios sont définis dans le client) et produit uniquement des rapports JSON (pas d'HTML).

---

## 3) Flux d'exécution (résumé)
1. Lancer le runner : `./tools/tests/run-stress-tests.sh basic|advanced [options]`.
2. Le script vérifie `dotnet`, trouve le projet client, build (`dotnet build`), puis exécute le client (`dotnet run -- ...`).
3. Le client instancie `AdvancedStressTests` : construit `TaskOptions` et `Properties`, crée `Service` et un répertoire de rapport horodaté.
4. Pour chaque scénario :
   - Crée un `AdvancedResultHandler` dédié.
   - Soumet `nbTasks` via `Service.Submit("ComputeWorkLoad", params..., resultHandler)` — retourne une liste de task IDs.
   - `AdvancedResultHandler` stocke `SubmittedTaskIds` et attend les callbacks `HandleResponse`/`HandleError`.
   - Attente active (polling) jusqu'à ce que `NbResults + NbErrors >= nbTasks` ou timeout.
   - Calcul des métriques et sauvegarde du rapport individuel.
5. Après tous les scénarios, génération d'un `comprehensive-report.json`.

## 3.1) Scénarios avancés — que se passe-t-il concrètement

Chaque scénario avancé implémente un pattern de soumission et d'attente spécifique. Ci-dessous le déroulé concret pour chacun :

- HighVolumeQuick ("HighVolumeQuick")
  - Objectif : mesurer la capacité de soumission et d'exécution sur un grand nombre de petites tâches.
  - Ce qui se passe : le client construit des petits payloads (ex : 512 KB d'entrée, sortie 8 KB) et soumet `nbTasks` (par défaut 5000) en une seule passe via `Service.Submit`.
  - Pattern : soumission continue et rapide (pas de pause entre les tâches). Le `AdvancedResultHandler` enregistre tous les task IDs soumis et attend les callbacks.
  - Résultats attendus : forte charge de soumission, throughput de soumission élevé ; la latence par tâche doit rester faible si les workers suivent.

- MediumVolumeStandard ("MediumVolumeStandard")
  - Objectif : scénario représentatif d'une charge moyenne (workload standard).
  - Ce qui se passe : payloads plus gros (exemple 1 024 KB) ou workload un peu plus long ; `nbTasks` autour de 2000.
  - Pattern : soumission soutenue, mesure des temps de soumission/exécution séparés pour distinguer client vs workers.
  - Résultats attendus : indicateur intermédiaire de stabilité et de capacité de traitement normal.

- LowVolumeHeavy ("LowVolumeHeavy")
  - Objectif : tester la robustesse des workers et la gestion des payloads lourds ou calculs coûteux.
  - Ce qui se passe : payloads volumineux (ex : 2 048 KB) et workload long (par ex. 1000 ms) ; nombre de tâches faible (ex : 500).
  - Pattern : soumission rapide mais le temps d'exécution par tâche est élevé ; `AdvancedResultHandler` observe des exécutions longues et agrège les résultats numériques.
  - Résultats attendus : throttling côté workers possible, échecs si ressources insuffisantes ; surveiller `FailedTasks` et `ExecutionDuration`.

- BurstLoad ("BurstLoad")
  - Objectif : simuler des pics — plusieurs rafales de soumissions successives.
  - Ce qui se passe : le test soumet `burstCount` rafales de `tasksPerBurst` tâches (exemple 3 × 1000). Entre chaque rafale une petite pause (ex: 1s) est faite.
  - Pattern : séries de soumissions massives suivies de courtes périodes d'attente ; bon pour mesurer la capacité à encaisser et retomber en charge.
  - Résultats attendus : mesurer how the system absorbs bursts (buffering, queueing), repérer pertes ou erreurs lors des pics.

- Endurance ("Endurance")
  - Objectif : évaluer la stabilité sur longue durée, détecter fuites mémoire, dégradation progressive ou instabilité.
  - Ce qui se passe : un nombre moyen/élevé de tâches avec des durées longues (ex : 1000 tâches, workload 5000 ms) exécutées de façon continue.
  - Pattern : longue période d'exécution avec accumulation progressive de métriques ; le timeout de 15 minutes s'applique par scénario mais peut être ajusté.
  - Résultats attendus : vérifier l'absence de dégradation (drop de throughput, erreurs croissantes) et l'utilisation mémoire/CPU stable.

Pour chaque scénario, le code suit le même cycle concret :
1. Construction du payloads et paramètres.
2. Appel à `Service.Submit(...)` qui renvoie la liste des task IDs.
3. Stockage des task IDs dans `resultHandler.SubmittedTaskIds`.
4. Les workers exécutent et appellent `HandleResponse` ou `HandleError` sur le handler.
5. Le handler incrémente `NbResults` / `NbErrors` et marque les IDs reçus.
6. La boucle `WaitForResult` attend que `NbResults + NbErrors >= nbTasks` ou que le timeout soit atteint.
7. Calcul des métriques, sauvegarde du rapport individuel et ajout au rapport complet.


Conseils pratiques : pour reproduire une situation précise, ajustez `--tasks`, `--workload-ms`, `--payload-kb`, `--tasks-per-buffer` et `--buffers-per-channel` côté runner.

## 3.2) Paramètres recommandés par scénario (exemples)

Le tableau ci-dessous propose des jeux de paramètres typiques utilisés pour chaque scénario. Ce sont des exemples pour comprendre la charge et reproduire localement ; la suite `advanced` a ses scénarios définis en dur dans le client.

| Scénario | nbTasks (ex.) | payload-kB | output-kB | workload-ms | tasks/buffer | buffers/channel | channels | Notes |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| HighVolumeQuick | 5000 | 512 | 8 | 10 | 50 | 5 | 5 | Forte soumission de petites tâches — mesure du throughput |
| MediumVolumeStandard | 2000 | 1024 | 8 | 100 | 50 | 5 | 5 | Scénario représentatif, charge moyenne |
| LowVolumeHeavy | 500 | 2048 | 8 | 1000 | 10 | 2 | 2 | Payloads lourds / calculs coûteux — surveiller erreurs et latence |
| BurstLoad (rafales) | 3×1000 (burstCount×tasksPerBurst) | 512 | 8 | 50 | 50 | 5 | 5 | Séries de pics; utile pour tester l'absorption par buffers/queues |
| Endurance | 1000 | 1024 | 8 | 5000 | 50 | 5 | 2 | Longue durée — surveiller stabilité et fuites mémoire |

> Remarque : ces valeurs sont des suggestions. La commande `advanced` du runner ignore les options par-scenario (les paramètres ci-dessus servent de guide pour tests manuels ou pour modifier directement le code client si besoin).


---

## 4) Contrat : Entrées / Sorties / Erreurs
- Entrées (exemples & types)
  - nbTasks : int (nombre de tâches à soumettre)
  - nbInputBytes : long (taille du payload en octets)
  - nbOutputBytes : long (taille de sortie demandée)
  - workLoadTimeInMs : int (durée de travail demandée en ms)
  - Options client : partition, nbTaskPerBuffer, nbBufferPerChannel, nbChannel

- Sorties
  - `TestResult` (pour chaque scénario) : TestId, ScenarioName, TaskCount, SubmissionDuration, ExecutionDuration, SuccessfulTasks, FailedTasks, MissingTasks, SuccessRate, Throughput (submission + execution), etc.
  - `ComprehensiveReport` (suite) : SuiteId, Start/End, Environment, TestResults[], Summary (totaux, moyennes, pic).

- Erreurs / modes d'échec
  - `ServiceInvocationException` remonté via `HandleError` pour tâches individuelles
  - Timeout par scénario si `NbResults + NbErrors < nbTasks` après 15 minutes (configurable dans le code)
  - Exception générale loggée et rethrow pour interrompre la suite

---

## 5) Détails d'implémentation importants
### 5.1 `Service.Submit(...)`
- Utiliser `Service.Submit` (qui renvoie les task IDs et déclenche les callbacks) plutôt que `SubmitAsync` si vous attendez des callbacks synchrones.
- `SubmitTasks(...)` construit le payload (via `Utils.ParamsHelper`) et appelle `Service.Submit(...)`, puis associe `SubmittedTaskIds` dans le `resultHandler`.

### 5.2 `AdvancedResultHandler`
- Thread-safe : `nbResults_` et `nbErrors_` mis à jour via `Interlocked.Increment`.
- `receivedTaskIds_` est un `ConcurrentDictionary<string, byte>` pour marquer les réponses reçues.
- Méthodes :
  - `HandleResponse(object response, string taskId)` : accumule les valeurs (cas `double[]`), marque le `taskId` reçu et incrémente `nbResults_`.
  - `HandleError(ServiceInvocationException e, string taskId)` : marque le `taskId` s'il existe, incrémente `nbErrors_`, logge les erreurs sauf les cancellations.
  - `WaitForResult(int nbTasks, CancellationToken token)` : boucle de polling (500ms) jusqu'à `NbResults + NbErrors >= nbTasks` ou timeout (15min).
  - `GetMissingIds()` : retourne les IDs soumis qui n'ont pas été reçus, utile pour diagnostics.

### 5.3 Timeout
- Timeout fixe par scénario : 15 minutes (modifiable si nécessaire).
- En cas de timeout, le logger signale combien de résultats ont été reçus.

### 5.4 Rapports
- JSON : structure `ComprehensiveReport` sérialisée avec indentation.
- HTML : fichier stylé (CSS inline) généré dynamiquement reprenant les métriques.

---

## 6) Commandes d'exécution & exemples
- Construire et lancer un test simple (basic) :

```bash
# basic (paramètres par défaut sauf override)
./tools/tests/run-stress-tests.sh basic --tasks 100 --workload-ms 10 --report ./reports/basic.json
```

Lancer la suite avancée : notez que le runner `advanced` **ignore** les options par-scenario (les scénarios et leurs paramètres sont définis dans le client). Vous pouvez définir le chemin du rapport JSON via `--report`. Exemple :

```bash
./tools/tests/run-stress-tests.sh advanced --report ./reports/advanced.json
```

- Exemples de `--report` :
  - `--report ./reports/out.json` -> utilise le chemin fourni
  - `--report` seul (si runner configuré pour auto-générer) -> crée `./reports/basic-<timestamp>.json` ou `advanced-<timestamp>.json`

> Note : selon la version du script le flag `--report` peut exiger un argument ou créer un fichier par défaut. Vérifier la version actuelle de `run-stress-tests.sh`.

---

## 7) Métriques et comment les interpréter
- SubmissionThroughput = nbTasks / SubmissionDuration.TotalSeconds
  - Mesure la vitesse à laquelle le client peut soumettre des tâches.
- ExecutionThroughput = nbTasks / ExecutionDuration.TotalSeconds
  - Mesure la vitesse visible côté résultats (workers + réseau + orchestration).
- SuccessRate = SuccessfulTasks / TaskCount * 100
  - Si < 100%, investiguer `FailedTasks` et `MissingTasks`.
- MissingTasks = TaskCount - (NbResults + NbErrors)
  - Signale des IDs qui n'ont pas déclenché de callback. Utiliser `GetMissingIds()` pour lister.

---

## 8) Diagnostic & checklist rapide (si problèms)
1. Vérifier que le client pointe vers le bon endpoint (`--endpoint` ou variable par défaut). Control plane reachable.
2. Si résultats manquants :
   - Vérifier que `Service.Submit` est utilisé (et non `SubmitAsync`) si les callbacks ne sont pas reçus.
   - Inspecter les logs du Control Plane et des workers (rechercher exceptions, timeouts, cancellations).
   - Récupérer `GetMissingIds()` depuis les rapports individuels et corréler avec worker logs.
3. Si beaucoup d'échecs : vérifier `FailedTasks` et les messages dans `AdvancedResultHandler.HandleError` (status code, message).
4. Si la soumission est lente : vérifier consommation CPU/RAM réseau, et si KEDA scale-to-zero provoque cold starts.
5. Pour les problèmes intermittents, exécuter plusieurs runs courts (basic) pour reproduire.

---

## 9) Cas limites et recommandations
- Volume très élevé (>100k tasks) : attention usage mémoire côté client (stockage des task IDs). Envisager une stratégie d'archivage/streaming pour tracking.
- Callbacks non reçus : ajouter métriques logs côté workers et enable tracing gRPC.
- Timeout global : rendre la durée configurable via CLI (`--timeout`) utile pour tests endurance.

---

## 10) Mapping rapide des fichiers à consulter
- Runner & utilitaires : `tools/tests/run-stress-tests.sh`
- Client advanced : `Samples/StressTests/ArmoniK.Samples.StressTests.Client/AdvancedStressTests.cs`
- Base helpers : `Samples/StressTests/ArmoniK.Samples.StressTests.Client/StressTests.cs`
- Reports produit : `./stress-test-reports/<timestamp>/` (json + html + tests)

---

## 11) Suggestions d'améliorations / futures itérations
- Rendre le timeout par scénario configurable via CLI.
- Option `--dry-run` pour builder et afficher la commande dotnet sans exécution.
- Add charts JS (Chart.js) dans l'HTML pour visualiser throughput / success rate.
- Export des `GetMissingIds()` par scénario dans un fichier `missing-<scenario>-<id>.txt` pour faciliter corrélation logs.
- Ajout de tests unitaires pour `AdvancedResultHandler` simulant callbacks success / error.

---

## 12) Exemple minimal d'un fragment JSON de `TestResult`
```json
{
  "TestId": "a1b2c3d4",
  "ScenarioName": "HighVolumeQuick",
  "TaskCount": 5000,
  "SuccessfulTasks": 5000,
  "FailedTasks": 0,
  "MissingTasks": 0,
  "SuccessRate": 100.0,
  "SubmissionThroughput": 1200.5,
  "ExecutionThroughput": 1150.2
}
```

