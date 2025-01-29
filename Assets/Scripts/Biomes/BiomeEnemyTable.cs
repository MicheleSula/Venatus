using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BiomeEnemyTable", menuName = "Game Data/Biome Enemy Table")]
public class BiomeEnemyTable : ScriptableObject
{
    public string biomeName;
    public List<EnemySpawnOption> spawnOptions;
}