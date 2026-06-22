using Fsm.Domain.States;
using Fsm.Domain.Visitors;

namespace Fsm.Domain;

public sealed class FsmDiagram
{
    private readonly Dictionary<string, State> _states = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Trigger> _triggers = new(StringComparer.Ordinal);
    private readonly List<Transition> _transitions = [];

    public IReadOnlyCollection<State> States => _states.Values;

    public IReadOnlyCollection<Trigger> Triggers => _triggers.Values;

    public IReadOnlyCollection<Transition> Transitions => _transitions.AsReadOnly();

    public InitialState? InitialState => _states.Values.OfType<InitialState>().FirstOrDefault();

    public void AddState(State state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!_states.TryAdd(state.Id, state))
        {
            throw new DomainException($"State '{state.Id}' already exists.");
        }

        if (state.Parent is CompoundState parent)
        {
            if (!_states.ContainsKey(parent.Id))
            {
                throw new DomainException($"Parent state '{parent.Id}' must be added before child state '{state.Id}'.");
            }

            parent.AddChild(state);
        }
    }

    public void AddTrigger(Trigger trigger)
    {
        ArgumentNullException.ThrowIfNull(trigger);

        if (!_triggers.TryAdd(trigger.Id, trigger))
        {
            throw new DomainException($"Trigger '{trigger.Id}' already exists.");
        }
    }

    public void AddTransition(Transition transition)
    {
        ArgumentNullException.ThrowIfNull(transition);

        if (_transitions.Any(existing => existing.Id == transition.Id))
        {
            throw new DomainException($"Transition '{transition.Id}' already exists.");
        }

        if (!_states.ContainsKey(transition.Source.Id))
        {
            throw new DomainException($"Transition '{transition.Id}' source state '{transition.Source.Id}' is not part of the diagram.");
        }

        if (!_states.ContainsKey(transition.Destination.Id))
        {
            throw new DomainException($"Transition '{transition.Id}' destination state '{transition.Destination.Id}' is not part of the diagram.");
        }

        if (transition.Trigger is not null && !_triggers.ContainsKey(transition.Trigger.Id))
        {
            throw new DomainException($"Transition '{transition.Id}' trigger '{transition.Trigger.Id}' is not part of the diagram.");
        }

        _transitions.Add(transition);
    }

    public State? FindState(string id)
    {
        return _states.GetValueOrDefault(id);
    }

    public Trigger? FindTrigger(string id)
    {
        return _triggers.GetValueOrDefault(id);
    }

    public IEnumerable<Transition> GetOutgoingTransitions(State state)
    {
        return _transitions.Where(transition => ReferenceEquals(transition.Source, state));
    }

    public IEnumerable<Transition> GetIncomingTransitions(State state)
    {
        return _transitions.Where(transition => ReferenceEquals(transition.Destination, state));
    }

    public void Accept(IFsmElementVisitor visitor)
    {
        visitor.VisitDiagram(this);
    }
}
