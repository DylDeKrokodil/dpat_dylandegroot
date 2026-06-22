using Fsm.Parsing;
using Fsm.Tests.Fixtures;
using Fsm.Validation;

namespace Fsm.Tests.Validation;

public class InitialFinalTransitionValidatorTests
{
    private readonly FsmTextParser _parser = new();
    private readonly InitialFinalTransitionValidator _validator = new();

    [Fact]
    public void ValidateRejectsIncomingTransitionsToInitialStates()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("invalid_initial.fsm"));

        var errors = _validator.Validate(diagram).ToList();

        var error = Assert.Single(errors);
        Assert.Equal("incoming_transition_to_initial_state", error.Code);
        Assert.Equal("t2", error.ElementId);
    }

    [Fact]
    public void ValidateRejectsOutgoingTransitionsFromFinalStates()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("invalid_final.fsm"));

        var errors = _validator.Validate(diagram).ToList();

        var error = Assert.Single(errors);
        Assert.Equal("outgoing_transition_from_final_state", error.Code);
        Assert.Equal("t3", error.ElementId);
    }
}
