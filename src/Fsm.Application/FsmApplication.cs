using Fsm.Parsing;
using Fsm.Presentation;
using Fsm.Simulation;
using Fsm.Validation;

namespace Fsm.Application;

public sealed class FsmApplication
{
    public const int SuccessExitCode = 0;
    public const int MissingInputExitCode = 1;
    public const int ParseErrorExitCode = 2;
    public const int ValidationErrorExitCode = 3;
    public const int SimulationErrorExitCode = 4;

    private readonly IFsmParser _parser;
    private readonly ValidationPipeline _validationPipeline;
    private readonly IFsmRenderer _renderer;
    private readonly IUserInterface _userInterface;

    public FsmApplication(
        IFsmParser parser,
        ValidationPipeline validationPipeline,
        IFsmRenderer renderer,
        IUserInterface userInterface)
    {
        _parser = parser;
        _validationPipeline = validationPipeline;
        _renderer = renderer;
        _userInterface = userInterface;
    }

    public static FsmApplication CreateDefault(IUserInterface userInterface)
    {
        return new FsmApplication(
            new FsmTextParser(),
            new ValidationPipeline(
            [
                new DeterministicTransitionValidator(),
                new InitialFinalTransitionValidator(),
                new UnreachableStateValidator(),
                new CompoundTargetValidator()
            ]),
            new ConsoleTextRenderer(),
            userInterface);
    }

    public int Run(string[] args)
    {
        var options = CommandOptions.Parse(args);
        var inputFilePath = ResolveInputFilePath(options);

        if (string.IsNullOrWhiteSpace(inputFilePath))
        {
            _userInterface.WriteLine("No FSM file path provided.");
            return MissingInputExitCode;
        }

        try
        {
            var diagram = _parser.ParseFile(ResolveExistingFilePath(inputFilePath));
            var validationResult = _validationPipeline.Validate(diagram);

            if (!validationResult.IsValid)
            {
                _userInterface.WriteLine("Validation errors:");

                foreach (var error in validationResult.Errors)
                {
                    _userInterface.WriteLine($"- {error.Code}: {error.Message}");
                }

                return ValidationErrorExitCode;
            }

            _userInterface.WriteLine(_renderer.RenderDiagram(diagram));

            if (options.Simulate)
            {
                RunSimulation(diagram);
            }

            return SuccessExitCode;
        }
        catch (ParseException exception)
        {
            _userInterface.WriteLine($"Parse error: {exception.Message}");
            return ParseErrorExitCode;
        }
        catch (IOException exception)
        {
            _userInterface.WriteLine($"Input error: {exception.Message}");
            return ParseErrorExitCode;
        }
        catch (UnauthorizedAccessException exception)
        {
            _userInterface.WriteLine($"Input error: {exception.Message}");
            return ParseErrorExitCode;
        }
        catch (SimulationException exception)
        {
            _userInterface.WriteLine($"Simulation error: {exception.Message}");
            return SimulationErrorExitCode;
        }
    }

    private string? ResolveInputFilePath(CommandOptions options)
    {
        return options.InputFilePath ?? _userInterface.ReadInputFilePath();
    }

    private static string ResolveExistingFilePath(string inputFilePath)
    {
        if (File.Exists(inputFilePath) || Path.IsPathRooted(inputFilePath))
        {
            return inputFilePath;
        }

        foreach (var searchRoot in GetSearchRoots())
        {
            var candidate = FindInAncestorDirectories(searchRoot, inputFilePath);

            if (candidate is not null)
            {
                return candidate;
            }
        }

        return inputFilePath;
    }

    private static IEnumerable<string> GetSearchRoots()
    {
        yield return Environment.CurrentDirectory;
        yield return AppContext.BaseDirectory;
    }

    private static string? FindInAncestorDirectories(string startDirectory, string relativePath)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private void RunSimulation(Fsm.Domain.FsmDiagram diagram)
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

    private bool AskGuardIfNeeded(Fsm.Domain.Transition transition)
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

    private static string FormatTransitionChoice(Fsm.Domain.Transition transition)
    {
        var trigger = transition.Trigger is null ? "automatic" : transition.Trigger.Id;
        var guard = transition.Guard.IsEmpty ? "" : $" [{transition.Guard.Expression}]";
        var effect = transition.Effect is null ? "" : $" / {transition.Effect.Description}";

        return $"{transition.Id}: {transition.Source.Id} -> {transition.Destination.Id} on {trigger}{guard}{effect}";
    }

    private sealed record CommandOptions(string? InputFilePath, bool Simulate)
    {
        public static CommandOptions Parse(string[] args)
        {
            string? inputFilePath = null;
            var simulate = false;

            foreach (var argument in args)
            {
                if (argument is "--simulate" or "-s")
                {
                    simulate = true;
                    continue;
                }

                inputFilePath ??= argument;
            }

            return new CommandOptions(inputFilePath, simulate);
        }
    }
}
