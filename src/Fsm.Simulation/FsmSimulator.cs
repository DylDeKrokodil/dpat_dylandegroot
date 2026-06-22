using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Simulation;

public sealed class FsmSimulator
{
    private readonly FsmDiagram _diagram;
    private readonly List<string> _log = [];

    public FsmSimulator(FsmDiagram diagram)
    {
        _diagram = diagram;
        CurrentState = diagram.InitialState
            ?? throw new SimulationException("Simulation requires an initial state.");

        _log.Add($"Simulation started at state '{CurrentState.Id}'.");
        LogStateActions(CurrentState, ActionType.EntryAction);
        LogStateActions(CurrentState, ActionType.DoAction);
    }

    public State CurrentState { get; private set; }

    public IReadOnlyList<string> Log => _log.AsReadOnly();

    public bool IsFinished => CurrentState is FinalState;

    public IReadOnlyList<Transition> GetAvailableTransitions()
    {
        var states = GetCurrentStateAndAncestors(CurrentState);

        return states
            .SelectMany(state => _diagram.GetOutgoingTransitions(state))
            .DistinctBy(transition => transition.Id)
            .ToList();
    }

    public SimulationStepResult TryFollow(Transition transition, bool guardAccepted)
    {
        if (!GetAvailableTransitions().Contains(transition))
        {
            throw new SimulationException($"Transition '{transition.Id}' is not available from current state '{CurrentState.Id}'.");
        }

        if (!transition.Guard.IsEmpty && !guardAccepted)
        {
            var rejectedEvents = new[]
            {
                $"Guard '{transition.Guard.Expression}' rejected for transition '{transition.Id}'."
            };

            _log.AddRange(rejectedEvents);
            return new SimulationStepResult(false, CurrentState, transition, rejectedEvents);
        }

        var events = new List<string>
        {
            $"Taking transition '{transition.Id}' from '{transition.Source.Id}' to '{transition.Destination.Id}' using {FormatTrigger(transition)} and {FormatGuard(transition)}."
        };

        events.AddRange(FormatStateActions(CurrentState, ActionType.ExitAction));

        if (transition.Effect is not null)
        {
            events.Add($"Effect: {transition.Effect.Description}");
        }

        CurrentState = transition.Destination;
        events.Add($"Current state is now '{CurrentState.Id}'.");
        events.AddRange(FormatStateActions(CurrentState, ActionType.EntryAction));
        events.AddRange(FormatStateActions(CurrentState, ActionType.DoAction));

        _log.AddRange(events);
        return new SimulationStepResult(true, CurrentState, transition, events);
    }

    private static IEnumerable<State> GetCurrentStateAndAncestors(State state)
    {
        var current = state;

        while (current is not null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    private void LogStateActions(State state, ActionType type)
    {
        _log.AddRange(FormatStateActions(state, type));
    }

    private static IEnumerable<string> FormatStateActions(State state, ActionType type)
    {
        return state.Actions
            .Where(action => action.Type == type)
            .Select(action => $"{FormatActionType(type)} action on '{state.Id}': {action.Description}");
    }

    private static string FormatActionType(ActionType type)
    {
        return type switch
        {
            ActionType.EntryAction => "Entry",
            ActionType.DoAction => "Do",
            ActionType.ExitAction => "Exit",
            _ => type.ToString()
        };
    }

    private static string FormatTrigger(Transition transition)
    {
        return transition.Trigger is null
            ? "automatic trigger"
            : $"trigger '{transition.Trigger.Id}'";
    }

    private static string FormatGuard(Transition transition)
    {
        return transition.Guard.IsEmpty
            ? "no guard"
            : $"accepted guard '{transition.Guard.Expression}'";
    }
}
