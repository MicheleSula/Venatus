using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup fadeOverlay;
    public TextMeshProUGUI enemyNameText;
    public Slider playerHealthSlider;
    public Slider enemyHealthSlider;
    public TextMeshProUGUI turnMessageBox;
    public GameObject actionMenuPanel;
    public GameObject bodyPartSelectionPanel;
    public Image enemyImage;
    public Image playerImage;
    public TextMeshProUGUI enemyBodyPartLog;
    public TextMeshProUGUI playerBodyPartLog;
    [SerializeField] private GameObject bodyPartButtonPrefab;

    private Player player;
    private Creature enemy;
    public BattleState state;

    void Start()
    {
        StartCoroutine(StartBattleSequence());
    }

    private IEnumerator StartBattleSequence()
    {
        Debug.Log("[BattleManager] Inizia la sequenza di battaglia");

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError("Player non trovato nella scena! Assicurati che abbia il tag 'Player'.");
            yield break;
        }
        player = playerGO.GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("Il GameObject con tag 'Player' non ha un componente Player!");
            yield break;
        }

        GameManager.Instance.LoadPlayerState(player);

        GameObject enemyGO = GameManager.Instance.overworldEnemyObject;
        if (enemyGO == null)
        {
            Debug.LogError("Nessun nemico associato nel GameManager!");
            yield break;
        }
        enemy = enemyGO.GetComponent<Creature>();
        if (enemy == null)
        {
            Debug.LogError("Il GameObject del nemico non ha un componente Creature!");
            yield break;
        }

        SetupUI();

        yield return StartCoroutine(FadeIn());

        GameManager.Instance.LoadEnemyState(enemy);

        ShowUI();

        bool playerFaster = player.finalStats.speed >= enemy.finalStats.speed;
        state = BattleState.START;
        StartBattle(playerFaster);
    }

    private void SetupUI()
    {
        if (player == null || player.creatureData == null || enemy == null || enemy.creatureData == null)
        {
            Debug.LogError("[SetupUI] Player o Enemy non validi!");
            return;
        }

        if (playerImage != null && player.creatureData.sprite != null)
        {
            playerImage.sprite = player.creatureData.sprite;
            playerImage.preserveAspect = true;
        }
        if (enemyImage != null && enemy.creatureData.sprite != null)
        {
            enemyImage.sprite = enemy.creatureData.sprite;
            enemyImage.preserveAspect = true;
        }
        if (enemyNameText != null)
        {
            enemyNameText.text = enemy.creatureData.creatureName;
        }

        UpdateHealthBars();
        UpdateBodyPartLog();
        UpdatePlayerBodyPartLog();
    }

    private void HideUI()
    {
        playerImage.enabled = false;
        enemyImage.enabled = false;
        enemyNameText.enabled = false;
        playerHealthSlider.gameObject.SetActive(false);
        enemyHealthSlider.gameObject.SetActive(false);
        actionMenuPanel.SetActive(false);
        turnMessageBox.text = "";
    }

    private void ShowUI()
    {
        playerImage.enabled = true;
        enemyImage.enabled = true;
        enemyNameText.enabled = true;
        playerHealthSlider.gameObject.SetActive(true);
        enemyHealthSlider.gameObject.SetActive(true);
        actionMenuPanel.SetActive(true);
    }

    private IEnumerator FadeIn()
    {
        fadeOverlay.alpha = 1f;
        while (fadeOverlay.alpha > 0f)
        {
            fadeOverlay.alpha -= Time.deltaTime * 2f;
            yield return null;
        }
        fadeOverlay.blocksRaycasts = false;
    }

    private IEnumerator FadeOut()
    {
        fadeOverlay.blocksRaycasts = true;
        fadeOverlay.alpha = 0f;
        while (fadeOverlay.alpha < 1f)
        {
            fadeOverlay.alpha += Time.deltaTime * 2f;
            yield return null;
        }
    }

    void StartBattle(bool playerFaster)
    {
        if (playerFaster)
        {
            state = BattleState.PLAYER_TURN;
            Debug.Log("[BattleManager] Turno del giocatore");
            actionMenuPanel.SetActive(true);
            PopulateBodyPartSelection();
        }
        else
        {
            state = BattleState.ENEMY_TURN;
            Debug.Log("[BattleManager] Turno del nemico");
            StartCoroutine(EnemyTurnSequence());
        }
    }

    public void OnClick_Attack(string targetPart)
    {
        if (state != BattleState.PLAYER_TURN)
        {
            Debug.LogWarning("[BattleManager] Non è il turno del giocatore.");
            return;
        }

        Debug.Log($"[BattleManager] Attaccando la parte: {targetPart}");
        if (!enemy.bodyPartSlots.ContainsKey(targetPart) || enemy.bodyPartSlots[targetPart] == null)
        {
            Debug.LogError($"[BattleManager] Body part {targetPart} non trovata nel nemico!");
            return;
        }

        StartCoroutine(PlayerAttackSequence(targetPart));
    }

    public void TryCaptureEnemy()
    {
        if (state != BattleState.PLAYER_TURN)
        {
            Debug.LogWarning("[BattleManager] Non è il turno del giocatore per tentare la cattura.");
            return;
        }

        float captureChance = Mathf.Clamp01(1f - (float)enemy.CurrentHealth / enemy.MaxHealth);
        float roll = Random.Range(0f, 1f);

        Debug.Log($"[BattleManager] Tentativo di cattura: probabilità {captureChance}, roll {roll}");

        if (roll <= captureChance)
        {
            Debug.Log("[BattleManager] Cattura riuscita!");
            GameManager.Instance.CaptureEnemy(enemy);

            state = BattleState.WON;
            StartCoroutine(EndBattleSequence());
        }
        else
        {
            Debug.Log("[BattleManager] Cattura fallita!");

            state = BattleState.ENEMY_TURN;
            StartCoroutine(EnemyTurnSequence());
        }
    }


    private IEnumerator PlayerAttackSequence(string targetPart)
    {
        Debug.Log($"[BattleManager] Il giocatore attacca: {targetPart}");
        actionMenuPanel.SetActive(false);

        yield return TypeMessage($"Attacchi {targetPart} del nemico {enemy.creatureData.creatureName}!");

        int damage = Mathf.Max(player.finalStats.attack - enemy.finalStats.defense, 0);
        Debug.Log($"[PlayerAttackSequence] Danno calcolato: {damage}");
        enemy.TakeDamage(targetPart, damage);

        UpdateHealthBars();
        UpdateBodyPartLog();
        PopulateBodyPartSelection();

        GameManager.Instance.SavePlayerState(player);
        GameManager.Instance.SaveEnemyState(enemy);

        if (enemy.CurrentHealth <= 0)
        {
            Debug.Log("[BattleManager] Il nemico è stato sconfitto!");
            state = BattleState.WON;
            StartCoroutine(EndBattleSequence());
            yield break;
        }

        state = BattleState.ENEMY_TURN;
        StartCoroutine(EnemyTurnSequence());
    }

    private IEnumerator EnemyTurnSequence()
    {
        Debug.Log("[BattleManager] Turno del nemico");

        var playerBodyParts = player.bodyPartSlots.Values
            .Where(part => part != null && !part.IsDestroyed())
            .ToList();

        if (playerBodyParts.Count > 0)
        {
            var targetPart = playerBodyParts[Random.Range(0, playerBodyParts.Count)];
            var chosenMove = enemy.GetRandomMove();

            if (chosenMove == null)
            {
                Debug.LogWarning("[EnemyTurnSequence] Il nemico non ha mosse disponibili.");
                state = BattleState.PLAYER_TURN;
                yield break;
            }

            yield return TypeMessage($"{enemy.creatureData.creatureName} usa {chosenMove.moveName} su {targetPart.basePart.name}!");

            chosenMove.ExecuteMove(enemy, player, targetPart.basePart.name);

            UpdateHealthBars();
            UpdatePlayerBodyPartLog();
            PopulateBodyPartSelection();

            GameManager.Instance.SavePlayerState(player);
            GameManager.Instance.SaveEnemyState(enemy);

            if (player.CurrentHealth <= 0)
            {
                Debug.Log("[BattleManager] Il giocatore è stato sconfitto.");
                state = BattleState.LOST;
                StartCoroutine(ExitBattle());
                yield break;
            }
        }
        else
        {
            Debug.LogWarning("[EnemyTurnSequence] Il nemico non ha bersagli validi.");
        }

        state = BattleState.PLAYER_TURN;
        PopulateBodyPartSelection();
        UpdatePlayerBodyPartLog();
    }

    private IEnumerator EndBattleSequence()
    {
        yield return TypeMessage($"Hai sconfitto {enemy.creatureData.creatureName}!");

        Debug.Log("[BattleManager] Fine battaglia.");

        GameManager.Instance.SavePlayerState(player);
        GameManager.Instance.SaveEnemyState(enemy);

        yield return StartCoroutine(FadeOut());

        GameManager.Instance.OnEnemyKilled();

        SceneManager.UnloadSceneAsync("BattleScene");
    }


    private IEnumerator ExitBattle()
    {
        yield return StartCoroutine(EndGameSequence());
    }

    private IEnumerator EndGameSequence()
    {
        yield return TypeMessage("Sei stato sconfitto!");

        GameManager.Instance.SavePlayerState(player);
        GameManager.Instance.SaveEnemyState(enemy);

        yield return StartCoroutine(FadeOut());

        SceneManager.LoadScene("GameOverScene", LoadSceneMode.Single);
    }

    private IEnumerator TypeMessage(string message)
    {
        turnMessageBox.text = "";
        foreach (char c in message)
        {
            turnMessageBox.text += c;
            yield return new WaitForSeconds(0.03f);
        }
        yield return new WaitForSeconds(0.3f);
    }

    void UpdateHealthBars()
    {
        playerHealthSlider.maxValue = player.MaxHealth;
        playerHealthSlider.value = player.CurrentHealth;

        enemyHealthSlider.maxValue = enemy.MaxHealth;
        enemyHealthSlider.value = enemy.CurrentHealth;
    }

    void UpdateBodyPartLog()
    {
        enemyBodyPartLog.text = "";
        foreach (var partInstance in enemy.bodyPartSlots.Values)
        {
            if (partInstance != null)
            {
                enemyBodyPartLog.text += $"{partInstance.basePart.name}: {partInstance.currentHealth}/{partInstance.basePart.maxHealth}\n";
            }
        }
    }

    void UpdatePlayerBodyPartLog()
    {
        playerBodyPartLog.text = "";
        foreach (var partInstance in player.bodyPartSlots.Values)
        {
            if (partInstance != null)
            {
                playerBodyPartLog.text += $"{partInstance.basePart.name}: {partInstance.currentHealth}/{partInstance.basePart.maxHealth}\n";
            }
        }
    }

    void PopulateBodyPartSelection()
    {
        foreach (Transform child in bodyPartSelectionPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var partInstance in enemy.bodyPartSlots.Values)
        {
            if (partInstance == null) continue;

            GameObject button = Instantiate(bodyPartButtonPrefab, bodyPartSelectionPanel.transform);

            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"{partInstance.basePart.name} ({partInstance.currentHealth}/{partInstance.basePart.maxHealth})";
            }

            Button btn = button.GetComponent<Button>();
            if (btn != null)
            {
                if (partInstance.IsDestroyed())
                {
                    btn.interactable = false;
                    ColorBlock cb = btn.colors;
                    cb.normalColor = Color.gray;
                    cb.disabledColor = Color.gray;
                    btn.colors = cb;
                }
                else
                {
                    btn.interactable = true;
                    ColorBlock cb = btn.colors;
                    cb.normalColor = Color.white;
                    cb.disabledColor = Color.gray;
                    btn.colors = cb;
                }

                btn.onClick.AddListener(() =>
                {
                    Debug.Log($"[PopulateBodyPartSelection] Pulsante premuto per: {partInstance.basePart.name}");
                    OnClick_Attack(partInstance.basePart.name);
                });
            }

            button.transform.localScale = Vector3.one;

        }

        Debug.Log("[PopulateBodyPartSelection] Tutti i pulsanti generati correttamente.");
    }
}