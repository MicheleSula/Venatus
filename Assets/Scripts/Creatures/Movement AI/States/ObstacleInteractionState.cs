using UnityEngine;

public class ObstacleInteractionState : MovementState
{
    private GameObject targetObstacle;
    private float attackCooldown = 1.0f;
    private float attackTimer = 0f;
    private int attackCounter = 0;
    private int requiredHits = 3;

    public ObstacleInteractionState(MovementFSM fsm, GameObject obstacle) : base(fsm)
    {
        targetObstacle = obstacle;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        fsm.NotifyStateChange(EnemyController.EnemyState.InteractingWithObstacle);

        if (targetObstacle != null)
        {
            Debug.Log($"Inizio distruzione ostacolo: {targetObstacle.name}");
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (targetObstacle == null)
        {
            Debug.Log("Ostacolo distrutto. Tornando a cercare il bersaglio.");
            fsm.ChangeState(new ChaseState(fsm));
            return;
        }

        float distance = Vector2.Distance(fsm.transform.position, targetObstacle.transform.position);
        if (distance > 0.5f)
        {
            Vector3 direction = (targetObstacle.transform.position - fsm.transform.position).normalized;
            fsm.transform.position += direction * fsm.moveSpeed * Time.deltaTime;
            Debug.Log("Avvicinamento all'ostacolo...");
            return;
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            attackCounter++;

            Debug.Log($"Colpendo ostacolo: {targetObstacle.name}, colpo {attackCounter}/{requiredHits}");

            Obstacle obstacle = targetObstacle.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                obstacle.TakeDamage(50f);
            }

            if (attackCounter >= requiredHits || (obstacle != null && obstacle.IsDestroyed()))
            {
                Debug.Log("Ostacolo distrutto.");
                Object.Destroy(targetObstacle);
                fsm.ChangeState(new ChaseState(fsm));
            }
        }
    }


    public override void OnExit()
    {
        base.OnExit();
        targetObstacle = null;
        attackCounter = 0;
    }
}