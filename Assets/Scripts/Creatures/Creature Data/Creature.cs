using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Creature : MonoBehaviour
{
    [Header("Dati di Base")]
    public CreatureData creatureData;

    [Header("Statistiche Finali")]
    [SerializeField]
    public CreatureStats finalStats = new CreatureStats();

    [Header("Mosse ereditate")]
    public List<MoveData> availableMoves = new List<MoveData>();

    public int CurrentHealth { get; set; }

    // Dizionario per le parti del corpo
    public Dictionary<string, BodyPartInstance> bodyPartSlots = new();

    private Dictionary<DamageType, float> resistanceDict = new();

    // Mappa per tracciare le mosse associate a ciascuna parte del corpo
    private Dictionary<BodyPartInstance, List<MoveData>> bodyPartToMoves = new();
    public Dictionary<string, GameObject> bodyPartGameObjects = new();
    public Transform bodyParent;

    public int MaxHealth
    {
        get
        {
            if (creatureData.defaultBodyParts == null) return 0;
            return creatureData.defaultBodyParts.Sum(part => part.maxHealth);
        }
    }

    public virtual void Initialize(CreatureData data, int? savedHealth = null)
    {
        if (data == null)
        {
            Debug.LogError($"[Creature] CreatureData è null per {name}");
            return;
        }

        creatureData = data;

        InitializeBodyParts();
        ResetStats();
        ApplyBaseStats();
        ApplyBodyPartModifiers();
        ApplyResistances();
        LoadMoves();

        int maxHealth = MaxHealth;

        CurrentHealth = savedHealth ?? maxHealth;

        foreach (var partInstance in bodyPartSlots.Values)
        {
            if (partInstance != null)
            {
                partInstance.ResetHealth();
            }
            else
            {
                Debug.LogError($"[Creature] partInstance è null per uno slot durante l'inizializzazione.");
            }
        }

        finalStats.maxHealth = MaxHealth;

        Debug.Log($"[Creature] {creatureData.creatureName} inizializzato con {CurrentHealth}/{maxHealth} HP.");
    }

    private void InitializeBodyParts()
    {
        if (creatureData.defaultBodyParts == null || creatureData.defaultBodyParts.Length == 0)
        {
            Debug.LogError("[Creature] Body parts non trovate o vuote!");
            return;
        }

        bodyPartSlots = new Dictionary<string, BodyPartInstance>
        {
            {"Testa", null},
            {"Torso", null},
            {"braccio_dx", null},
            {"braccio_sx", null},
            {"gamba_dx", null},
            {"gamba_sx", null}
        };

        // Log dei body parts disponibili
        Debug.Log("[Creature] Body parts disponibili in CreatureData:");
        foreach (var part in creatureData.defaultBodyParts)
        {
            Debug.Log($"- {part.name}");
        }

        foreach (var part in creatureData.defaultBodyParts)
        {
            if (bodyPartSlots.ContainsKey(part.name))
            {
                var newPartInstance = new BodyPartInstance(part);
                bodyPartSlots[part.name] = newPartInstance;
                Debug.Log($"[Creature] Body part caricata: {newPartInstance.basePart.name}, MaxHealth: {newPartInstance.basePart.maxHealth}, Instance ID: {newPartInstance.GetHashCode()}");
            }
            else
            {
                Debug.LogWarning($"[Creature] Body part {part.name} non corrisponde a nessuno slot definito.");
            }
        }

        // Verifica che tutte le parti del corpo siano state inizializzate correttamente
        foreach (var slot in bodyPartSlots)
        {
            if (slot.Value == null)
            {
                Debug.LogWarning($"[Creature] Slot '{slot.Key}' non ha una parte del corpo assegnata.");
            }
        }
    }

    public void LoadMoves()
    {
        availableMoves.Clear();
        bodyPartToMoves.Clear();

        // Aggiungi mosse di famiglia se presenti
        if (creatureData.family != null && creatureData.family.familyMoves != null)
        {
            availableMoves.AddRange(creatureData.family.familyMoves);
            // Puoi tracciare queste mosse separatamente se necessario
        }

        // Aggiungi mosse di specie se presenti
        if (creatureData.species != null && creatureData.species.speciesMove != null)
        {
            availableMoves.AddRange(creatureData.species.speciesMove);
            // Puoi tracciare queste mosse separatamente se necessario
        }

        // Aggiungi mosse associate a ciascuna parte del corpo
        foreach (var partInstance in bodyPartSlots.Values)
        {
            if (partInstance != null && partInstance.basePart.partMoves != null)
            {
                availableMoves.AddRange(partInstance.basePart.partMoves);
                bodyPartToMoves[partInstance] = new List<MoveData>(partInstance.basePart.partMoves);
            }
        }

        Debug.Log($"[Creature] {creatureData.creatureName} ha caricato {availableMoves.Count} mosse.");
    }

    private void ResetStats()
    {
        finalStats.Reset();
        resistanceDict.Clear();

        foreach (DamageType dt in System.Enum.GetValues(typeof(DamageType)))
        {
            resistanceDict[dt] = 1f;
        }
    }

    private void ApplyBaseStats()
    {
        finalStats.attack = creatureData.species.baseAttack + creatureData.family.baseAttackModifier + creatureData.composition.compositionAttackMod;
        finalStats.defense = creatureData.species.baseDefense + creatureData.family.baseDefenseModifier + creatureData.composition.compositionDefenseMod;
        finalStats.speed = creatureData.species.baseSpeed + creatureData.family.baseSpeedModifier + creatureData.composition.compositionSpeedMod;
        // finalStats.maxHealth = creatureData.species.baseHealth;
    }

    private void ApplyResistances()
    {
        ApplyResistanceProfiles(creatureData.family.baseResistances);
        ApplyResistanceProfiles(creatureData.species.speciesResistances);
        ApplyResistanceProfiles(creatureData.composition.compositionResistances);
    }

    private void ApplyResistanceProfiles(DamageResistanceProfile[] profiles)
    {
        if (profiles == null) return;

        foreach (var profile in profiles)
        {
            if (profile == null) continue;
            resistanceDict[profile.damageType] *= profile.multiplier;
        }
    }

    public virtual void EquipBodyPart(string slotName, BodyPartItem newPart)
    {
        if (!bodyPartSlots.ContainsKey(slotName)) return;

        var newPartInstance = new BodyPartInstance(newPart);
        bodyPartSlots[slotName] = newPartInstance;

        // Aggiungi le mosse della nuova parte del corpo
        if (newPart.partMoves != null)
        {
            availableMoves.AddRange(newPart.partMoves);
            bodyPartToMoves[newPartInstance] = new List<MoveData>(newPart.partMoves);
        }

        ResetStats();
        ApplyBaseStats();
        ApplyBodyPartModifiers();
    }

    private void ApplyBodyPartModifiers()
    {
        foreach (var partInstance in bodyPartSlots.Values)
        {
            if (partInstance != null)
            {
                finalStats.attack += partInstance.basePart.attackModifier;
                finalStats.defense += partInstance.basePart.defenseModifier;
                finalStats.speed += partInstance.basePart.speedModifier;

                Debug.Log($"[Creature] Modificatore applicato da {partInstance.basePart.name}: " +
                          $"ATK +{partInstance.basePart.attackModifier}, DEF +{partInstance.basePart.defenseModifier}, SPD +{partInstance.basePart.speedModifier}");

                ApplyResistanceProfiles(partInstance.basePart.additionalResistances);
            }
        }
    }

    public MoveData GetRandomMove()
    {
        if (availableMoves.Count == 0)
        {
            Debug.LogWarning($"[Creature] {creatureData.creatureName} non ha mosse disponibili!");
            return null;
        }
        return availableMoves[Random.Range(0, availableMoves.Count)];
    }

    public virtual void TakeDamage(string partName, int damage)
    {
        if (bodyPartSlots.ContainsKey(partName))
        {
            var partInstance = bodyPartSlots[partName];

            // Evita di infliggere danno a una parte già distrutta
            if (partInstance.IsDestroyed())
            {
                Debug.Log($"[TakeDamage] La parte {partInstance.basePart.name} è già distrutta. Danno ignorato.");
                return;
            }

            Debug.Log($"[TakeDamage] Prima del danno: {partInstance.basePart.name} HP: {partInstance.currentHealth}/{partInstance.basePart.maxHealth}");
            partInstance.TakeDamage(damage);
            Debug.Log($"[TakeDamage] Dopo il danno: {partInstance.basePart.name} HP: {partInstance.currentHealth}/{partInstance.basePart.maxHealth}");

            // Aggiorna il SpriteRenderer se presente
            if (bodyPartGameObjects.ContainsKey(partName))
            {
                SpriteRenderer sr = bodyPartGameObjects[partName].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (partInstance.IsDestroyed())
                    {
                        sr.sprite = null; // O un sprite di distruzione
                        Destroy(bodyPartGameObjects[partName]);
                        bodyPartGameObjects.Remove(partName);
                    }
                    else if (partInstance.IsDamaged())
                    {
                        sr.sprite = partInstance.basePart.damagedSprite;
                    }
                    else
                    {
                        sr.sprite = partInstance.basePart.healthySprite;
                    }
                }
            }

            // Calcola la salute totale rimanente considerando solo le parti non distrutte
            CurrentHealth = bodyPartSlots.Values
                .Where(p => p != null && !p.IsDestroyed())
                .Sum(p => p.currentHealth);

            if (partInstance.IsDestroyed())
            {
                Debug.Log($"[TakeDamage] {partInstance.basePart.name} è distrutta.");

                // Rimuovi le mosse associate a questa parte del corpo
                if (bodyPartToMoves.ContainsKey(partInstance))
                {
                    foreach (var move in bodyPartToMoves[partInstance])
                    {
                        availableMoves.Remove(move);
                        Debug.Log($"[Creature] Mossa {move.moveName} rimossa a causa della distruzione di {partInstance.basePart.name}");
                    }
                    bodyPartToMoves.Remove(partInstance);
                }

                if (partInstance.basePart.name == "Testa")
                {
                    Die();
                    return;
                }
            }

            if (CurrentHealth <= 0 || CheckTorsoCritical())
            {
                Die();
            }
        }
        else
        {
            Debug.LogWarning($"[TakeDamage] Parte del corpo {partName} non trovata.");
        }
    }

    private bool CheckTorsoCritical()
    {
        if (bodyPartSlots.TryGetValue("Torso", out var torso) && torso != null)
        {
            bool isCritical = torso.currentHealth <= torso.basePart.maxHealth * 0.5f;
            if (isCritical)
            {
                Debug.Log("[Creature] Il torso è in condizioni critiche! HP del torso: " +
                          $"{torso.currentHealth}/{torso.basePart.maxHealth}");
            }
            return isCritical;
        }
        return false;
    }

    private bool CanHitHead()
    {
        int destroyedParts = bodyPartSlots.Values.Count(p => p.IsDestroyed());
        int requiredDestroyedParts = Mathf.CeilToInt(bodyPartSlots.Count * 0.5f);

        Debug.Log($"[Creature] Parti distrutte: {destroyedParts}/{requiredDestroyedParts} necessarie per attaccare la testa.");

        return destroyedParts >= requiredDestroyedParts;
    }

    public int TotalHP
    {
        get
        {
            return bodyPartSlots.Values.Sum(part => part.basePart.maxHealth);
        }
    }

    private void Die()
    {
        Debug.Log($"{name} è morto!");
        // Puoi aggiungere ulteriori logiche di morte qui
    }

    public bool HasLegs()
    {
        return bodyPartSlots.ContainsKey("gamba_sx") &&
               bodyPartSlots["gamba_sx"] != null &&
               !bodyPartSlots["gamba_sx"].IsDestroyed() &&
               bodyPartSlots.ContainsKey("gamba_dx") &&
               bodyPartSlots["gamba_dx"] != null &&
               !bodyPartSlots["gamba_dx"].IsDestroyed();
    }
}