using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyOverworld : MonoBehaviour
{
    [Header("Dati del Nemico")]
    public CreatureData enemyData;
    public GameObject corpsePrefab;

    [Header("Riferimenti")]
    public BiomeSpawner spawnerReference;

    private bool isDead = false;

    public void Initialize(CreatureData data)
    {
        if (data == null)
        {
            Debug.LogWarning($"[EnemyOverworld] Initialize chiamato con dati nulli su {name}");
            return;
        }

        enemyData = data;
        ApplyDataToComponents();

        Debug.Log($"[EnemyOverworld] Inizializzato {name} con specie {enemyData.species.speciesName}");
        LogEnemyDetails();
    }

    private void ApplyDataToComponents()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && enemyData.sprite != null)
            spriteRenderer.sprite = enemyData.sprite;

        var creatureComponent = GetComponent<Creature>();
        if (creatureComponent != null)
        {
            creatureComponent.Initialize(enemyData);
        }
        else
        {
            Debug.LogWarning($"[EnemyOverworld] Nessun componente Creature trovato su {name}");
        }
    }

    private void LogEnemyDetails()
    {
        Debug.Log($"[EnemyOverworld] Statistiche della creatura: " +
                  $"ATK: {enemyData.species.baseAttack}, " +
                  $"DEF: {enemyData.species.baseDefense}, " +
                  $"SPD: {enemyData.species.baseSpeed}, ");

        if (enemyData.defaultBodyParts != null && enemyData.defaultBodyParts.Length > 0)
        {
            Debug.Log("[EnemyOverworld] Body Parts della creatura:");
            foreach (var part in enemyData.defaultBodyParts)
            {
                Debug.Log($"- {part.name}: " +
                          $"Healthy Sprite: {(part.healthySprite != null ? part.healthySprite.name : "Nessuno")}, " +
                          $"Damaged Sprite: {(part.damagedSprite != null ? part.damagedSprite.name : "Nessuno")}");
            }
        }
        else
        {
            Debug.LogWarning("[EnemyOverworld] Nessuna body part assegnata alla creatura.");
        }
    }

    public void ApplyBodyPartState()
    {
        var creatureComponent = GetComponent<Creature>();
        if (creatureComponent != null)
        {
            foreach (var part in creatureComponent.creatureData.defaultBodyParts)
            {
                if (part.IsDestroyed())
                {
                    Transform partTransform = transform.Find(part.name);
                    if (partTransform != null)
                    {
                        Destroy(partTransform.gameObject);
                        Debug.Log($"[EnemyOverworld] Body part {part.name} rimossa.");
                    }
                }
            }
        }
    }



    public void BecomeCorpse()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[EnemyOverworld] {name} è morto e diventa un cadavere");

        if (corpsePrefab != null)
        {
            GameObject corpse = Instantiate(corpsePrefab, transform.position, Quaternion.identity);
            var lootableCorpse = corpse.GetComponent<LootableCorpse>();

            if (lootableCorpse != null)
            {
                lootableCorpse.InitCorpse(enemyData, 360f, spawnerReference, true);
            }
        }

        Destroy(gameObject);
    }
}