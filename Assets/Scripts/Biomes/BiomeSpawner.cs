using UnityEngine;
using System.Collections.Generic;

public class BiomeSpawner : MonoBehaviour
{
    [Header("Parametri di Spawn")]
    public float spawnInterval = 10f;
    public float spawnRadius = 20f;
    public int maxEnemiesAtOnce = 10;

    [Header("Tabelle Biomi")]
    public BiomeEnemyTable ForestaTable;
    public BiomeEnemyTable PaludeTable;
    public BiomeEnemyTable CimiteroTable;

    private float spawnTimer;
    private int currentEnemies;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.overworldPaused)
            return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && currentEnemies < maxEnemiesAtOnce)
        {
            spawnTimer = 0f;
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        if (PlayerController.Instance == null) return;

        Vector2 spawnPosition = GetRandomSpawnPosition();
        BiomeEnemyTable biomeTable = GetBiomeTableAtPosition(spawnPosition);

        if (biomeTable == null || biomeTable.spawnOptions.Count == 0) return;

        EnemySpawnOption spawnOption = SpawnUtility.WeightedRandomPick(biomeTable.spawnOptions);
        if (spawnOption == null || spawnOption.enemyPrefab == null || spawnOption.enemyData == null)
        {
            Debug.LogWarning("[BiomeSpawner] Opzione di spawn non valida o dati mancanti.");
            return;
        }

        SpawnEnemy(spawnOption, spawnPosition, biomeTable.biomeName);
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        return (Vector2)PlayerController.Instance.transform.position + randomOffset;
    }

    private BiomeEnemyTable GetBiomeTableAtPosition(Vector2 position)
    {
        if (position.x < 0) return ForestaTable;
        if (position.x < 100) return PaludeTable;
        return CimiteroTable;
    }

    private void SpawnEnemy(EnemySpawnOption option, Vector2 position, string biomeName)
    {
        Vector3 spawnPosition = new Vector3(position.x, position.y, 0f);
        GameObject enemyGO = Instantiate(option.enemyPrefab, spawnPosition, Quaternion.identity);
        enemyGO.name = $"{biomeName}_{option.enemyPrefab.name}";

        EnemyOverworld enemyComponent = enemyGO.GetComponent<EnemyOverworld>();
        if (enemyComponent != null)
        {
            enemyComponent.spawnerReference = this;
            enemyComponent.Initialize(option.enemyData);

            var skeletonController = enemyGO.GetComponent<EnemySkeletonController>();
            if (skeletonController != null)
            {
                skeletonController.InitializeSkeleton(option.enemyData);
            }

            Debug.Log($"[BiomeSpawner] Nemico spawnato: {enemyComponent.name}, specie: {option.enemyData.species.speciesName}");
        }

        OnEnemySpawned();
    }


    private void OnEnemySpawned()
    {
        currentEnemies++;
    }

    public void OnCorpseRemoved()
    {
        currentEnemies = Mathf.Max(0, currentEnemies - 1);
        Debug.Log($"[BiomeSpawner] Corpo rimosso. Nemici attivi: {currentEnemies}");
    }
}