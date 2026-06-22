using Fsm.Domain;

namespace Fsm.Validation;

public sealed class DeterministicTransitionValidator : IFsmValidator
{
    public IEnumerable<ValidationError> Validate(FsmDiagram diagram)
    {
        foreach (var state in diagram.States)
        {
            var outgoingTransitions = diagram.GetOutgoingTransitions(state).ToList();

            if (outgoingTransitions.Count <= 1)
            {
                continue;
            }

            foreach (var error in ValidateAutomaticTransitions(state.Id, outgoingTransitions))
            {
                yield return error;
            }

            foreach (var error in ValidateTriggeredTransitions(state.Id, outgoingTransitions))
            {
                yield return error;
            }
        }
    }

    private static IEnumerable<ValidationError> ValidateAutomaticTransitions(string stateId, IReadOnlyList<Transition> outgoingTransitions)
    {
        var automaticTransitions = outgoingTransitions.Where(transition => transition.IsAutomatic).ToList();

        if (automaticTransitions.Count == 0)
        {
            yield break;
        }

        var unconditionalAutomaticTransitions = automaticTransitions
            .Where(transition => transition.Guard.IsEmpty)
            .ToList();

        if (unconditionalAutomaticTransitions.Count > 0 && outgoingTransitions.Count > unconditionalAutomaticTransitions.Count)
        {
            yield return new ValidationError(
                "non_deterministic_automatic_transition",
                $"State '{stateId}' mixes unconditional automatic transitions with other transitions.",
                stateId);
        }

        foreach (var guardGroup in automaticTransitions.GroupBy(transition => transition.Guard.Expression))
        {
            var conflictingTransitions = guardGroup.ToList();

            if (conflictingTransitions.Count > 1)
            {
                yield return new ValidationError(
                    "non_deterministic_automatic_transition",
                    $"State '{stateId}' has conflicting automatic transitions: {string.Join(", ", conflictingTransitions.Select(transition => transition.Id))}.",
                    stateId);
            }
        }
    }

    private static IEnumerable<ValidationError> ValidateTriggeredTransitions(string stateId, IReadOnlyList<Transition> outgoingTransitions)
    {
        var groupedTransitions = outgoingTransitions
            .Where(transition => transition.Trigger is not null)
            .GroupBy(transition => transition.Trigger!.Id);

        foreach (var triggerGroup in groupedTransitions)
        {
            var transitions = triggerGroup.ToList();

            for (var leftIndex = 0; leftIndex < transitions.Count; leftIndex++)
            {
                for (var rightIndex = leftIndex + 1; rightIndex < transitions.Count; rightIndex++)
                {
                    var left = transitions[leftIndex];
                    var right = transitions[rightIndex];

                    if (GuardsConflict(left.Guard, right.Guard))
                    {
                        yield return new ValidationError(
                            "non_deterministic_transition",
                            $"State '{stateId}' has conflicting transitions '{left.Id}' and '{right.Id}' for trigger '{triggerGroup.Key}'.",
                            stateId);
                    }
                }
            }
        }
    }

    private static bool GuardsConflict(Guard left, Guard right)
    {
        return left.IsEmpty || right.IsEmpty || string.Equals(left.Expression, right.Expression, StringComparison.Ordinal);
    }
}
