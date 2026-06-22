namespace Fsm.Validation;

public sealed class ValidationResult
{
    private ValidationResult(IReadOnlyList<ValidationError> errors)
    {
        Errors = errors;
    }

    public IReadOnlyList<ValidationError> Errors { get; }

    public bool IsValid => Errors.Count == 0;

    public static ValidationResult Success()
    {
        return new ValidationResult([]);
    }

    public static ValidationResult Failed(IEnumerable<ValidationError> errors)
    {
        return new ValidationResult(errors.ToList());
    }
}
