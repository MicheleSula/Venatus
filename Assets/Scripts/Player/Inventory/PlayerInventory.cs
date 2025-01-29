using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public InventorySlot(ItemData item, int qty)
    {
        this.item = item;
        this.quantity = qty;
    }
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    public List<InventorySlot> slots = new List<InventorySlot>();
    public Dictionary<EquipmentSlot, EquipmentData> equippedItems = new Dictionary<EquipmentSlot, EquipmentData>();
    public Player player;  // Assicurati che su questo GameObject ci sia anche lo script "Player"

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            equippedItems[slot] = null;
        }

        player = GetComponent<Player>();
        Debug.Log("[PlayerInventory] Awake -> Trovato player: " + (player ? player.name : "NULL"));
    }


    public void AddItem(ItemData newItem, int amount)
    {
        InventorySlot slot = slots.Find(s => s.item == newItem);
        if (slot != null)
        {
            slot.quantity += amount;
        }
        else
        {
            slots.Add(new InventorySlot(newItem, amount));
        }

        PlayerUIManager.Instance?.RefreshInventoryUI();
    }

    public bool RemoveItem(ItemData item, int amount)
    {
        InventorySlot slot = slots.Find(s => s.item == item);
        if (slot == null || slot.quantity < amount) return false;

        slot.quantity -= amount;
        if (slot.quantity <= 0)
        {
            slots.Remove(slot);
        }

        PlayerUIManager.Instance?.RefreshInventoryUI();
        return true;
    }

    public void EquipEquipmentItem(ItemData itemData)
    {
        if (itemData == null || itemData.itemType != ItemType.Equipment) return;

        EquipmentData equip = itemData as EquipmentData;
        if (equip == null) return;

        EquipmentSlot slot = equip.slotType;

        // Se c'è già qualcosa equipaggiato in quello slot, lo rimetto nell'inventario
        if (equippedItems[slot] != null)
        {
            AddItem(equippedItems[slot], 1);
        }

        RemoveItem(equip, 1);
        equippedItems[slot] = equip;

        // Aggiorno le stats del Player
        player.MarkStatsAsDirty();
        player.InitializeStats();
    }

    public void UnequipEquipmentItem(EquipmentSlot slot)
    {
        if (!equippedItems.ContainsKey(slot) || equippedItems[slot] == null) return;

        AddItem(equippedItems[slot], 1);
        equippedItems[slot] = null;

        player.MarkStatsAsDirty();
        player.InitializeStats();
    }
}