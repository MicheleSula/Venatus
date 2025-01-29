using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public ItemData itemData;
    public int quantity = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory.Instance.AddItem(itemData, quantity);
            Destroy(gameObject);
        }
    }
}