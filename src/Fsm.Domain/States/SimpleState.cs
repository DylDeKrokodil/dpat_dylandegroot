namespace Fsm.Domain.States;

using Fsm.Domain.Visitors;

public sealed class SimpleState : State
{
    public SimpleState(string id, string displayName, State? parent = null)
        : base(id, displayName, parent, StateType.Simple)
    {
    }

    public override void Accept(IFsmElementVisitor visitor)
    {
        visitor.VisitSimpleState(this);
    }
}
