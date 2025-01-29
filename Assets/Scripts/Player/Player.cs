using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Player : Creature
{
    public event System.Action OnStatsChanged;
    private bool statsDirty = false;

    public override void Initialize(CreatureData data, int? savedHealth = null)
    {
        base.Initialize(data, savedHealth);

        InitializeStats();
        InitializeBodyPartGameObjects();

        if (!savedHealth.HasValue)
        {
            CurrentHealth = MaxHealth;
        }

        finalStats.maxHealth = MaxHealth;

        Debug.Log($"[Player] Inizializzato con {CurrentHealth}/{MaxHealth} HP.");
    }

    private void Awake()
    {
        if (bodyParent != null)
        {
            DontDestroyOnLoad(bodyParent.gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeBodyPartGameObjects()
    {
        if (bodyParent != null)
    {
        bodyParent.SetParent(this.transform);
        bodyParent.localPosition = Vector3.zero;
    }
    else
    {
        GameObject parent = new GameObject("PlayerBodyParts");
        parent.transform.SetParent(this.transform);
        parent.transform.localPosition = Vector3.zero;
        bodyParent = parent.transform;
    }


        foreach (var part in bodyPartSlots)
        {
            if (part.Value != null && part.Value.gameObject == null && !bodyPartGameObjects.ContainsKey(part.Key))
            {
                // Crea il GameObject per la parte del corpo
                GameObject partGO = new GameObject(part.Key);
                partGO.transform.SetParent(bodyParent);
                partGO.transform.localPosition = Vector3.zero;

                // Aggiungi SpriteRenderer
                SpriteRenderer spriteRenderer = partGO.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = part.Value.basePart.healthySprite;

                // Assegna il GameObject alla parte del corpo
                part.Value.gameObject = partGO;

                // Aggiungi al dizionario
                bodyPartGameObjects[part.Key] = partGO;

                Debug.Log($"[Player] Body part GameObject creato: {part.Key}");
            }
            else if (part.Value == null)
            {
                Debug.LogWarning($"[Player] Body part {part.Key} è null e non può essere inizializzata.");
            }
        }
    }

    public override void EquipBodyPart(string slotName, BodyPartItem bodyPart)
    {
        base.EquipBodyPart(slotName, bodyPart);
        InitializeBodyPartGameObject(slotName);
    }

    private void InitializeBodyPartGameObject(string slotName)
    {
        if (!bodyPartSlots.ContainsKey(slotName))
        {
            Debug.LogWarning($"[Player] Lo slot '{slotName}' non esiste!");
            return;
        }

        var partInstance = bodyPartSlots[slotName];
        if (partInstance != null && !bodyPartGameObjects.ContainsKey(slotName))
        {
            // Crea il GameObject per la nuova parte del corpo
            GameObject partGO = new GameObject(slotName);
            partGO.transform.SetParent(bodyParent);
            partGO.transform.localPosition = Vector3.zero;

            // Aggiungi SpriteRenderer
            SpriteRenderer spriteRenderer = partGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = partInstance.basePart.healthySprite;

            // Assegna il GameObject alla parte del corpo
            partInstance.gameObject = partGO;

            // Aggiungi al dizionario
            bodyPartGameObjects[slotName] = partGO;

            Debug.Log($"[Player] Body part GameObject creato: {slotName}");
        }
    }

    public void MarkStatsAsDirty()
    {
        statsDirty = true;
    }

    public void InitializeStats()
    {
        if (!statsDirty) return;

        ResetStats();
        ApplyEquipmentModifiers();
        ApplyBodyPartModifiers();

        statsDirty = false;

        OnStatsChanged?.Invoke();

        Debug.Log($"[Player] Statistiche aggiornate: {finalStats}");
    }

    private void ResetStats()
    {
        finalStats.Reset();

        finalStats.attack = creatureData.species.baseAttack + creatureData.family.baseAttackModifier + creatureData.composition.compositionAttackMod;
        finalStats.defense = creatureData.species.baseDefense + creatureData.family.baseDefenseModifier + creatureData.composition.compositionDefenseMod;
        finalStats.speed = creatureData.species.baseSpeed + creatureData.family.baseSpeedModifier + creatureData.composition.compositionSpeedMod;
        finalStats.maxHealth = TotalHP;
    }

    private void ApplyEquipmentModifiers()
    {
        foreach (var equipment in PlayerInventory.Instance.equippedItems.Values)
        {
            if (equipment != null)
            {
                finalStats.attack += equipment.attackModifier;
                finalStats.defense += equipment.defenseModifier;
                finalStats.speed += equipment.speedModifier;
                finalStats.maxHealth += equipment.healthModifier;
            }
        }
    }

    private void ApplyBodyPartModifiers()
    {
        foreach (var partInstance in bodyPartSlots.Values)
        {
            if (partInstance != null && !partInstance.IsDestroyed())
            {
                finalStats.attack += partInstance.basePart.attackModifier;
                finalStats.defense += partInstance.basePart.defenseModifier;
                finalStats.speed += partInstance.basePart.speedModifier;
            }
        }
    }

    // TODO SALVATAGGIO DEGLI STATI DEL PLAYER ###############################################################

    public PlayerSaveData CreateSaveData()
    {
        PlayerSaveData saveData = new PlayerSaveData();
        saveData.creatureName = creatureData.creatureName;
        saveData.currentHP = CurrentHealth;
        saveData.bodyParts = new List<BodyPartSaveData>();

        foreach (var part in bodyPartSlots)
        {
            BodyPartSaveData partSave = new BodyPartSaveData
            {
                slotName = part.Key,
                bodyPartName = part.Value != null ? part.Value.basePart.partName : "None",
                currentHealth = part.Value != null ? part.Value.currentHealth : 0
            };
            saveData.bodyParts.Add(partSave);
        }

        // Salva l'inventario
        if (inventory != null)
        {
            saveData.inventoryData = inventory.CreateSaveData();
        }
        else
        {
            saveData.inventoryData = new List<InventoryItemSaveData>();
        }

        return saveData;
    }

    // Metodo per caricare i dati del Player dal salvataggio
    public void LoadFromSaveData(PlayerSaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogWarning("[Player] Tentativo di caricare dai dati di salvataggio null.");
            return;
        }

        // Carica CreatureData dal Resources
        creatureData = Resources.Load<CreatureData>(saveData.creatureName);
        if (creatureData == null)
        {
            Debug.LogError($"[Player] CreatureData '{saveData.creatureName}' non trovata nei Resources.");
            return;
        }

        // Inizializza il Player con i dati caricati
        Initialize(creatureData, saveData.currentHP);

        foreach (var partSave in saveData.bodyParts)
        {
            string slotName = partSave.slotName;
            string bodyPartName = partSave.bodyPartName;
            int currentHealth = partSave.currentHealth;

            if (bodyPartName == "None")
            {
                if (bodyPartSlots.ContainsKey(slotName))
                {
                    bodyPartSlots[slotName] = null;
                    if (bodyPartGameObjects.ContainsKey(slotName))
                    {
                        Destroy(bodyPartGameObjects[slotName]);
                        bodyPartGameObjects.Remove(slotName);
                    }
                }
                continue;
            }

            // Carica BodyPartItem dal Resources
            BodyPartItem bodyPart = Resources.Load<BodyPartItem>($"BodyParts/{bodyPartName}");
            if (bodyPart == null)
            {
                Debug.LogError($"[Player] BodyPartItem '{bodyPartName}' non trovata nei Resources/BodyParts.");
                continue;
            }

            // Equip Body Part
            EquipBodyPart(slotName, bodyPart);

            if (bodyPartSlots.ContainsKey(slotName) && bodyPartSlots[slotName] != null)
            {
                bodyPartSlots[slotName].currentHealth = currentHealth;

                // Aggiorna lo SpriteRenderer in base alla salute
                if (bodyPartGameObjects.ContainsKey(slotName))
                {
                    SpriteRenderer sr = bodyPartGameObjects[slotName].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        if (bodyPartSlots[slotName].IsDestroyed())
                        {
                            sr.sprite = null; // O un sprite di distruzione
                            Destroy(bodyPartGameObjects[slotName]);
                            bodyPartGameObjects.Remove(slotName);
                        }
                        else if (bodyPartSlots[slotName].IsDamaged())
                        {
                            sr.sprite = bodyPartSlots[slotName].basePart.damagedSprite;
                        }
                        else
                        {
                            sr.sprite = bodyPartSlots[slotName].basePart.healthySprite;
                        }
                    }
                }
            }
        }

        // Carica l'inventario
        if (inventory != null && saveData.inventoryData != null)
        {
            inventory.LoadFromSaveData(saveData.inventoryData);
        }

        // Aggiorna le statistiche
        InitializeStats();

        OnStatsChanged?.Invoke();

        Debug.Log($"[Player] Dati caricati per {creatureData.creatureName}, HP: {CurrentHealth}/{MaxHealth}");
    }
}