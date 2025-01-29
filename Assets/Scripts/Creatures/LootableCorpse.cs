using UnityEngine;

public class LootableCorpse : MonoBehaviour
{
    public float autoDecayTime = 360f;
    public float postLootTime = 180f;
    public bool hasLoot = true;

    private float timer = 0f;
    private bool isLooted = false;
    private BiomeSpawner spawnerRef;

    public CreatureData corpseData;

    public void InitCorpse(CreatureData data, float decayTime, BiomeSpawner sp, bool loot)
    {
        corpseData = data;
        autoDecayTime = decayTime;
        spawnerRef = sp;
        hasLoot = loot;

        Debug.Log($"[LootableCorpse] InitCorpse on {name}, spawnerRef={(spawnerRef ? spawnerRef.name : "NULL")}, autoDecayTime={autoDecayTime}");
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (!isLooted && timer >= autoDecayTime)
        {
            Debug.Log($"[LootableCorpse] autoDecay scaduto => CleanupCorpse()");
            CleanupCorpse();
        }
        else if (isLooted && timer >= postLootTime)
        {
            Debug.Log($"[LootableCorpse] postLootTime scaduto => CleanupCorpse()");
            CleanupCorpse();
        }
    }

    private void CleanupCorpse()
    {
        Debug.Log($"[LootableCorpse] CleanupCorpse -> Chiamo CorpseRemoved() su spawnerRef={(spawnerRef ? spawnerRef.name : "NULL")}");
        spawnerRef?.OnCorpseRemoved();
        Destroy(gameObject);
    }

    public void OnPlayerInteract()
    {
        if (!isLooted && hasLoot)
        {
            Debug.Log("Loot preso dal cadavere!");
            PlayerInventory.Instance.AddItem(corpseData.lootItem, corpseData.lootQuantity);

            hasLoot = false;
            isLooted = true;
            timer = 0f;
        }
    }
}