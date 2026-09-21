using System.Collections.Generic;
using PromptToPlayable.Encounters;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace PromptToPlayable.Editor.Encounters
{
    [CustomEditor(typeof(EncounterSpawner))]
    public sealed class EncounterSpawnerEditor : UnityEditor.Editor
    {
        private const string StylePath = "Assets/Scripts/Editor/Encounters/EncounterSpawnerInspector.uss";

        private VisualElement validationPanel;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            validationPanel = new VisualElement();
            validationPanel.AddToClassList("validation-panel");
            root.Add(validationPanel);

            Button validateButton = new Button(RefreshValidation)
            {
                text = "Validate Encounter"
            };
            validateButton.AddToClassList("validate-button");
            root.Add(validateButton);

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            root.TrackSerializedObjectValue(serializedObject, _ => RefreshValidation());
            RefreshValidation();
            return root;
        }

        private void RefreshValidation()
        {
            if (validationPanel == null)
            {
                return;
            }

            EncounterValidationReport report = EncounterSpawnerValidator.Validate((EncounterSpawner)target);
            validationPanel.Clear();

            Label status = new Label(report.StatusLabel);
            status.AddToClassList("validation-status");
            status.AddToClassList($"status-{report.StatusClass}");
            validationPanel.Add(status);

            foreach (EncounterValidationResult result in report.Results)
            {
                Label resultLabel = new Label($"{result.SeverityLabel}: {result.Message}");
                resultLabel.AddToClassList("validation-result");
                resultLabel.AddToClassList($"severity-{result.SeverityClass}");
                validationPanel.Add(resultLabel);
            }
        }
    }

    internal static class EncounterSpawnerValidator
    {
        internal const float NavMeshSampleDistance = 0.5f;

        internal static EncounterValidationReport Validate(EncounterSpawner spawner)
        {
            EncounterSpawnerReferences references = EncounterSpawnerReferences.Read(spawner);
            List<EncounterValidationResult> results = new();

            if (references.EncounterDefinition == null)
            {
                results.Add(EncounterValidationResult.Error("EncounterDefinition is missing."));
            }

            if (references.Player == null)
            {
                results.Add(EncounterValidationResult.Error("Player Transform is missing."));
            }

            if (references.Extraction == null)
            {
                results.Add(EncounterValidationResult.Error("Extraction Transform is missing."));
            }

            if (references.SpawnPoints.Count == 0)
            {
                results.Add(EncounterValidationResult.Error("No spawn points are assigned."));
            }

            int validSpawnPointCount = 0;
            for (int index = 0; index < references.SpawnPoints.Count; index++)
            {
                Transform spawnPoint = references.SpawnPoints[index];
                if (spawnPoint == null)
                {
                    results.Add(EncounterValidationResult.Warning($"Spawn point entry {index + 1} is null."));
                    continue;
                }

                validSpawnPointCount++;
            }

            int requestedEnemyCount = ValidateEnemyEntries(references.EncounterDefinition, results);
            if (validSpawnPointCount > 0 && validSpawnPointCount < requestedEnemyCount)
            {
                results.Add(EncounterValidationResult.Warning(
                    $"Only {validSpawnPointCount} valid spawn points are assigned for {requestedEnemyCount} requested enemies."));
            }

            bool hasBakedNavMesh = HasBakedNavMesh();
            if (!hasBakedNavMesh)
            {
                results.Add(EncounterValidationResult.Error("No baked NavMesh can be sampled in the scene."));
            }
            else
            {
                ValidateNavMeshPositions(references, results);
                ValidatePath(references, results);
            }

            results.Add(EncounterValidationResult.Info("Validation completed."));
            return new EncounterValidationReport(results);
        }

        internal static bool IsOnNavMesh(Transform transform)
        {
            return transform != null && NavMesh.SamplePosition(
                transform.position,
                out _,
                NavMeshSampleDistance,
                NavMesh.AllAreas);
        }

        internal static bool TryCalculateCompletePath(Transform player, Transform extraction, out NavMeshPath path)
        {
            path = new NavMeshPath();
            return player != null
                && extraction != null
                && NavMesh.CalculatePath(player.position, extraction.position, NavMesh.AllAreas, path)
                && path.status == NavMeshPathStatus.PathComplete;
        }

        private static int ValidateEnemyEntries(EncounterDefinition definition, List<EncounterValidationResult> results)
        {
            if (definition == null)
            {
                return 0;
            }

            int requestedEnemyCount = 0;
            for (int index = 0; index < definition.EnemySpawns.Count; index++)
            {
                EnemySpawnEntry entry = definition.EnemySpawns[index];
                if (entry == null || entry.EnemyPrefab == null)
                {
                    results.Add(EncounterValidationResult.Error($"Enemy entry {index + 1} has no prefab assigned."));
                    continue;
                }

                if (entry.Count < 1)
                {
                    results.Add(EncounterValidationResult.Error($"Enemy entry {index + 1} has a count below 1."));
                    continue;
                }

                requestedEnemyCount += entry.Count;
            }

            return requestedEnemyCount;
        }

        private static bool HasBakedNavMesh()
        {
            return NavMesh.CalculateTriangulation().vertices.Length > 0;
        }

        private static void ValidateNavMeshPositions(EncounterSpawnerReferences references, List<EncounterValidationResult> results)
        {
            for (int index = 0; index < references.SpawnPoints.Count; index++)
            {
                Transform spawnPoint = references.SpawnPoints[index];
                if (spawnPoint != null && !IsOnNavMesh(spawnPoint))
                {
                    results.Add(EncounterValidationResult.Error($"Spawn point entry {index + 1} is not on or near the NavMesh."));
                }
            }

            if (references.Extraction != null && !IsOnNavMesh(references.Extraction))
            {
                results.Add(EncounterValidationResult.Error("Extraction point is not on or near the NavMesh."));
            }
        }

        private static void ValidatePath(EncounterSpawnerReferences references, List<EncounterValidationResult> results)
        {
            if (references.Player == null || references.Extraction == null)
            {
                return;
            }

            if (!TryCalculateCompletePath(references.Player, references.Extraction, out _))
            {
                results.Add(EncounterValidationResult.Error("No complete NavMesh path exists from player to extraction."));
            }
        }
    }

    internal readonly struct EncounterSpawnerReferences
    {
        internal EncounterDefinition EncounterDefinition { get; }
        internal Transform Player { get; }
        internal Transform Extraction { get; }
        internal List<Transform> SpawnPoints { get; }

        private EncounterSpawnerReferences(EncounterDefinition encounterDefinition, Transform player, Transform extraction, List<Transform> spawnPoints)
        {
            EncounterDefinition = encounterDefinition;
            Player = player;
            Extraction = extraction;
            SpawnPoints = spawnPoints;
        }

        internal static EncounterSpawnerReferences Read(EncounterSpawner spawner)
        {
            SerializedObject serializedSpawner = new SerializedObject(spawner);
            SerializedProperty spawnPointsProperty = serializedSpawner.FindProperty("spawnPoints");
            List<Transform> spawnPoints = new();

            for (int index = 0; index < spawnPointsProperty.arraySize; index++)
            {
                spawnPoints.Add(spawnPointsProperty.GetArrayElementAtIndex(index).objectReferenceValue as Transform);
            }

            return new EncounterSpawnerReferences(
                serializedSpawner.FindProperty("encounterDefinition").objectReferenceValue as EncounterDefinition,
                serializedSpawner.FindProperty("player").objectReferenceValue as Transform,
                serializedSpawner.FindProperty("extraction").objectReferenceValue as Transform,
                spawnPoints);
        }
    }

    internal sealed class EncounterValidationReport
    {
        internal IReadOnlyList<EncounterValidationResult> Results { get; }

        internal EncounterValidationReport(IReadOnlyList<EncounterValidationResult> results)
        {
            Results = results;
        }

        internal string StatusLabel => HasErrors ? "INVALID" : HasWarnings ? "VALID WITH WARNINGS" : "VALID";
        internal string StatusClass => HasErrors ? "invalid" : HasWarnings ? "warning" : "valid";

        private bool HasErrors => HasSeverity(EncounterValidationSeverity.Error);
        private bool HasWarnings => HasSeverity(EncounterValidationSeverity.Warning);

        private bool HasSeverity(EncounterValidationSeverity severity)
        {
            foreach (EncounterValidationResult result in Results)
            {
                if (result.Severity == severity)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal sealed class EncounterValidationResult
    {
        internal EncounterValidationSeverity Severity { get; }
        internal string Message { get; }
        internal string SeverityLabel => Severity.ToString();
        internal string SeverityClass => Severity.ToString().ToLowerInvariant();

        private EncounterValidationResult(EncounterValidationSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        internal static EncounterValidationResult Error(string message) => new(EncounterValidationSeverity.Error, message);
        internal static EncounterValidationResult Warning(string message) => new(EncounterValidationSeverity.Warning, message);
        internal static EncounterValidationResult Info(string message) => new(EncounterValidationSeverity.Info, message);
    }

    internal enum EncounterValidationSeverity
    {
        Error,
        Warning,
        Info
    }

    internal static class EncounterSpawnerGizmos
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        private static void DrawWhenSelected(EncounterSpawner spawner, GizmoType gizmoType)
        {
            EncounterSpawnerReferences references = EncounterSpawnerReferences.Read(spawner);
            bool hasBakedNavMesh = NavMesh.CalculateTriangulation().vertices.Length > 0;

            foreach (Transform spawnPoint in references.SpawnPoints)
            {
                if (spawnPoint == null)
                {
                    continue;
                }

                Gizmos.color = hasBakedNavMesh && EncounterSpawnerValidator.IsOnNavMesh(spawnPoint)
                    ? Color.green
                    : Color.red;
                Gizmos.DrawSphere(spawnPoint.position + Vector3.up * 0.15f, 0.25f);
            }

            if (references.Extraction != null)
            {
                Gizmos.color = hasBakedNavMesh && EncounterSpawnerValidator.IsOnNavMesh(references.Extraction)
                    ? Color.green
                    : Color.red;
                Gizmos.DrawWireSphere(references.Extraction.position, 0.7f);
            }

            if (references.Player == null || references.Extraction == null)
            {
                return;
            }

            if (hasBakedNavMesh && EncounterSpawnerValidator.TryCalculateCompletePath(references.Player, references.Extraction, out NavMeshPath path))
            {
                Gizmos.color = Color.cyan;
                for (int index = 1; index < path.corners.Length; index++)
                {
                    Gizmos.DrawLine(path.corners[index - 1], path.corners[index]);
                }
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(references.Player.position, references.Extraction.position);
            }
        }
    }
}
