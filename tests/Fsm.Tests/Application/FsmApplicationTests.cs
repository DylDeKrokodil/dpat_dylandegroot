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

    private sealed class FakeUserInterface : IUserInterface
    {
        private readonly List<string> _messages = [];

        public string Output => string.Join(Environment.NewLine, _messages);

        public string? ReadInputFilePath()
        {
            return null;
        }

        public void WriteLine(string message = "")
        {
            _messages.Add(message);
        }
    }
}
