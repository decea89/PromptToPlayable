using System.Collections.Generic;
using UnityEngine;

namespace PromptToPlayable.Encounters
{
    /// <summary>
    /// Instantiates the enemies requested by an encounter when this object starts.
    /// </summary>
    public sealed class EncounterSpawner : MonoBehaviour
    {
        [SerializeField]
        private EncounterDefinition encounterDefinition;

        [SerializeField]
        private Transform player;

        [SerializeField]
        private Transform extraction;

        [SerializeField]
        private List<Transform> spawnPoints = new();

        private void Start()
        {
            if (encounterDefinition == null)
            {
                Debug.LogWarning("EncounterSpawner has no EncounterDefinition assigned.", this);
                return;
            }

            List<Transform> availableSpawnPoints = GetAvailableSpawnPoints();
            if (availableSpawnPoints.Count == 0)
            {
                Debug.LogWarning("EncounterSpawner has no valid spawn points assigned.", this);
                return;
            }

            Transform spawnedEnemies = GetOrCreateSpawnedEnemiesRoot();
            int spawnPointIndex = 0;

            foreach (EnemySpawnEntry entry in encounterDefinition.EnemySpawns)
            {
                if (entry == null || entry.EnemyPrefab == null)
                {
                    Debug.LogWarning("EncounterSpawner skipped an enemy entry with no prefab assigned.", this);
                    continue;
                }

                int spawnCount = Mathf.Clamp(entry.Count, 1, 10);
                for (int enemyIndex = 0; enemyIndex < spawnCount; enemyIndex++)
                {
                    Transform spawnPoint = availableSpawnPoints[spawnPointIndex % availableSpawnPoints.Count];
                    Instantiate(entry.EnemyPrefab, spawnPoint.position, spawnPoint.rotation, spawnedEnemies);
                    spawnPointIndex++;
                }
            }
        }

        private List<Transform> GetAvailableSpawnPoints()
        {
            List<Transform> availableSpawnPoints = new();
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint == null)
                {
                    Debug.LogWarning("EncounterSpawner skipped a missing spawn point reference.", this);
                    continue;
                }

                availableSpawnPoints.Add(spawnPoint);
            }

            return availableSpawnPoints;
        }

        private Transform GetOrCreateSpawnedEnemiesRoot()
        {
            Transform spawnedEnemies = transform.Find("SpawnedEnemies");
            if (spawnedEnemies != null)
            {
                return spawnedEnemies;
            }

            GameObject spawnedEnemiesObject = new GameObject("SpawnedEnemies");
            spawnedEnemiesObject.transform.SetParent(transform);
            return spawnedEnemiesObject.transform;
        }
    }
}
