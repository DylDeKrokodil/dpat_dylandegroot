using Fsm.Domain;
using Fsm.Validation;

namespace Fsm.Tests.Validation;

public class ValidationPipelineTests
{
    [Fact]
    public void ValidateRunsMultipleValidatorsAndReturnsAllErrors()
    {
        var pipeline = new ValidationPipeline(
        [
            new StubValidator(new ValidationError("one", "First")),
            new StubValidator(new ValidationError("two", "Second"))
        ]);

        var result = pipeline.Validate(new FsmDiagram());

        Assert.False(result.IsValid);
        Assert.Equal(["one", "two"], result.Errors.Select(error => error.Code));
    }

    private sealed class StubValidator(ValidationError error) : IFsmValidator
    {
        public IEnumerable<ValidationError> Validate(FsmDiagram diagram)
        {
            yield return error;
        }
    }
}
