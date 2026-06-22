using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Validation;

public sealed class CompoundTargetValidator : IFsmValidator
{
    public IEnumerable<ValidationError> Validate(FsmDiagram diagram)
    {
        foreach (var transition in diagram.Transitions.Where(transition => transition.Destination is CompoundState))
        {
            yield return new ValidationError(
                "transition_targets_compound_state",
                $"Transition '{transition.Id}' ends at compound state '{transition.Destination.Id}'.",
                transition.Id);
        }
    }
}
