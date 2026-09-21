using System;
using System.Collections.Generic;
using UnityEngine;

namespace PromptToPlayable.Encounters
{
    /// <summary>
    /// Designer-authored data describing the enemies that belong to one encounter.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EncounterDefinition",
        menuName = "Prompt To Playable/Encounters/Encounter Definition",
        order = 0)]
    public sealed class EncounterDefinition : ScriptableObject
    {
        [SerializeField]
        private string displayName = "New Encounter";

        [SerializeField]
        private List<EnemySpawnEntry> enemySpawns = new();

        public string DisplayName => displayName;
        public IReadOnlyList<EnemySpawnEntry> EnemySpawns => enemySpawns;
    }

    /// <summary>
    /// One enemy prefab and the number of instances requested by an encounter.
    /// </summary>
    [Serializable]
    public sealed class EnemySpawnEntry
    {
        [SerializeField]
        private GameObject enemyPrefab;

        [SerializeField, Range(1, 10)]
        private int count = 1;

        public GameObject EnemyPrefab => enemyPrefab;
        public int Count => count;
    }
}
