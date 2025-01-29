using UnityEngine;

[System.Serializable]
public class BodyPartInstance
{
    public BodyPartItem basePart;
    public int currentHealth;
    public GameObject gameObject;

    // Costruttore che accetta un BodyPartItem
    public BodyPartInstance(BodyPartItem part)
    {
        basePart = part;
        currentHealth = part.maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
    }

    public bool IsDestroyed()
    {
        return currentHealth <= 0;
    }

    public bool IsDamaged()
    {
        return currentHealth < basePart.maxHealth;
    }

    public void ResetHealth()
    {
        currentHealth = basePart.maxHealth;
    }
}
