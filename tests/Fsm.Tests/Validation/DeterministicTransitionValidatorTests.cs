using Fsm.Parsing;
using Fsm.Tests.Fixtures;
using Fsm.Validation;

namespace Fsm.Tests.Validation;

public class DeterministicTransitionValidatorTests
{
    private readonly FsmTextParser _parser = new();
    private readonly DeterministicTransitionValidator _validator = new();

    [Theory]
    [InlineData("invalid_deterministic1.fsm")]
    [InlineData("invalid_deterministic2.fsm")]
    [InlineData("invalid_deterministic3.fsm")]
    public void ValidateRejectsNonDeterministicSamples(string fileName)
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor(fileName));

        var errors = _validator.Validate(diagram).ToList();

        Assert.NotEmpty(errors);
        Assert.All(errors, error => Assert.Equal("state1", error.ElementId));
    }

    [Fact]
    public void ValidateAcceptsDeterministicSample()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("valid_deterministic.fsm"));

        var errors = _validator.Validate(diagram);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateAcceptsGuardedAutomaticTransitionMixedWithTriggeredTransitions()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("example_user_account.fsm"));

        var errors = _validator.Validate(diagram);

        Assert.Empty(errors);
    }
}
