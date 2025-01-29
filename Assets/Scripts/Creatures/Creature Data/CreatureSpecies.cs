using UnityEngine;

[CreateAssetMenu(fileName = "NewCreatureSpecies", menuName = "Game Data/Creature Species")]
public class CreatureSpecies : ScriptableObject
{
    public string speciesName;
    public DamageResistanceProfile[] speciesResistances;
    public int baseAttack;
    public int baseDefense;
    public int baseSpeed;
    public MoveData[] speciesMove;
}