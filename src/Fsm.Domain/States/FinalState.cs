namespace Fsm.Domain.States;

using Fsm.Domain.Visitors;

public sealed class FinalState : State
{
    public FinalState(string id, string displayName, State? parent = null)
        : base(id, displayName, parent, StateType.Final)
    {
    }

    public override void Accept(IFsmElementVisitor visitor)
    {
        visitor.VisitFinalState(this);
    }
}
