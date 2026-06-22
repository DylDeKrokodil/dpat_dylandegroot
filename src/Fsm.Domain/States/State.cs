namespace Fsm.Domain.States;

public abstract class State
{
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

    public bool IsNested => Parent is not null;

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
}
