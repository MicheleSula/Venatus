using UnityEngine;

[CreateAssetMenu(fileName = "NewCreatureData", menuName = "Game Data/Creature Data")]
public class CreatureData : ScriptableObject
{
    public string creatureName;
    public Sprite sprite;
    public CreatureFamily family;
    public CreatureSpecies species;
    public BodyComposition composition;
    public BehaviorTrait[] defaultTraits;
    public BodyPartItem[] defaultBodyParts;
   
    [Header("Dati loot")]
    public ItemData lootItem;
    public int lootQuantity;
}