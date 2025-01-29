using UnityEngine;

[CreateAssetMenu(fileName = "NewCreatureFamily", menuName = "Game Data/Creature Family")]
public class CreatureFamily : ScriptableObject
{
    public string familyName;
    public DamageResistanceProfile[] baseResistances;
    public int baseAttackModifier;
    public int baseDefenseModifier;
    public int baseSpeedModifier;
    public MoveData[] familyMoves;
}