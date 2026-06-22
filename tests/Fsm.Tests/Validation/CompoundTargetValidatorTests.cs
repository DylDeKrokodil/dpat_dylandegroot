using Fsm.Parsing;
using Fsm.Tests.Fixtures;
using Fsm.Validation;

namespace Fsm.Tests.Validation;

public class CompoundTargetValidatorTests
{
    private readonly FsmTextParser _parser = new();
    private readonly CompoundTargetValidator _validator = new();

    [Fact]
    public void ValidateRejectsTransitionsEndingAtCompoundStates()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("invalid_compound.fsm"));

        var errors = _validator.Validate(diagram).ToList();

        var error = Assert.Single(errors);
        Assert.Equal("transition_targets_compound_state", error.Code);
        Assert.Equal("t1", error.ElementId);
    }

    [Fact]
    public void ValidateAcceptsTransitionsEndingInsideCompoundStates()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("valid_compound.fsm"));

        var errors = _validator.Validate(diagram);

        Assert.Empty(errors);
    }
}
