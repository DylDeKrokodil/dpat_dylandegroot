using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Building;

public sealed class FsmModelBuilder
{
    private readonly FsmDiagram _diagram = new();
    private readonly StateFactory _stateFactory;
    private readonly HashSet<string> _elementIds = new(StringComparer.Ordinal);

    public FsmModelBuilder()
        : this(new StateFactory())
    {
    }

    public FsmModelBuilder(StateFactory stateFactory)
    {
        _stateFactory = stateFactory;
    }

    public FsmDiagram Diagram => _diagram;

    public State AddState(string id, string parentId, string name, StateType type)
    {
        ReserveElementId(id, "state");

        var parent = ResolveParent(parentId, id);
        var state = _stateFactory.Create(id, name, type, parent);

        try
        {
            _diagram.AddState(state);
        }
        catch
        {
            _elementIds.Remove(id);
            throw;
        }

        return state;
    }

    public Trigger AddTrigger(string id, string description)
    {
        ReserveElementId(id, "trigger");

        var trigger = new Trigger(id, description);

        try
        {
            _diagram.AddTrigger(trigger);
        }
        catch
        {
            _elementIds.Remove(id);
            throw;
        }

        return trigger;
    }

    public Transition AddTransition(string id, string sourceId, string destinationId, string? triggerId, string guard)
    {
        ReserveElementId(id, "transition");

        var source = RequireState(sourceId, $"Transition '{id}' source state '{sourceId}' does not exist.");
        var destination = RequireState(destinationId, $"Transition '{id}' destination state '{destinationId}' does not exist.");
        var trigger = ResolveTrigger(id, triggerId);
        var transition = new Transition(id, source, destination, trigger, new Guard(guard));

        try
        {
            _diagram.AddTransition(transition);
        }
        catch
        {
            _elementIds.Remove(id);
            throw;
        }

        return transition;
    }

    public FsmAction AddAction(string ownerId, string description, ActionType type)
    {
        return type == ActionType.TransitionAction
            ? AddTransitionAction(ownerId, description)
            : AddStateAction(ownerId, description, type);
    }

    public FsmDiagram Build()
    {
        return _diagram;
    }

    private void ReserveElementId(string id, string elementType)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ModelBuildingException($"{elementType} id is required.");
        }

        if (!_elementIds.Add(id))
        {
            throw new ModelBuildingException($"Element id '{id}' already exists.");
        }
    }

    private State? ResolveParent(string parentId, string childId)
    {
        if (parentId == "_")
        {
            return null;
        }

        var parent = RequireState(parentId, $"Parent state '{parentId}' for state '{childId}' does not exist.");

        if (parent is not CompoundState)
        {
            throw new ModelBuildingException($"Parent state '{parentId}' for state '{childId}' must be a compound state.");
        }

        return parent;
    }

    private State RequireState(string id, string message)
    {
        return _diagram.FindState(id) ?? throw new ModelBuildingException(message);
    }

    private Trigger? ResolveTrigger(string transitionId, string? triggerId)
    {
        if (string.IsNullOrWhiteSpace(triggerId))
        {
            return null;
        }

        return _diagram.FindTrigger(triggerId)
            ?? throw new ModelBuildingException($"Transition '{transitionId}' trigger '{triggerId}' does not exist.");
    }

    private StateAction AddStateAction(string ownerId, string description, ActionType type)
    {
        var owner = RequireState(ownerId, $"Action owner state '{ownerId}' does not exist.");
        var action = new StateAction(owner, description, type);

        owner.AddAction(action);

        return action;
    }

    private TransitionAction AddTransitionAction(string ownerId, string description)
    {
        var owner = _diagram.Transitions.FirstOrDefault(transition => transition.Id == ownerId)
            ?? throw new ModelBuildingException($"Action owner transition '{ownerId}' does not exist.");

        var action = new TransitionAction(owner, description);
        owner.SetEffect(action);

        return action;
    }
}
