namespace Fsm.Domain.States;

public sealed class FinalState : State
{
    public FinalState(string id, string displayName, State? parent = null)
        : base(id, displayName, parent, StateType.Final)
    {
    }
}
