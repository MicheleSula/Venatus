public abstract class MovementState
{
    protected MovementFSM fsm;
    protected Creature creature;

    public MovementState(MovementFSM fsm)
    {
        this.fsm = fsm;
        this.creature = fsm.creature;
    }

    public virtual void OnEnter()  { }
    public virtual void LogicUpdate() { }
    public virtual void OnExit()   { }
}