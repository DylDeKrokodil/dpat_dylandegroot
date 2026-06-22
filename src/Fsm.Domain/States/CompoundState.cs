namespace Fsm.Domain.States;

public sealed class CompoundState : State
{
    private readonly List<State> _children = [];

    public CompoundState(string id, string displayName, State? parent = null)
        : base(id, displayName, parent, StateType.Compound)
    {
    }

    public IReadOnlyCollection<State> Children => _children.AsReadOnly();

    public void AddChild(State child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (!ReferenceEquals(child.Parent, this))
        {
            throw new DomainException($"State '{child.Id}' does not declare '{Id}' as its parent.");
        }

        if (_children.Any(existing => existing.Id == child.Id))
        {
            throw new DomainException($"Compound state '{Id}' already contains child state '{child.Id}'.");
        }

        _children.Add(child);
    }
}
