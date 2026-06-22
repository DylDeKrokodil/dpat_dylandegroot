namespace Fsm.Parsing;

public sealed class ParseException : Exception
{
    public ParseException(int lineNumber, string message)
        : base($"Line {lineNumber}: {message}")
    {
        LineNumber = lineNumber;
    }

    public int LineNumber { get; }
}
