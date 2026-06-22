using Fsm.Parsing;
using Fsm.Tests.Fixtures;
using Fsm.Validation;

namespace Fsm.Tests.Validation;

public class UnreachableStateValidatorTests
{
    private readonly FsmTextParser _parser = new();
    private readonly UnreachableStateValidator _validator = new();

    [Fact]
    public void ValidateRejectsUnreachableSample()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("invalid_unreachable.fsm"));

        var errors = _validator.Validate(diagram).ToList();

        var error = Assert.Single(errors);
        Assert.Equal("unreachable_state", error.Code);
        Assert.Equal("state2", error.ElementId);
    }

    [Fact]
    public void ValidateTreatsCompoundContainersAsReachableWhenAChildIsReached()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("valid_compound.fsm"));

        var errors = _validator.Validate(diagram);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateSkipsDiagramsWithoutInitialState()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("valid_deterministic.fsm"));

        var errors = _validator.Validate(diagram);

        Assert.Empty(errors);
    }
}
