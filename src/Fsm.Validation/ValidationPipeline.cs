using Fsm.Domain;

namespace Fsm.Validation;

public sealed class ValidationPipeline
{
    private readonly IReadOnlyList<IFsmValidator> _validators;

    public ValidationPipeline(IEnumerable<IFsmValidator> validators)
    {
        _validators = validators.ToList();
    }

    public ValidationResult Validate(FsmDiagram diagram)
    {
        var errors = _validators
            .SelectMany(validator => validator.Validate(diagram))
            .ToList();

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failed(errors);
    }
}
