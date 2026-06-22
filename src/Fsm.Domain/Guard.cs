namespace Fsm.Domain;

public sealed record Guard(string Expression)
{
    public bool IsEmpty => string.IsNullOrWhiteSpace(Expression);

    public static Guard Empty { get; } = new(string.Empty);
}
