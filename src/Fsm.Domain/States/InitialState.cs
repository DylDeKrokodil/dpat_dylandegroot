namespace Fsm.Domain.States;

using Fsm.Domain.Visitors;

public sealed class InitialState : State
{
    public InitialState(string id, string displayName, State? parent = null)
        : base(id, displayName, parent, StateType.Initial)
    {
    }

    public override void Accept(IFsmElementVisitor visitor)
    {
        visitor.VisitInitialState(this);
    }
}
