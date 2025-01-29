using UnityEngine;

[CreateAssetMenu(fileName = "NewBodyComposition", menuName = "Game Data/Body Composition")]
public class BodyComposition : ScriptableObject
{
    public string compositionName;
    public DamageResistanceProfile[] compositionResistances;
    public int compositionAttackMod;
    public int compositionDefenseMod;
    public int compositionSpeedMod;
}