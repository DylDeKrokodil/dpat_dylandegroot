namespace Fsm.Domain.States;

using Fsm.Domain.Visitors;

public abstract class State
{
    private readonly List<StateAction> _actions = [];

    protected State(string id, string displayName, State? parent, StateType type)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainException("State id is required.");
        }

        if (parent is not null and not CompoundState)
        {
            throw new DomainException($"State '{id}' can only be nested inside a compound state.");
        }

        Id = id;
        DisplayName = displayName;
        Parent = parent;
        Type = type;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public State? Parent { get; }

    public StateType Type { get; }

    public IReadOnlyCollection<StateAction> Actions => _actions.AsReadOnly();

    public bool IsNested => Parent is not null;

    public void AddAction(StateAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!ReferenceEquals(action.Owner, this))
        {
            throw new DomainException($"Action '{action.Description}' does not belong to state '{Id}'.");
        }

        _actions.Add(action);
    }

    public bool IsNestedIn(State possibleParent)
    {
        var current = Parent;

        while (current is not null)
        {
            if (ReferenceEquals(current, possibleParent))
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    public abstract void Accept(IFsmElementVisitor visitor);
}
