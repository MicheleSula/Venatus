using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;

    [Header("Audio Clips")]
    public AudioClip patrolSound;
    public AudioClip chaseSound;
    public AudioClip attackSound;
    public AudioClip deathSound;
    public AudioClip interactingSound;

    [Header("Animator Triggers")]
    public string patrolTrigger = "Patrol";
    public string chaseTrigger = "Chase";
    public string attackTrigger = "Attack";
    public string deathTrigger = "Death";
    public string interactingTrigger = "Interacting";

    public enum EnemyState { Patrol, Chase, Attack, Death, InteractingWithObstacle }
    private EnemyState currentState;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (newState)
        {
            case EnemyState.Patrol:
                SetAnimationAndSound(patrolTrigger, patrolSound, true);
                break;

            case EnemyState.Chase:
                SetAnimationAndSound(chaseTrigger, chaseSound, true);
                break;

            case EnemyState.Attack:
                SetAnimationAndSound(attackTrigger, attackSound, false);
                break;

            case EnemyState.Death:
                SetAnimationAndSound(deathTrigger, deathSound, false);
                break;

            case EnemyState.InteractingWithObstacle:
                SetAnimationAndSound(interactingTrigger, interactingSound, false);
                break;
        }
    }

    private void SetAnimationAndSound(string trigger, AudioClip sound, bool loop)
    {
        if (!string.IsNullOrEmpty(trigger))
            animator.SetTrigger(trigger);

        if (sound != null)
        {
            audioSource.clip = sound;
            audioSource.loop = loop;
            audioSource.Play();
        }
    }
}