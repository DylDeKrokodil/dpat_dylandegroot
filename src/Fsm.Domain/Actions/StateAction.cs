using Fsm.Domain.States;

namespace Fsm.Domain;

public sealed class StateAction : FsmAction
{
    private static readonly HashSet<ActionType> AllowedTypes =
    [
        ActionType.EntryAction,
        ActionType.DoAction,
        ActionType.ExitAction
    ];

    public StateAction(State owner, string description, ActionType type)
        : base(description, type)
    {
        ArgumentNullException.ThrowIfNull(owner);

        if (!AllowedTypes.Contains(type))
        {
            throw new DomainException($"Action type '{type}' cannot be attached to a state.");
        }

        Owner = owner;
    }

    public State Owner { get; }
}
