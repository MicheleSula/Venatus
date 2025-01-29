using UnityEngine;

public class SpotlightDamage : MonoBehaviour
{
    public int damagePerSecond = 2;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Creature playerCreature = other.GetComponent<Creature>();
            if (playerCreature != null)
            {
                playerCreature.TakeDamage("Torso", damagePerSecond);
            }
        }
    }
}