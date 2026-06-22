namespace Fsm.Domain.States;

public sealed class SimpleState : State
{
    public SimpleState(string id, string displayName, State? parent = null)
        : base(id, displayName, parent, StateType.Simple)
    {
    }
}
