using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool overworldPaused = false;
    public CreatureData playerDataAsset;
    public int playerCurrentHP = -1;
    public Vector3 savedPlayerPosition = Vector3.zero;
    public bool battleInProgress = false;
    public CreatureData currentEnemyData;
    public CreatureData currentPlayerData;
    public GameObject overworldEnemyObject;
    public Dictionary<string, int> enemyCurrentHPs = new();
    public Dictionary<string, Dictionary<string, int>> enemyBodyPartHPs = new();
    public Dictionary<string, int> playerBodyPartHPs = new();
    public List<CreatureData> party = new List<CreatureData>();
    // SAVEFILE STATES
    public static int SelectedSaveSlot = 1;
    public CreatureSpawner creatureSpawner;
    public Transform partySpawnPoint;
    private string mainGameSceneName = "OverworldScene";

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainGameSceneName)
        {
            if (SaveLoadManager.Instance.SaveSlotExists(GameManager.SelectedSaveSlot))
            {
                LoadGame(GameManager.SelectedSaveSlot);
            }
            else
            {
                InitializeNewGame();
            }
        }
    }

    // Aggiungi una variabile statica per tenere traccia dello slot selezionato
    private void Start()
    {
        if (playerDataAsset != null)
        {
            currentPlayerData = playerDataAsset;

            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                Player player = playerGO.GetComponent<Player>();
                if (player != null)
                {
                    player.Initialize(playerDataAsset);

                    if (playerCurrentHP == -1)
                    {
                        playerCurrentHP = player.MaxHealth;
                        Debug.Log($"[GameManager] HP iniziali del player impostati a {playerCurrentHP}");
                    }

                    player.CurrentHealth = playerCurrentHP;

                    foreach (var part in player.creatureData.defaultBodyParts)
                    {
                        if (part.currentHealth <= 0)
                        {
                            part.currentHealth = part.maxHealth;
                        }
                    }
                    PlayerUIManager.Instance?.RefreshStatsUI();
                }
                else
                {
                    Debug.LogWarning("[GameManager] Il GameObject con tag 'Player' non ha un componente Player!");
                }
            }
            else
            {
                Debug.LogWarning("[GameManager] Nessun GameObject con tag 'Player' trovato!");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] Nessun dato del player assegnato (playerDataAsset).");
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame(int slot)
    {
        SaveData saveData = new SaveData();

        // Salva i dati del Player
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            Player player = playerGO.GetComponent<Player>();
            if (player != null)
            {
                saveData.playerData = player.CreateSaveData();
                saveData.playerPosition = new Vector3SaveData(playerGO.transform.position);
            }
            else
            {
                Debug.LogWarning("[GameManager] Il GameObject con tag 'Player' non ha un componente Player!");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] Nessun GameObject con tag 'Player' trovato durante il salvataggio!");
        }

        // Salva i dati del party
        saveData.partyData = new List<CreatureSaveData>();
        foreach (var creatureData in party)
        {
            CreatureSaveData creatureSave = new CreatureSaveData();
            creatureSave.creatureName = creatureData.creatureName;
            creatureSave.bodyParts = creatureData.defaultBodyParts.Select(bp => new BodyPartSaveData
            {
                slotName = bp.partName, // Assumendo che slotName sia lo stesso di partName
                bodyPartName = bp.partName,
                currentHealth = bp.currentHealth
            }).ToList();

            saveData.partyData.Add(creatureSave);
        }

        // Salva l'inventario del player
        if (PlayerInventory.Instance != null)
        {
            saveData.inventoryData = PlayerInventory.Instance.CreateSaveData();
        }
        else
        {
            saveData.inventoryData = new List<InventoryItemSaveData>();
            Debug.LogWarning("[GameManager] PlayerInventory.Instance è null durante il salvataggio!");
        }

        // Salva altri stati di GameManager se necessario
        saveData.overworldPaused = overworldPaused;
        saveData.battleInProgress = battleInProgress;

        // Utilizza SaveLoadManager per salvare
        if (SaveLoadManager.Instance != null)
        {
            bool success = SaveLoadManager.Instance.SaveGame(slot, saveData);
            if (success)
            {
                Debug.Log($"[GameManager] Gioco salvato correttamente nello slot {slot}.");
            }
        }
        else
        {
            Debug.LogError("[GameManager] SaveLoadManager.Instance è null!");
        }
    }

    // Metodo per caricare il gioco da uno slot specifico
    public void LoadGame(int slot)
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("[GameManager] SaveLoadManager.Instance è null!");
            return;
        }

        SaveData saveData = SaveLoadManager.Instance.LoadGame(slot);
        if (saveData == null)
        {
            Debug.LogWarning($"[GameManager] Nessun salvataggio trovato nello slot {slot}.");
            return;
        }

        // Carica i dati del Player
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            Player player = playerGO.GetComponent<Player>();
            if (player != null && saveData.playerData != null)
            {
                player.LoadFromSaveData(saveData.playerData);
                playerGO.transform.position = saveData.playerPosition.ToVector3();
                PlayerUIManager.Instance?.RefreshStatsUI();
                Debug.Log("[GameManager] Stato del player caricato.");
            }
            else
            {
                Debug.LogWarning("[GameManager] Il GameObject con tag 'Player' non ha un componente Player o manca playerData nel salvataggio!");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] Nessun GameObject con tag 'Player' trovato durante il caricamento!");
        }

        // Carica i dati del party
        if (saveData.partyData != null)
        {
            party.Clear();
            foreach (var creatureSave in saveData.partyData)
            {
                CreatureData creatureData = Resources.Load<CreatureData>($"CreatureData/{creatureSave.creatureName}");
                if (creatureData != null)
                {
                    CreatureData loadedCreature = Instantiate(creatureData);
                    foreach (var partSave in creatureSave.bodyParts)
                    {
                        // Assegna l'HP corrente a ciascuna body part
                        var part = loadedCreature.defaultBodyParts.FirstOrDefault(bp => bp.partName == partSave.bodyPartName);
                        if (part != null)
                        {
                            part.currentHealth = partSave.currentHealth;
                        }
                        else
                        {
                            Debug.LogWarning($"[GameManager] BodyPart '{partSave.bodyPartName}' non trovata in CreatureData '{creatureSave.creatureName}'.");
                        }
                    }

                    party.Add(loadedCreature);

                    // Istanzia le creature nel gioco
                    creatureSpawner.SpawnCreature(loadedCreature, partySpawnPoint.position);
                }
                else
                {
                    Debug.LogWarning($"[GameManager] CreatureData '{creatureSave.creatureName}' non trovata nei Resources/CreatureData.");
                }
            }
        }

        // Carica l'inventario del player
        if (PlayerInventory.Instance != null && saveData.inventoryData != null)
        {
            PlayerInventory.Instance.LoadFromSaveData(saveData.inventoryData);
        }
        else
        {
            Debug.LogWarning("[GameManager] PlayerInventory.Instance è null o saveData.inventoryData è null durante il caricamento!");
        }

        // Carica altri stati di GameManager se necessario
        overworldPaused = saveData.overworldPaused;
        battleInProgress = saveData.battleInProgress;

        Debug.Log($"[GameManager] Gioco caricato correttamente dallo slot {slot}.");
    }

    // Metodo per eliminare un salvataggio specifico
    public void DeleteSave(int slot)
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.DeleteSave(slot);
        }
        else
        {
            Debug.LogError("[GameManager] SaveLoadManager.Instance è null!");
        }
    }

    // Metodo per verificare se uno slot di salvataggio esiste
    public bool SaveSlotExists(int slot)
    {
        if (SaveLoadManager.Instance != null)
        {
            return SaveLoadManager.Instance.SaveSlotExists(slot);
        }
        else
        {
            Debug.LogError("[GameManager] SaveLoadManager.Instance è null!");
            return false;
        }
    }

    public void LoadPlayerState(Player player)
    {
        if (player == null || player.creatureData == null)
        {
            Debug.LogWarning("Tentativo di caricare lo stato del giocatore nullo.");
            return;
        }

        player.CurrentHealth = playerCurrentHP;

        foreach (var slot in player.bodyPartSlots.Keys.ToList())
        {
            string key = $"{slot}_Health";
            if (playerBodyPartHPs.TryGetValue(key, out int partHP))
            {
                player.bodyPartSlots[slot].currentHealth = partHP;
                if (partHP <= 0)
                {
                    if (player.bodyPartGameObjects.ContainsKey(slot) && player.bodyPartGameObjects[slot] != null)
                    {
                        Destroy(player.bodyPartGameObjects[slot]);
                        player.bodyPartGameObjects.Remove(slot);
                    }
                    player.bodyPartSlots[slot] = null;
                    Debug.Log($"[GameManager] Body part {slot} del player distrutta.");
                }
            }
            else
            {
                Debug.LogWarning($"[GameManager] Nessun dato body parts salvato per il player slot {slot}.");
            }
        }

        Debug.Log("[GameManager] Stato del player caricato.");
    }

    public void SavePlayerState(Player player)
    {
        if (player == null || player.creatureData == null)
        {
            Debug.LogWarning("Tentativo di salvare lo stato di un player nullo.");
            return;
        }

        playerCurrentHP = player.CurrentHealth;

        foreach (var part in player.bodyPartSlots)
        {
            string key = $"{part.Key}_Health";
            playerBodyPartHPs[key] = part.Value != null ? part.Value.currentHealth : 0;
            Debug.Log($"[GameManager] Salvataggio: {part.Key} con HP {(part.Value != null ? part.Value.currentHealth : 0)}/{part.Value?.basePart.maxHealth}");
        }

        Debug.Log("[GameManager] Stato del player salvato.");
    }

    public void SaveEnemyState(Creature enemy)
    {
        if (enemy == null || enemy.creatureData == null)
        {
            Debug.LogWarning("Tentativo di salvare lo stato di un nemico nullo.");
            return;
        }

        enemyCurrentHPs[enemy.name] = enemy.CurrentHealth;

        if (!enemyBodyPartHPs.ContainsKey(enemy.name))
        {
            enemyBodyPartHPs[enemy.name] = new Dictionary<string, int>();
        }

        foreach (var part in enemy.bodyPartSlots)
        {
            enemyBodyPartHPs[enemy.name][part.Key] = part.Value != null ? part.Value.currentHealth : 0;
        }

        Debug.Log($"[GameManager] Stato del nemico {enemy.name} salvato.");
    }

    public void LoadEnemyState(Creature enemy)
    {
        if (enemy == null || enemy.creatureData == null)
        {
            Debug.LogWarning("Tentativo di caricare lo stato di un nemico nullo.");
            return;
        }

        if (enemyCurrentHPs.TryGetValue(enemy.name, out int currentHP))
        {
            enemy.CurrentHealth = currentHP;
        }
        else
        {
            Debug.LogWarning($"[GameManager] Nessun dato HP salvato per il nemico {enemy.name}.");
        }

        if (enemyBodyPartHPs.TryGetValue(enemy.name, out var partsState))
        {
            foreach (var part in enemy.creatureData.defaultBodyParts)
            {
                if (partsState.TryGetValue(part.name, out int partHP))
                {
                    part.currentHealth = partHP;
                }
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] Nessun dato body parts salvato per il nemico {enemy.name}.");
        }

        Debug.Log($"[GameManager] Stato del nemico {enemy.name} caricato.");

        EnemySkeletonController skeletonController = enemy.GetComponent<EnemySkeletonController>();
        if (skeletonController != null)
        {
            skeletonController.InitializeSkeleton(enemy.creatureData);
        }
        else
        {
            Debug.LogWarning($"[GameManager] Nemico {enemy.name} non ha un componente EnemySkeletonController!");
        }
    }

    public void OnEnemyKilled()
    {
        if (overworldEnemyObject != null)
        {
            EnemyOverworld eow = overworldEnemyObject.GetComponent<EnemyOverworld>();
            if (eow != null)
            {
                GameObject corpse = Instantiate(eow.corpsePrefab, overworldEnemyObject.transform.position, Quaternion.identity);

                LootableCorpse lootableCorpse = corpse.GetComponent<LootableCorpse>();
                if (lootableCorpse != null)
                {
                    lootableCorpse.InitCorpse(eow.enemyData, 360f, null, true);
                }

                Debug.Log($"[GameManager] Nemico sconfitto e trasformato in cadavere: {corpse.name}");
            }

            enemyCurrentHPs.Remove(overworldEnemyObject.name);
            enemyBodyPartHPs.Remove(overworldEnemyObject.name);

            Destroy(overworldEnemyObject);
            overworldEnemyObject = null;
        }

        overworldPaused = false;
        battleInProgress = false;

        RestoreOverworldState();
    }

    private void RestoreOverworldState()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerGO.transform.position = savedPlayerPosition;
            PlayerController.Instance.EnableControls(true);

            Camera.main.GetComponent<CameraController>().target = playerGO.transform;

            Debug.Log($"[GameManager] Player ripristinato alla posizione {savedPlayerPosition}");
        }
        else
        {
            Debug.LogError("[GameManager] Player non trovato al ritorno all'overworld.");
        }
    }

    public void CaptureEnemy(Creature enemy)
    {
        if (enemy == null || enemy.creatureData == null)
        {
            Debug.LogError("[GameManager] Tentativo di catturare un nemico nullo.");
            return;
        }

        CreatureData capturedCreature = Instantiate(enemy.creatureData);
        for (int i = 0; i < capturedCreature.defaultBodyParts.Length; i++)
        {
            var origPart = capturedCreature.defaultBodyParts[i];
            var newPart = Instantiate(origPart);
            if (enemy.bodyPartSlots.TryGetValue(origPart.name, out var partInstance))
            {
                if (partInstance != null)
                {
                    newPart.currentHealth = partInstance.currentHealth;
                }
            }

            capturedCreature.defaultBodyParts[i] = newPart;
        }

        capturedCreature.creatureName += "_Captured_" + Random.Range(1000, 9999);

        party.Add(capturedCreature);

        Debug.Log($"[GameManager] Creatura {capturedCreature.creatureName} (ID={capturedCreature.GetInstanceID()}) catturata e aggiunta al party!");

        Debug.Log("[GameManager] Party attuale:");
        for (int i = 0; i < party.Count; i++)
        {
            Debug.Log($" -> party[{i}]: name={party[i].creatureName}, ID={party[i].GetInstanceID()}");
        }
    }
}