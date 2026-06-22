using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Validation;

public sealed class UnreachableStateValidator : IFsmValidator
{
    public IEnumerable<ValidationError> Validate(FsmDiagram diagram)
    {
        if (diagram.InitialState is null)
        {
            yield return new ValidationError(
                "missing_initial_state",
                "FSM does not contain an initial state.");
            yield break;
        }

        var reachableStates = FindReachableStates(diagram, diagram.InitialState);

        foreach (var unreachableState in diagram.States.Where(state => !reachableStates.Contains(state)))
        {
            yield return new ValidationError(
                "unreachable_state",
                $"State '{unreachableState.Id}' is unreachable from initial state '{diagram.InitialState.Id}'.",
                unreachableState.Id);
        }
    }

    private static HashSet<State> FindReachableStates(FsmDiagram diagram, State initialState)
    {
        var reachableStates = new HashSet<State>();
        var workQueue = new Queue<State>();

        MarkReachable(initialState, reachableStates, workQueue);

        while (workQueue.Count > 0)
        {
            var state = workQueue.Dequeue();

            foreach (var transition in diagram.GetOutgoingTransitions(state))
            {
                MarkReachable(transition.Destination, reachableStates, workQueue);
            }
        }

        return reachableStates;
    }

    private static void MarkReachable(State state, HashSet<State> reachableStates, Queue<State> workQueue)
    {
        MarkStateAndAncestors(state, reachableStates, workQueue);

        if (state is CompoundState compoundState)
        {
            foreach (var child in compoundState.Children)
            {
                MarkReachable(child, reachableStates, workQueue);
            }
        }
    }

    private static void MarkStateAndAncestors(State state, HashSet<State> reachableStates, Queue<State> workQueue)
    {
        var current = state;

        while (current is not null)
        {
            if (reachableStates.Add(current))
            {
                workQueue.Enqueue(current);

                if (current is CompoundState compoundState)
                {
                    foreach (var child in compoundState.Children)
                    {
                        MarkReachable(child, reachableStates, workQueue);
                    }
                }
            }

            current = current.Parent;
        }
    }
}
