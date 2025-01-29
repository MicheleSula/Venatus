using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "NewBodyPart", menuName = "Game Data/Body Part")]
public class BodyPartItem : ItemData
{
    public int maxHealth;
    [HideInInspector]
    public int currentHealth;
    public int attackModifier;
    public int defenseModifier;
    public int speedModifier;
    public DamageResistanceProfile[] additionalResistances;
    public BodyComposition overrideComposition;
    public MoveData[] partMoves;

    public Sprite healthySprite;
    public Sprite damagedSprite;

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
    }

    public bool IsDamaged()
    {
        return currentHealth > 0 && currentHealth <= maxHealth * 0.3f;
    }

    public bool IsDestroyed()
    {
        return currentHealth <= 0;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }
}
