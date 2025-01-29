using UnityEngine;
using Pathfinding;

public class RandomPatrolState : MovementState
{
    private Vector3 randomDestination;
    private float interval = 3f;
    private float chooseTimer;

    public RandomPatrolState(MovementFSM fsm) : base(fsm) { }

    public override void OnEnter()
    {
        base.OnEnter();
        fsm.NotifyStateChange(EnemyController.EnemyState.Patrol);

        if (fsm.animator != null)
        {
            fsm.animator.SetBool("isChasing", false);
            fsm.animator.SetBool("isPatrolling", true);
        }

        ChooseNewDestination();
        chooseTimer = interval;
    }

    public override void OnExit()
    {
        base.OnExit();
        if (fsm.animator != null)
        {
            fsm.animator.SetBool("isPatrolling", false);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        bool following = fsm.FollowPath();
        if (!following)
        {
            chooseTimer -= Time.deltaTime;
            if (chooseTimer <= 0f)
            {
                ChooseNewDestination();
                chooseTimer = interval;
            }
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            float distFromPlayer = Vector2.Distance(fsm.transform.position, playerObj.transform.position);
            if (distFromPlayer < fsm.detectionRange)
            {
                fsm.ChangeState(new ChaseState(fsm));
            }
        }
    }

    private void ChooseNewDestination()
    {
        Vector2 randomOffset = Random.insideUnitCircle * 5f;
        randomDestination = fsm.transform.position + (Vector3)randomOffset;

        fsm.RequestPath(fsm.transform.position, randomDestination);
    }
}