using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Simulation;

public sealed record SimulationStepResult(
    bool TransitionTaken,
    State CurrentState,
    Transition? Transition,
    IReadOnlyList<string> Events);
