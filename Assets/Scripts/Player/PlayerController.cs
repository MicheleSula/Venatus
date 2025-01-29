using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    private PlayerMovement playerMovement;
    private PlayerDodge playerDodge;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        playerMovement = GetComponent<PlayerMovement>();
        playerDodge = GetComponent<PlayerDodge>();
    }

    /// <summary>
    /// Abilita o disabilita i controlli del player.
    /// </summary>
    public void EnableControls(bool enable)
    {
        if (playerMovement != null) playerMovement.enabled = enable;
        if (playerDodge != null) playerDodge.enabled = enable;

        // Ferma i movimenti se i controlli vengono disabilitati
        if (!enable && playerMovement != null)
        {
            playerMovement.HandleAllMovements();
        }
    }

    private void Update()
    {
        // Controlla se i movimenti sono abilitati
        if (playerMovement != null && playerMovement.enabled)
        {
            playerMovement.HandleAllMovements();
        }

        // Controlla il dodge
        if (playerDodge != null && playerDodge.enabled && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            playerDodge.AttemptDodge();
        }
    }
}