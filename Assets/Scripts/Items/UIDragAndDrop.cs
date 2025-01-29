using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIDragAndDrop : MonoBehaviour
{
    public static UIDragAndDrop Instance;

    public Image dragIcon;
    private bool isDragging = false;
    private ItemData draggingItem;
    private UIInventorySlot originSlot;

    private void Awake()
    {
        Instance = this;
        dragIcon.enabled = false;
    }

    public void StartDrag(UIInventorySlot slot, ItemData item)
    {
        if (item == null) return;

        isDragging = true;
        draggingItem = item;
        originSlot = slot;

        dragIcon.sprite = item.icon;
        dragIcon.enabled = true;
    }

    public bool IsDragging() => isDragging;

    public void UpdateDragPosition(PointerEventData eventData)
    {
        if (isDragging)
        {
            dragIcon.transform.position = eventData.position;
        }
    }

    public void EndDrag(UIInventorySlot dropSlot, PointerEventData eventData)
    {
        if (!isDragging) return;

        if (dropSlot != null && dropSlot != originSlot)
        {
            if (dropSlot.isEquipmentSlot)
            {
                if (draggingItem.itemType == ItemType.Equipment)
                {
                    PlayerInventory.Instance.EquipEquipmentItem(draggingItem);
                }
                else if (draggingItem.itemType == ItemType.BodyPart)
                {
                    BodyPartItem bp = draggingItem as BodyPartItem;
                    if (bp != null)
                    {
                        PlayerInventory.Instance.player.EquipBodyPart(dropSlot.equipmentSlotType.ToString(), bp);
                    }
                }
            }
            else
            {
                Debug.Log("Hai trascinato un item su un altro slot di inventario.");
            }
        }

        CancelDrag();
    }

    public void CancelDrag()
    {
        isDragging = false;
        draggingItem = null;
        originSlot = null;
        dragIcon.enabled = false;
    }
}