using UnityEngine;
using Pathfinding;

public class Obstacle : MonoBehaviour
{
    public float health = 100f;

    private void Start()
    {
        UpdateGraph(false);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            UpdateGraph(true);
            Destroy(gameObject);
        }
    }

    public bool IsDestroyed()
    {
        return health <= 0f;
    }

    private void UpdateGraph(bool walkable)
    {
        Bounds obstacleBounds = GetComponent<Collider2D>().bounds;
        GraphUpdateObject guo = new GraphUpdateObject(obstacleBounds)
        {
            modifyWalkability = true,
            setWalkability = walkable
        };
        AstarPath.active.UpdateGraphs(guo);

        Debug.Log($"Ostacolo {gameObject.name}: Walkability impostata a {walkable}");

        if (walkable)
        {
            GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
            if (enemy != null)
            {
                var fsm = enemy.GetComponent<MovementFSM>();
                if (fsm != null)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        fsm.RequestPath(fsm.transform.position, player.transform.position);
                    }
                }
            }
        }
    }

}