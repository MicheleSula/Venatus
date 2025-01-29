// MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    public Button newGameButton;
    public Button loadGameButton;
    public Button continueButton; 
    public Transform saveSlotContainer;
    public GameObject saveSlotPrefab;

    private string mainGameSceneName = "OverworldScene";
    private string saveDirectory;
    private int maxSaveSlots = 3;

    private void Start()
    {
        saveDirectory = Application.persistentDataPath;
        
        newGameButton.onClick.AddListener(() => StartNewGame(1));
        loadGameButton.onClick.AddListener(() => LoadGame(1));
        continueButton.onClick.AddListener(() => ContinueGame(1)); // Continuare dallo slot 1

        // Disabilita i bottoni di caricamento se non esiste un salvataggio
        for (int slot = 1; slot <= maxSaveSlots; slot++)
        {
            string slotPath = Path.Combine(saveDirectory, $"save{slot}.json");
            bool exists = File.Exists(slotPath);
            // Qui puoi gestire la visibilità o l'interattività dei bottoni basati sull'esistenza del salvataggio
            // Ad esempio:
            // loadButtons[slot - 1].interactable = exists;
        }
    }

    private void StartNewGame(int slot)
    {
        GameManager.SelectedSaveSlot = slot;
        GameManager.Instance?.DeleteSave(slot); // Elimina eventuali salvataggi precedenti nello slot scelto
        SceneManager.LoadScene(mainGameSceneName);
    }

    private void LoadGame(int slot)
    {
        if (GameManager.Instance.SaveSlotExists(slot))
        {
            GameManager.SelectedSaveSlot = slot;
            SceneManager.LoadScene(mainGameSceneName);
        }
        else
        {
            Debug.LogWarning($"[MainMenuController] Nessun file di salvataggio trovato per lo slot {slot}!");
        }
    }

    private void ContinueGame(int slot)
    {
        if (GameManager.Instance.SaveSlotExists(slot))
        {
            GameManager.SelectedSaveSlot = slot;
            SceneManager.LoadScene(mainGameSceneName);
        }
        else
        {
            Debug.LogWarning($"[MainMenuController] Nessun file di salvataggio trovato per lo slot {slot}!");
        }
    }
}