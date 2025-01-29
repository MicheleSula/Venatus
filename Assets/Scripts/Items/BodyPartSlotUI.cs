using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BodyPartSlotUI : MonoBehaviour
{
    public Image partImage;
    public TextMeshProUGUI hpText;

    public void Setup(BodyPartItem part)
    {
        if (part == null) return;

        // Sprite in base a se è danneggiato o no
        if (partImage != null)
            partImage.sprite = part.IsDamaged() ? part.damagedSprite : part.healthySprite;

        if (hpText != null)
            hpText.text = $"{part.currentHealth}/{part.maxHealth}";
    }
}