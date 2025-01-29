using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventorySlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI quantityText;

    public bool isEquipmentSlot = false;
    public EquipmentSlot equipmentSlotType;

    private ItemData currentItem;

    public void SetItem(ItemData item, int quantity)
    {
        currentItem = item;

        if (item != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
        else
        {
            icon.enabled = false;
            quantityText.text = "";
        }
    }

    public ItemData GetItem() => currentItem;
}