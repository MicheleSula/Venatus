using UnityEngine;
using Pathfinding;

public class ChaseState : MovementState
{
    private float recalcTimer = 0f;
    private float recalcInterval = 1f;

    public ChaseState(MovementFSM fsm) : base(fsm) { }

    public override void OnEnter()
    {
        base.OnEnter();
        fsm.NotifyStateChange(EnemyController.EnemyState.Chase);

        if (fsm.animator != null)
        {
            fsm.animator.SetBool("isChasing", true);
            fsm.animator.SetBool("isPatrolling", false);
        }

        RequestChasePath();
    }

    public override void OnExit()
    {
        base.OnExit();
        if (fsm.animator != null)
        {
            fsm.animator.SetBool("isChasing", false);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        recalcTimer -= Time.deltaTime;
        if (recalcTimer <= 0f)
        {
            recalcTimer = recalcInterval;
            RequestChasePath();
        }

        bool following = fsm.FollowPath();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        float dist = Vector2.Distance(fsm.transform.position, playerObj.transform.position);
        if (dist > fsm.detectionRange * 1.5f)
        {
            fsm.ChangeState(new RandomPatrolState(fsm));
        }
    }

    private void RequestChasePath()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        fsm.RequestPath(fsm.transform.position, playerObj.transform.position);
    }
}