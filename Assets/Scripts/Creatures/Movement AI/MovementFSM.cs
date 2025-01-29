using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

[RequireComponent(typeof(Creature))]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(Animator))]
public class MovementFSM : MonoBehaviour
{
    public MovementState currentState;
    [HideInInspector] public Creature creature;
    [HideInInspector] public EnemyController enemyController;
    [HideInInspector] public Animator animator;

    public float detectionRange = 5f;
    public float moveSpeed = 2f;
    private float stuckTimer = 0f;
    private float maxStuckTime = 1f;
    public Seeker seeker;
    public Path currentPath;
    public int currentWaypointIndex;
    public float nextWaypointDistance = 0.5f;

    public LayerMask obstacleLayer;
    public float obstacleInteractionRange = 1.5f;

    private bool isPathPending = false;
    private float pathRequestCooldown = 1f;
    private float pathRequestTimer = 0f;

    private void Awake()
    {
        creature = GetComponent<Creature>();
        seeker = GetComponent<Seeker>();
        enemyController = GetComponent<EnemyController>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentState = new RandomPatrolState(this);
        currentState.OnEnter();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.overworldPaused)
        {
            return;
        }

        pathRequestTimer -= Time.deltaTime;
        currentState?.LogicUpdate();
    }

    public void ChangeState(MovementState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }

    public void RequestPath(Vector3 startPos, Vector3 targetPos)
    {
        if (seeker == null || isPathPending || pathRequestTimer > 0f) return;

        isPathPending = true;
        pathRequestTimer = pathRequestCooldown;
        seeker.StartPath(startPos, targetPos, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        isPathPending = false;
        if (p.error)
        {
            currentPath = null;
        }
        else
        {
            currentPath = p;
            currentWaypointIndex = 0;
        }
    }

    public bool FollowPath()
    {
        if (currentPath == null || currentWaypointIndex >= currentPath.vectorPath.Count)
        {
            return false;
        }

        Vector3 waypoint = currentPath.vectorPath[currentWaypointIndex];
        Vector3 dir = (waypoint - transform.position).normalized;

        Collider2D obstacle = Physics2D.OverlapPoint(waypoint, obstacleLayer);
        if (obstacle != null)
        {
            Debug.Log($"Nemico bloccato dall'ostacolo: {obstacle.name}");
            ChangeState(new ObstacleInteractionState(this, obstacle.gameObject));
            return false;
        }

        float speedUsed = creature.HasLegs() ? moveSpeed : moveSpeed * 0.4f;
        transform.position += dir * speedUsed * Time.deltaTime;

        float dist = Vector2.Distance(transform.position, waypoint);
        if (dist < nextWaypointDistance)
        {
            currentWaypointIndex++;
        }

        return true;
    }


    private Collider2D GetBlockingObstacle(Collider2D[] obstacles, Vector3 waypoint)
{
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player == null) return null;

    Collider2D blockingObstacle = null;
    float minDistance = float.MaxValue;

    foreach (var obstacle in obstacles)
    {
        float distanceToObstacle = Vector3.Distance(transform.position, obstacle.transform.position);

        Bounds obstacleBounds = obstacle.bounds;
        GraphUpdateObject guo = new GraphUpdateObject(obstacleBounds)
        {
            modifyWalkability = true,
            setWalkability = true
        };
        AstarPath.active.UpdateGraphs(guo);

        var path = ABPath.Construct(transform.position, player.transform.position, null);
        seeker.StartPath(path);
        AstarPath.WaitForPath(path);

        GraphUpdateObject restoreGuo = new GraphUpdateObject(obstacleBounds)
        {
            modifyWalkability = true,
            setWalkability = false
        };
        AstarPath.active.UpdateGraphs(restoreGuo);

        if (path.error && distanceToObstacle < minDistance)
        {
            minDistance = distanceToObstacle;
            blockingObstacle = obstacle;
        }
    }

    if (blockingObstacle != null)
    {
        Debug.Log($"Ostacolo selezionato come bloccante: {blockingObstacle.name}");
    }

    return blockingObstacle;
}




    public void NotifyStateChange(EnemyController.EnemyState state)
    {
        if (enemyController != null)
        {
            enemyController.ChangeState(state);
        }
    }

}