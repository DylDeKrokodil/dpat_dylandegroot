namespace Fsm.Domain;

public sealed class TransitionAction : FsmAction
{
    public TransitionAction(Transition owner, string description)
        : base(description, ActionType.TransitionAction)
    {
        Owner = owner;
    }

    public Transition Owner { get; }
}
