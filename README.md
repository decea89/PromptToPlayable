# Prompt To Playable

## Minimal arena setup

The project includes `Tools > Prompt To Playable > Build Minimal Arena` in the Unity Editor.

Run this command to replace `Assets/Scenes/Arena.unity` with the prototype arena. It creates a dark, primitive-only URP arena with cyan floor accents, a player placeholder, a green extraction point, and three spawn markers. It also adds and bakes a `NavMeshSurface` that collects only the arena floor, providing the NavMesh required by the upcoming encounter validation.

The command is intentionally destructive to the contents of `Arena.unity`; confirm its dialog only when that scene may be replaced. AI Navigation `2.0.14` and URP `17.6.0` are already declared in `Packages/manifest.json`.

## Encounter data

Create an encounter asset from **Assets > Create > Prompt To Playable > Encounters > Encounter Definition**. Give it a display name, then add one or more enemy spawn entries. Each entry takes one enemy prefab and a count from 1 to 10. This asset is data-only: it does not refer to scene objects or spawn enemies by itself.

## Runtime encounter spawning

Add `EncounterSpawner` to an encounter GameObject, then assign its `EncounterDefinition`, player, extraction point, and ordered spawn points. At `Start`, it creates a `SpawnedEnemies` child and instantiates each configured entry in round-robin spawn-point order. Entries without a prefab and missing spawn-point references are logged and skipped safely.

## Validation tool

Select the GameObject with `EncounterSpawner` and click **Validate Encounter** in its Inspector. The status box reports **VALID**, **VALID WITH WARNINGS**, or **INVALID**, followed by each finding. It checks required references and enemy entries, spawn-point counts and NavMesh placement, the extraction point, and whether the player has a complete NavMesh path to extraction. When selected, the Scene view shows valid spawn/extraction points in green, invalid ones in red, and the calculated route in cyan (or red when no complete route exists).
