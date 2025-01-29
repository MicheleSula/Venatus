using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PartySlotUI : MonoBehaviour
{
    [Header("Left Panel")]
    public TextMeshProUGUI nameText;
    public Image spriteImage;
    public Slider hpSlider;
    
    // Se vuoi anche Attack, Defense, Speed:
    //public TextMeshProUGUI attackText;
    //public TextMeshProUGUI defenseText;
    //public TextMeshProUGUI speedText;

    [Header("Right Panel")]
    public RectTransform bodyPartsPanel;

    /// <summary>
    /// Visualizza i dati di una creatura e popola le body parts a destra.
    /// </summary>
    public void Setup(CreatureData data, GameObject bodyPartSlotPrefab)
    {
        if (data == null) return;
        
        // 1. Nome e Sprite
        if (nameText != null)
            nameText.text = data.creatureName;

        if (spriteImage != null)
            spriteImage.sprite = data.sprite;

        // 2. Calcolo HP totali sommando tutti i body parts
        var parts = data.defaultBodyParts;
        int currentHP = parts.Sum(p => p.currentHealth);
        int maxHP = parts.Sum(p => p.maxHealth);

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        // 3. (Opzionale) Attack/Defense/Speed
        // if (attackText != null) attackText.text = ...
        // ...

        // 4. Pulizia bodyPartsPanel
        foreach (Transform child in bodyPartsPanel)
        {
            Destroy(child.gameObject);
        }

        // 5. Creazione di uno slot BodyPartSlotUI per ogni body part
        foreach (var part in parts)
        {
            var slotObj = Instantiate(bodyPartSlotPrefab, bodyPartsPanel);
            var slotUI = slotObj.GetComponent<BodyPartSlotUI>();
            if (slotUI != null)
                slotUI.Setup(part);
        }
    }
}