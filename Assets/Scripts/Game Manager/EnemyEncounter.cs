using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour
{
    public CreatureData enemyData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance.battleInProgress) return;

            PlayerController.Instance.EnableControls(false);

            GameManager.Instance.savedPlayerPosition = other.transform.position;
            GameManager.Instance.overworldEnemyObject = this.gameObject;
            GameManager.Instance.currentEnemyData = enemyData;

            if (GameManager.Instance.party.Count > 0)
                GameManager.Instance.currentPlayerData = GameManager.Instance.party[0];
            else
                GameManager.Instance.currentPlayerData = GameManager.Instance.playerDataAsset;

            GameManager.Instance.battleInProgress = true;
            GameManager.Instance.overworldPaused = true;

            SceneManager.LoadScene("BattleScene", LoadSceneMode.Additive);
        }
    }
}