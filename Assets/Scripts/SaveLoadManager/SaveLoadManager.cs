// SaveLoadManager.cs
using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;
    private string saveDirectory;
    public int maxSaveSlots = 3; // Puoi modificare in base alle tue esigenze

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveDirectory = Application.persistentDataPath;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool SaveGame(int slot, SaveData data)
    {
        if (slot < 1 || slot > maxSaveSlots)
        {
            Debug.LogError($"[SaveLoadManager] Slot {slot} non valido. Deve essere tra 1 e {maxSaveSlots}.");
            return false;
        }

        string slotPath = Path.Combine(saveDirectory, $"save{slot}.json");
        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(slotPath, json);
            Debug.Log($"[SaveLoadManager] Gioco salvato correttamente nello slot {slot}.");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Errore durante il salvataggio nello slot {slot}: {e.Message}");
            return false;
        }
    }

    public SaveData LoadGame(int slot)
    {
        if (slot < 1 || slot > maxSaveSlots)
        {
            Debug.LogError($"[SaveLoadManager] Slot {slot} non valido. Deve essere tra 1 e {maxSaveSlots}.");
            return null;
        }

        string slotPath = Path.Combine(saveDirectory, $"save{slot}.json");
        if (!File.Exists(slotPath))
        {
            Debug.LogWarning($"[SaveLoadManager] Nessun file di salvataggio trovato per lo slot {slot}.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(slotPath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"[SaveLoadManager] Gioco caricato correttamente dallo slot {slot}.");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Errore durante il caricamento dallo slot {slot}: {e.Message}");
            return null;
        }
    }

    public void DeleteSave(int slot)
    {
        if (slot < 1 || slot > maxSaveSlots)
        {
            Debug.LogError($"[SaveLoadManager] Slot {slot} non valido. Deve essere tra 1 e {maxSaveSlots}.");
            return;
        }

        string slotPath = Path.Combine(saveDirectory, $"save{slot}.json");
        if (File.Exists(slotPath))
        {
            try
            {
                File.Delete(slotPath);
                Debug.Log($"[SaveLoadManager] File di salvataggio dello slot {slot} eliminato.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveLoadManager] Errore durante l'eliminazione dello slot {slot}: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"[SaveLoadManager] Nessun file di salvataggio trovato per lo slot {slot} durante l'eliminazione.");
        }
    }

    public bool SaveSlotExists(int slot)
    {
        if (slot < 1 || slot > maxSaveSlots)
        {
            Debug.LogError($"[SaveLoadManager] Slot {slot} non valido. Deve essere tra 1 e {maxSaveSlots}.");
            return false;
        }

        string slotPath = Path.Combine(saveDirectory, $"save{slot}.json");
        return File.Exists(slotPath);
    }
}