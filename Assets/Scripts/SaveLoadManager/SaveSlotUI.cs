// SaveSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class SaveSlotUI : MonoBehaviour
{
    public Text slotNameText;
    public Text saveDateText;
    public Button loadButton;
    public Button deleteButton;

    private int slotNumber;
    private string saveDirectory;

    private void Start()
    {
        saveDirectory = Application.persistentDataPath;
    }

    public void Setup(int slot)
    {
        slotNumber = slot;
        string slotPath = Path.Combine(saveDirectory, $"save{slot}.json");
        
        if (File.Exists(slotPath))
        {
            slotNameText.text = $"Slot {slot}";
            string json = File.ReadAllText(slotPath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            saveDateText.text = File.GetCreationTime(slotPath).ToString("g"); // Mostra la data e l'ora del salvataggio

            loadButton.onClick.AddListener(() => LoadGame());
            deleteButton.onClick.AddListener(() => DeleteSave());
        }
        else
        {
            slotNameText.text = $"Slot {slot} (Vuoto)";
            saveDateText.text = "Nessun salvataggio.";
            loadButton.interactable = false;
            deleteButton.interactable = false;
        }
    }

    private void LoadGame()
    {
        GameManager.SelectedSaveSlot = slotNumber;
        SceneManager.LoadScene("Overworld");
    }

    private void DeleteSave()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.DeleteSave(slotNumber);
            // Ricarica la scena del menu per aggiornare la UI
            SceneManager.LoadScene("MainMenu");
        }
    }
}