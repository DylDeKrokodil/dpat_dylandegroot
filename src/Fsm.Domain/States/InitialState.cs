namespace Fsm.Domain.States;

public sealed class InitialState : State
{
    public InitialState(string id, string displayName, State? parent = null)
        : base(id, displayName, parent, StateType.Initial)
    {
    }
}
