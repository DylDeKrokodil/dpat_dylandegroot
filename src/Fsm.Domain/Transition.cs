using Fsm.Domain.States;
using Fsm.Domain.Visitors;

namespace Fsm.Domain;

public sealed class Transition
{
    public Transition(string id, State source, State destination, Trigger? trigger, Guard guard)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainException("Transition id is required.");
        }

        Id = id;
        Source = source;
        Destination = destination;
        Trigger = trigger;
        Guard = guard;
    }

    public string Id { get; }

    public State Source { get; }

    public State Destination { get; }

    public Trigger? Trigger { get; }

    public Guard Guard { get; }

    public TransitionAction? Effect { get; private set; }

    public bool IsAutomatic => Trigger is null;

    public bool IsSelfTransition => ReferenceEquals(Source, Destination);

    public void SetEffect(TransitionAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!ReferenceEquals(action.Owner, this))
        {
            throw new DomainException($"Action '{action.Description}' does not belong to transition '{Id}'.");
        }

        Effect = action;
    }

    public void Accept(IFsmElementVisitor visitor)
    {
        visitor.VisitTransition(this);
    }
}
