using UnityEngine;
using System.Collections;
using System;

public class PlayerDodge : MonoBehaviour
{
    public event Action OnDodgeStart;
    public event Action OnDodgeEnd;

    public float dodgeSpeed = 20.0f;
    public float dodgeTime = 0.5f;
    private float dodgeCooldown = 1.0f;
    private float lastDodgeTime = float.NegativeInfinity;
    private bool isDodging = false;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;

    private bool attemptDodge = false;
    private Vector2 dodgeDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        dodgeDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            attemptDodge = true;
        }
    }

    private void FixedUpdate()
    {
        if (attemptDodge)
        {
            AttemptDodge();
            attemptDodge = false;
        }
    }

    public void AttemptDodge()
    {
        if (Time.time >= lastDodgeTime + dodgeCooldown && !isDodging)
        {
            if (dodgeDirection != Vector2.zero)
            {
                StartCoroutine(PerformDodge(dodgeDirection));
            }
            else
            {
                Debug.Log("Space pressed but no direction input detected.");
            }
        }
        else if (Time.time < lastDodgeTime + dodgeCooldown)
        {
            Debug.Log($"Dodge on cooldown. Time left: {lastDodgeTime + dodgeCooldown - Time.time}");
        }
    }

    private IEnumerator PerformDodge(Vector2 direction)
    {
        isDodging = true;
        OnDodgeStart?.Invoke();
        
        if (playerMovement != null) playerMovement.enabled = false;
        
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * dodgeSpeed, ForceMode2D.Impulse);

        yield return new WaitForSeconds(dodgeTime);

        rb.velocity = Vector2.zero;
        
        if (playerMovement != null) playerMovement.enabled = true;
        
        OnDodgeEnd?.Invoke();
        isDodging = false;
        lastDodgeTime = Time.time;
        Debug.Log($"Dodge completed at time: {lastDodgeTime}");
    }
}
