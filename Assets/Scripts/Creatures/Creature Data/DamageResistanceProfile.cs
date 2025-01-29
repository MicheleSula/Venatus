using UnityEngine;

[System.Serializable]
public class DamageResistanceProfile
{
    public DamageType damageType;

    [Range(0f, 3f)]
    public float multiplier = 1f;
}