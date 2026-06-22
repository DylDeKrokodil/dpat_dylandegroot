namespace Fsm.Validation;

public sealed record ValidationError(string Code, string Message, string? ElementId = null);
