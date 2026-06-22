using Fsm.Application;
using Fsm.Parsing;
using Fsm.Presentation;
using Fsm.Tests.Fixtures;
using Fsm.Validation;

namespace Fsm.Tests.Application;

public class FsmApplicationTests
{
    [Fact]
    public void RunRendersValidDiagramAndReturnsSuccess()
    {
        var userInterface = new FakeUserInterface();
        var application = CreateApplication(userInterface);

        var exitCode = application.Run([SampleFsmFiles.PathFor("example_lamp.fsm")]);

        Assert.Equal(FsmApplication.SuccessExitCode, exitCode);
        Assert.Contains("FSM Diagram", userInterface.Output);
        Assert.Contains("Lamp is off", userInterface.Output);
    }

    [Fact]
    public void RunResolvesRepoRootRelativeSamplePaths()
    {
        var userInterface = new FakeUserInterface();
        var application = CreateApplication(userInterface);

        var exitCode = application.Run([Path.Combine("Test FSMs", "example_lamp.fsm")]);

        Assert.Equal(FsmApplication.SuccessExitCode, exitCode);
        Assert.Contains("FSM Diagram", userInterface.Output);
    }

    [Fact]
    public void RunPrintsValidationErrorsAndReturnsValidationExitCode()
    {
        var userInterface = new FakeUserInterface();
        var application = CreateApplication(userInterface);

        var exitCode = application.Run([SampleFsmFiles.PathFor("invalid_initial.fsm")]);

        Assert.Equal(FsmApplication.ValidationErrorExitCode, exitCode);
        Assert.Contains("Validation errors:", userInterface.Output);
        Assert.Contains("incoming_transition_to_initial_state", userInterface.Output);
    }

    [Fact]
    public void RunPrintsParseErrorsAndReturnsParseExitCode()
    {
        var userInterface = new FakeUserInterface();
        var application = CreateApplication(userInterface);

        var exitCode = application.Run(["does-not-exist.fsm"]);

        Assert.Equal(FsmApplication.ParseErrorExitCode, exitCode);
        Assert.Contains("Input error:", userInterface.Output);
    }

    [Fact]
    public void RunCanSimulateOneTransitionAndStop()
    {
        var userInterface = new FakeUserInterface(["1", "q"]);
        var application = CreateApplication(userInterface);

        var exitCode = application.Run([SampleFsmFiles.PathFor("example_lamp.fsm"), "--simulate"]);

        Assert.Equal(FsmApplication.SuccessExitCode, exitCode);
        Assert.Contains("Simulation started.", userInterface.Output);
        Assert.Contains("Taking transition 't1' from 'initial' to 'off'.", userInterface.Output);
        Assert.Contains("Entry action on 'off': Start off timer", userInterface.Output);
        Assert.Contains("Simulation log:", userInterface.Output);
    }

    [Fact]
    public void RunPrintsSimulationErrorWhenDiagramCannotBeSimulated()
    {
        var userInterface = new FakeUserInterface();
        var application = CreateApplication(userInterface);

        var exitCode = application.Run([SampleFsmFiles.PathFor("valid_deterministic.fsm"), "--simulate"]);

        Assert.Equal(FsmApplication.SimulationErrorExitCode, exitCode);
        Assert.Contains("Simulation error: Simulation requires an initial state.", userInterface.Output);
    }

    private static FsmApplication CreateApplication(IUserInterface userInterface)
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

    private sealed class FakeUserInterface(IEnumerable<string>? inputs = null) : IUserInterface
    {
        private readonly List<string> _messages = [];
        private readonly Queue<string> _inputs = new(inputs ?? []);

        public string Output => string.Join(Environment.NewLine, _messages);

        public string? ReadInputFilePath()
        {
            return null;
        }

        public string? ReadLine()
        {
            return _inputs.Count > 0 ? _inputs.Dequeue() : null;
        }

        public void WriteLine(string message = "")
        {
            _messages.Add(message);
        }
    }
}
