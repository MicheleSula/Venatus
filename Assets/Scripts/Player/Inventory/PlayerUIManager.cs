using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance;

    [Header("Menu Manager")]
    public GameObject menuPanel;
    public List<Button> tabButtons;
    public List<GameObject> contentPanels;

    [Header("Stats UI")]
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI speedText;

    [Header("Inventory UI")]
    public List<UIInventorySlot> inventorySlots;
    public UIInventorySlot headSlot;
    public UIInventorySlot chestSlot;

    [Header("Party UI")]
    public Transform partyContainer;
    public GameObject partySlotPrefab;
    public GameObject bodyPartSlotPrefab;

    private bool isMenuOpen = false;
    private int activeTabIndex = 0;
    private int currentPartyIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[PlayerUIManager] Awake -> Instance set.");
        }
        else
        {
            Debug.LogWarning("[PlayerUIManager] Awake -> Another Instance detected, destroying this one.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("[PlayerUIManager] Start -> Setup Tab Buttons, Refresh Stats, Update Tab Display.");
        SetupTabButtons();
        RefreshStatsUI();
        UpdateTabDisplay();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("[PlayerUIManager] Update -> Tab pressed. Toggling Menu.");
            ToggleMenu();
        }
    }

    private void SetupTabButtons()
    {
        Debug.Log("[PlayerUIManager] SetupTabButtons -> Setting OnClick listeners.");
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        Debug.Log($"[PlayerUIManager] ToggleMenu -> isMenuOpen = {isMenuOpen}");

        if (menuPanel != null)
        {
            menuPanel.SetActive(isMenuOpen);
            Debug.Log($"[PlayerUIManager] ToggleMenu -> menuPanel.SetActive({isMenuOpen}).");
        }
        else
        {
            Debug.LogWarning("[PlayerUIManager] ToggleMenu -> menuPanel is NULL!");
        }

        Time.timeScale = isMenuOpen ? 0f : 1f;
        Debug.Log($"[PlayerUIManager] ToggleMenu -> Time.timeScale = {Time.timeScale}");

        if (isMenuOpen)
        {
            RefreshInventoryUI();
            RefreshPartyUI();
        }
    }

    public void SwitchTab(int index)
    {
        Debug.Log($"[PlayerUIManager] SwitchTab -> index = {index}");
        activeTabIndex = index;
        UpdateTabDisplay();
    }

    private void UpdateTabDisplay()
    {
        Debug.Log($"[PlayerUIManager] UpdateTabDisplay -> activeTabIndex = {activeTabIndex}");
        for (int i = 0; i < contentPanels.Count; i++)
        {
            bool show = (i == activeTabIndex);
            contentPanels[i].SetActive(show);
            Debug.Log($"[PlayerUIManager] UpdateTabDisplay -> contentPanels[{i}] setActive({show}).");
        }

        for (int i = 0; i < tabButtons.Count; i++)
        {
            ColorBlock colors = tabButtons[i].colors;
            if (i == activeTabIndex)
                colors.normalColor = Color.yellow;
            else
                colors.normalColor = Color.white;
            tabButtons[i].colors = colors;
        }
    }

    public void RefreshStatsUI()
    {
        Debug.Log("[PlayerUIManager] RefreshStatsUI -> Checking player stats...");

        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("[PlayerUIManager] RefreshStatsUI -> PlayerInventory.Instance is NULL!");
            return;
        }

        var player = PlayerInventory.Instance.player;
        if (player == null)
        {
            Debug.LogWarning("[PlayerUIManager] RefreshStatsUI -> player is NULL in PlayerInventory!");
            return;
        }

        var stats = player.finalStats;
        Debug.Log($"[PlayerUIManager] RefreshStatsUI -> Reading Stats: ATK={stats.attack}, DEF={stats.defense}, SPD={stats.speed}");

        if (attackText != null) attackText.text = $"ATK: {stats.attack}";
        if (defenseText != null) defenseText.text = $"DEF: {stats.defense}";
        if (speedText != null) speedText.text = $"SPD: {stats.speed}";
    }

    public void RefreshInventoryUI()
    {
        Debug.Log("[PlayerUIManager] RefreshInventoryUI -> Updating inventory slots.");

        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("[PlayerUIManager] RefreshInventoryUI -> PlayerInventory.Instance is NULL!");
            return;
        }

        var slotsData = PlayerInventory.Instance.slots;
        if (slotsData == null)
        {
            Debug.LogWarning("[PlayerUIManager] RefreshInventoryUI -> PlayerInventory slots data is NULL!");
            return;
        }

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < slotsData.Count)
            {
                Debug.Log($"[PlayerUIManager] RefreshInventoryUI -> Setting slot {i} with item {slotsData[i].item?.itemName}, qty {slotsData[i].quantity}");
                inventorySlots[i].SetItem(slotsData[i].item, slotsData[i].quantity);
            }
            else
            {
                Debug.Log($"[PlayerUIManager] RefreshInventoryUI -> Clearing slot {i}");
                inventorySlots[i].SetItem(null, 0);
            }
        }

        var equipped = PlayerInventory.Instance.equippedItems;
        if (equipped.TryGetValue(EquipmentSlot.Head, out var headItem))
        {
            Debug.Log($"[PlayerUIManager] RefreshInventoryUI -> HeadSlot = {headItem?.itemName ?? "NULL"}");
            headSlot.SetItem(headItem, headItem != null ? 1 : 0);
        }
        else
        {
            headSlot.SetItem(null, 0);
        }

        if (equipped.TryGetValue(EquipmentSlot.Chest, out var chestItem))
        {
            Debug.Log($"[PlayerUIManager] RefreshInventoryUI -> ChestSlot = {chestItem?.itemName ?? "NULL"}");
            chestSlot.SetItem(chestItem, chestItem != null ? 1 : 0);
        }
        else
        {
            chestSlot.SetItem(null, 0);
        }
    }

    public void RefreshPartyUI()
    {
        Debug.Log("[PlayerUIManager] RefreshPartyUI -> Updating Party UI.");

        if (partyContainer == null)
        {
            Debug.LogWarning("[PlayerUIManager] partyContainer is NULL!");
            return;
        }

        // Distrugge eventuali vecchi "PartySlotUI"
        foreach (Transform child in partyContainer)
            Destroy(child.gameObject);

        var party = GameManager.Instance.party; // supponendo contenga CreatureData
        if (party == null || party.Count == 0)
        {
            Debug.LogWarning("Party is empty!");
            return;
        }

        // Garantisci che currentPartyIndex sia entro limiti
        currentPartyIndex = Mathf.Clamp(currentPartyIndex, 0, party.Count - 1);

        // Prendi la creatura corrente
        var creatureData = party[currentPartyIndex];

        // Instanzia il prefab "PartySlotUI"
        var slotObj = Instantiate(partySlotPrefab, partyContainer);
        var slotUI = slotObj.GetComponent<PartySlotUI>();
        if (slotUI != null)
        {
            // Passa la creatura e il prefab BodyPartSlotUI
            slotUI.Setup(creatureData, bodyPartSlotPrefab);
        }
    }


    public void ShowNextPartyCreature()
    {
        Debug.Log("[PlayerUIManager] ShowNextPartyCreature -> Next clicked.");
        if (GameManager.Instance == null) return;
        var party = GameManager.Instance.party;
        if (party == null || party.Count == 0) return;

        currentPartyIndex = (currentPartyIndex + 1) % party.Count;
        RefreshPartyUI();
    }

    public void ShowPrevPartyCreature()
    {
        Debug.Log("[PlayerUIManager] ShowPrevPartyCreature -> Previous clicked.");

        if (GameManager.Instance == null) return;
        var party = GameManager.Instance.party;
        if (party == null || party.Count == 0) return;

        currentPartyIndex--;
        if (currentPartyIndex < 0) currentPartyIndex = party.Count - 1;
        RefreshPartyUI();
    }
}