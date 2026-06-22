using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Validation;

public sealed class InitialFinalTransitionValidator : IFsmValidator
{
    public IEnumerable<ValidationError> Validate(FsmDiagram diagram)
    {
        foreach (var transition in diagram.Transitions)
        {
            if (transition.Destination is InitialState)
            {
                yield return new ValidationError(
                    "incoming_transition_to_initial_state",
                    $"Transition '{transition.Id}' ends at initial state '{transition.Destination.Id}'.",
                    transition.Id);
            }

            if (transition.Source is FinalState)
            {
                yield return new ValidationError(
                    "outgoing_transition_from_final_state",
                    $"Transition '{transition.Id}' starts from final state '{transition.Source.Id}'.",
                    transition.Id);
            }
        }
    }
}
