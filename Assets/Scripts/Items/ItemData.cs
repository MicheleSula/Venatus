using UnityEngine;

public enum ItemType
{
    Consumable,
    Equipment,
    Material,
    BodyPart
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Game Data/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public string description;
    public int maxStack = 99;
}

public enum EquipmentSlot
{
    Head,
    Chest,
    Legs,
    Weapon1,
    Weapon2,
    Necklace,
    Ring
}

[CreateAssetMenu(fileName = "NewEquipmentData", menuName = "Game Data/Equipment Data")]
public class EquipmentData : ItemData
{
    [Header("Modificatori Statistiche")]
    public int attackModifier = 0;
    public int defenseModifier = 0;
    public int speedModifier = 0;
    public int healthModifier = 0;

    [Header("Tipo di Slot")]
    public EquipmentSlot slotType;
}

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Game Data/Consumable")]
public class ConsumableData : ItemData
{
    public int healAmount;
}