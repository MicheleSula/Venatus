// CreatureSpawner.cs
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    public GameObject creaturePrefab;
    public Transform spawnPoint;
    public void SpawnCreature(CreatureData creatureData, Vector3 position)
    {
        if (creaturePrefab == null)
        {
            Debug.LogError("[CreatureSpawner] creaturePrefab non assegnato!");
            return;
        }

        GameObject creatureGO = Instantiate(creaturePrefab, position, Quaternion.identity);
        Creature creature = creatureGO.GetComponent<Creature>();
        if (creature != null)
        {
            creature.Initialize(creatureData);
            Debug.Log($"[CreatureSpawner] Creatura {creatureData.creatureName} spawnata a {position}.");
        }
        else
        {
            Debug.LogError("[CreatureSpawner] Il prefab assegnato non ha un componente Creature!");
        }
    }
}