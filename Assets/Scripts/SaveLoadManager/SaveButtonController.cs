// SaveButtonController.cs
using UnityEngine;
using UnityEngine.UI;

public class SaveButtonController : MonoBehaviour
{
    public Button saveButton;
    public int saveSlot = 1; // Puoi rendere questo variabile modificabile nell'Inspector

    private void Start()
    {
        saveButton.onClick.AddListener(SaveGame);
    }

    private void SaveGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGame(saveSlot);
            Debug.Log("[SaveButtonController] Gioco salvato tramite bottone.");
            // Opzionale: Aggiungi feedback visivo all'utente, come una notifica
        }
        else
        {
            Debug.LogError("[SaveButtonController] GameManager.Instance è null!");
        }
    }
}