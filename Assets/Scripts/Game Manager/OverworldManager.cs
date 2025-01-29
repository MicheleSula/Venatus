using UnityEngine;

public class OverworldManager : MonoBehaviour
{
    public GameObject player;

    private void Start()
    {
        Vector3 savedPos = GameManager.Instance.savedPlayerPosition;
        player.transform.position = savedPos;
    }
}