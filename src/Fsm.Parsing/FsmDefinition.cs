namespace Fsm.Parsing;

public sealed record FsmDefinition(DefinitionType Type, string RawText, int LineNumber);
