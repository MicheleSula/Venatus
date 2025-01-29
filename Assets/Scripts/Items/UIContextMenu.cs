using UnityEngine;
using UnityEngine.UI;

public class UIContextMenu : MonoBehaviour
{
    public static UIContextMenu Instance;

    public GameObject panel;
    public Button equipButton;
    public Button dropButton;
    private UIInventorySlot slotReferenced;

    private void Awake()
    {
        Instance = this;
        HideContextMenu();

        equipButton.onClick.AddListener(OnEquipClicked);
        dropButton.onClick.AddListener(OnDropClicked);
    }

    public void ShowContextMenu(UIInventorySlot slot, Vector2 screenPosition)
    {
        slotReferenced = slot;

        panel.SetActive(true);
        panel.transform.position = screenPosition + new Vector2(50, -50);
    }

    public void HideContextMenu()
    {
        slotReferenced = null;
        panel.SetActive(false);
    }

    private void OnEquipClicked()
    {
        if (slotReferenced != null)
        {
            ItemData item = slotReferenced.GetItem();
            if (item != null)
            {
                if (item.itemType == ItemType.Equipment)
                {
                    PlayerInventory.Instance.EquipEquipmentItem(item);
                }
                else if (item.itemType == ItemType.BodyPart)
                {
                    BodyPartItem bodyPart = item as BodyPartItem;
                    if (bodyPart != null)
                    {
                        PlayerInventory.Instance.player.EquipBodyPart("Head", bodyPart);
                    }
                }
            }
        }
        HideContextMenu();
    }

    private void OnDropClicked()
    {
        if (slotReferenced != null)
        {
            ItemData item = slotReferenced.GetItem();
            if (item != null)
            {
                PlayerInventory.Instance.RemoveItem(item, 1);
                // TODO GESTIONE DELL'OGGETTO DOPO CHE L'HAI DROPPATO (PREFAB PER TERRA?)
            }
        }
        HideContextMenu();
    }
}