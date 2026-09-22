# Prompt To Playable

A small Unity 6 prototype exploring how far a terminal-first, AI-assisted workflow can go when turning a simple game idea into a playable scene.

The goal was not to build a complete game. It was to test a focused question:

> Can a lightweight encounter prototype be created, validated, and iterated on through Unity CLI + Codex?

The result is a minimal playable arena containing:

- A player placeholder
- An extraction point
- Configurable enemy spawn locations
- Data-driven encounter definitions
- Runtime encounter spawning
- An in-Editor validation tool for checking the setup

## Why this exists

Most game prototypes begin with manual setup across scenes, GameObjects, components, and Inspector references. That is flexible, but repetitive setup can also be slow and error-prone.

This project explores a different workflow: describe the intended feature, use an AI-assisted terminal workflow to help create the implementation, and verify the result inside Unity.

The important part is not that AI generated code. The important part is that the output can be inspected, run, and validated in a real Unity project.

## What it does

The prototype creates a small arena with a player start, extraction zone, and enemy spawn points.

Encounter data lives in a reusable `EncounterDefinition` asset. An `EncounterSpawner` reads that asset at runtime and creates the configured enemies across the assigned spawn points.

The custom Inspector includes a **Validate Encounter** button. It checks the references and scene setup, including:

- Player and extraction references
- Encounter and enemy prefab entries
- Spawn-point configuration
- NavMesh placement
- Whether the player has a complete NavMesh route to extraction

The validator reports one of three states:

- `VALID`
- `VALID WITH WARNINGS`
- `INVALID`

It also visualizes valid points, invalid points, and the calculated route in the Scene view.

## Tech

- Unity 6
- Universal Render Pipeline (URP)
- AI Navigation / NavMesh
- Unity CLI
- Codex

## Project structure

```text
Assets/
  Editor/         # Arena builder and encounter validation tooling
  Scripts/        # Runtime spawning and encounter data
  Scenes/         # Prototype arena
```

## Try it

1. Open the project with the Unity version specified in `ProjectSettings/ProjectVersion.txt`.
2. Open `Assets/Scenes/Arena.unity`.
3. Select the GameObject that contains `EncounterSpawner`.
4. Assign an `EncounterDefinition`, player, extraction point, and spawn points.
5. Click **Validate Encounter** in the Inspector.
6. Enter Play mode to test runtime spawning.

> The menu command `Tools > Prompt To Playable > Build Minimal Arena` replaces `Assets/Scenes/Arena.unity`. Use it only when you are happy for that scene to be regenerated.

## Status

This is an experimental prototype, not a production-ready game framework.

It is intentionally small: the repository exists to document the workflow, show the resulting Unity setup, and provide a concrete starting point for further experiments.
