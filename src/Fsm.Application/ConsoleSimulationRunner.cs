using Fsm.Domain;
using Fsm.Simulation;

namespace Fsm.Application;

public sealed class ConsoleSimulationRunner : ISimulationRunner
{
    private readonly IUserInterface _userInterface;

    public ConsoleSimulationRunner(IUserInterface userInterface)
    {
        _userInterface = userInterface;
    }

    public void Run(FsmDiagram diagram)
    {
        var simulator = new FsmSimulator(diagram);

        _userInterface.WriteLine();
        _userInterface.WriteLine("Simulation started.");

        while (!simulator.IsFinished)
        {
            _userInterface.WriteLine();
            _userInterface.WriteLine($"Current state: {simulator.CurrentState.Id}");

            var availableTransitions = simulator.GetAvailableTransitions();

            if (availableTransitions.Count == 0)
            {
                _userInterface.WriteLine("No available transitions. Simulation stopped.");
                break;
            }

            _userInterface.WriteLine("Available triggers/transitions:");

            for (var index = 0; index < availableTransitions.Count; index++)
            {
                _userInterface.WriteLine($"{index + 1}. {FormatTransitionChoice(availableTransitions[index])}");
            }

            _userInterface.WriteLine("Choose trigger/transition number, or q to quit:");
            var choice = _userInterface.ReadLine();

            if (string.Equals(choice, "q", StringComparison.OrdinalIgnoreCase))
            {
                _userInterface.WriteLine("Simulation stopped by user.");
                break;
            }

            if (!int.TryParse(choice, out var selectedNumber)
                || selectedNumber < 1
                || selectedNumber > availableTransitions.Count)
            {
                _userInterface.WriteLine("Invalid transition choice.");
                continue;
            }

            var selectedTransition = availableTransitions[selectedNumber - 1];
            var guardAccepted = AskGuardIfNeeded(selectedTransition);
            var result = simulator.TryFollow(selectedTransition, guardAccepted);

            foreach (var simulationEvent in result.Events)
            {
                _userInterface.WriteLine(simulationEvent);
            }
        }

        if (simulator.IsFinished)
        {
            _userInterface.WriteLine();
            _userInterface.WriteLine($"Final state reached: {simulator.CurrentState.Id}");
        }

        _userInterface.WriteLine();
        _userInterface.WriteLine("Simulation log:");

        foreach (var logEntry in simulator.Log)
        {
            _userInterface.WriteLine($"- {logEntry}");
        }
    }

    private bool AskGuardIfNeeded(Transition transition)
    {
        if (transition.Guard.IsEmpty)
        {
            return true;
        }

        _userInterface.WriteLine($"Is guard \"{transition.Guard.Expression}\" true? y/n");
        var answer = _userInterface.ReadLine();

        return string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(answer, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatTransitionChoice(Transition transition)
    {
        var trigger = transition.Trigger is null ? "automatic" : transition.Trigger.Id;
        var guard = transition.Guard.IsEmpty ? "" : $" [{transition.Guard.Expression}]";
        var effect = transition.Effect is null ? "" : $" / {transition.Effect.Description}";

        return $"{transition.Id}: {transition.Source.Id} -> {transition.Destination.Id} on {trigger}{guard}{effect}";
    }
}
