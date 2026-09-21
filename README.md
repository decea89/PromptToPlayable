# Prompt To Playable

## Minimal arena setup

The project includes `Tools > Prompt To Playable > Build Minimal Arena` in the Unity Editor.

Run this command to replace `Assets/Scenes/Arena.unity` with the prototype arena. It creates a dark, primitive-only URP arena with cyan floor accents, a player placeholder, a green extraction point, and three spawn markers. It also adds and bakes a `NavMeshSurface` that collects only the arena floor, providing the NavMesh required by the upcoming encounter validation.

The command is intentionally destructive to the contents of `Arena.unity`; confirm its dialog only when that scene may be replaced. AI Navigation `2.0.14` and URP `17.6.0` are already declared in `Packages/manifest.json`.

## Encounter data

Create an encounter asset from **Assets > Create > Prompt To Playable > Encounters > Encounter Definition**. Give it a display name, then add one or more enemy spawn entries. Each entry takes one enemy prefab and a count from 1 to 10. This asset is data-only: it does not refer to scene objects or spawn enemies by itself.
