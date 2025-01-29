using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    public float crouchSpeed = 2.5f;

    private float currentSpeed;
    private Rigidbody2D rb;
    public Animator animator;
    private Vector2 movement;
    public AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Gestisce il movimento del player.
    /// </summary>
    public void HandleAllMovements()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Leggi l'input del movimento
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Aggiorna l'animator con i valori del movimento
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        // Determina la velocità attuale in base ai tasti premuti
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);

        currentSpeed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        // Calcola la nuova posizione del player
        Vector2 newPosition = rb.position + movement.normalized * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // Gestisci il suono dei passi
        if (movement != Vector2.zero && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (movement == Vector2.zero && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}