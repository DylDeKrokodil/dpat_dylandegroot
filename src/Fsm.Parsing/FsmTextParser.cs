using System.Text.RegularExpressions;
using Fsm.Building;
using Fsm.Domain;

namespace Fsm.Parsing;

public sealed partial class FsmTextParser : IFsmParser
{
    private readonly FsmTokenizer _tokenizer;

    public FsmTextParser()
        : this(new FsmTokenizer())
    {
    }

    public FsmTextParser(FsmTokenizer tokenizer)
    {
        _tokenizer = tokenizer;
    }

    public FsmDiagram ParseFile(string filePath)
    {
        return ParseText(File.ReadAllText(filePath));
    }

    public FsmDiagram ParseText(string text)
    {
        var definitions = _tokenizer.Tokenize(text);
        var builder = new FsmModelBuilder();
        var actionDefinitions = new List<FsmDefinition>();

        foreach (var definition in definitions)
        {
            try
            {
                switch (definition.Type)
                {
                    case DefinitionType.State:
                        ParseState(definition, builder);
                        break;
                    case DefinitionType.Trigger:
                        ParseTrigger(definition, builder);
                        break;
                    case DefinitionType.Action:
                        actionDefinitions.Add(definition);
                        break;
                    case DefinitionType.Transition:
                        ParseTransition(definition, builder);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(definition.Type), definition.Type, null);
                }
            }
            catch (ModelBuildingException exception)
            {
                throw new ParseException(definition.LineNumber, exception.Message);
            }
            catch (DomainException exception)
            {
                throw new ParseException(definition.LineNumber, exception.Message);
            }
        }

        foreach (var actionDefinition in actionDefinitions)
        {
            try
            {
                ParseAction(actionDefinition, builder);
            }
            catch (ModelBuildingException exception)
            {
                throw new ParseException(actionDefinition.LineNumber, exception.Message);
            }
            catch (DomainException exception)
            {
                throw new ParseException(actionDefinition.LineNumber, exception.Message);
            }
        }

        return builder.Build();
    }

    private static void ParseState(FsmDefinition definition, FsmModelBuilder builder)
    {
        var match = StateDefinitionRegex().Match(definition.RawText);

        if (!match.Success)
        {
            throw new ParseException(definition.LineNumber, "Malformed STATE definition.");
        }

        builder.AddState(
            Required(match, "id"),
            Required(match, "parent"),
            Required(match, "name"),
            ParseStateType(definition.LineNumber, Required(match, "type")));
    }

    private static void ParseTrigger(FsmDefinition definition, FsmModelBuilder builder)
    {
        var match = TriggerDefinitionRegex().Match(definition.RawText);

        if (!match.Success)
        {
            throw new ParseException(definition.LineNumber, "Malformed TRIGGER definition.");
        }

        builder.AddTrigger(Required(match, "id"), Required(match, "description"));
    }

    private static void ParseTransition(FsmDefinition definition, FsmModelBuilder builder)
    {
        var match = TransitionDefinitionRegex().Match(definition.RawText);

        if (!match.Success)
        {
            throw new ParseException(definition.LineNumber, "Malformed TRANSITION definition.");
        }

        var (triggerId, guard) = ParseTransitionTail(definition.LineNumber, Required(match, "tail"));

        builder.AddTransition(
            Required(match, "id"),
            Required(match, "source"),
            Required(match, "destination"),
            triggerId,
            guard);
    }

    private static void ParseAction(FsmDefinition definition, FsmModelBuilder builder)
    {
        var match = ActionDefinitionRegex().Match(definition.RawText);

        if (!match.Success)
        {
            throw new ParseException(definition.LineNumber, "Malformed ACTION definition.");
        }

        builder.AddAction(
            Required(match, "owner"),
            Required(match, "description"),
            ParseActionType(definition.LineNumber, Required(match, "type")));
    }

    private static (string? TriggerId, string Guard) ParseTransitionTail(int lineNumber, string tail)
    {
        var trimmedTail = tail.Trim();

        if (trimmedTail.Length == 0)
        {
            return (null, string.Empty);
        }

        if (trimmedTail.StartsWith('"'))
        {
            return (null, ParseQuotedGuard(lineNumber, trimmedTail));
        }

        var firstSpaceIndex = trimmedTail.IndexOf(' ', StringComparison.Ordinal);

        if (firstSpaceIndex < 0)
        {
            return (trimmedTail, string.Empty);
        }

        var triggerId = trimmedTail[..firstSpaceIndex];
        var guardText = trimmedTail[firstSpaceIndex..].Trim();

        if (guardText.Length == 0)
        {
            return (triggerId, string.Empty);
        }

        return (triggerId, ParseQuotedGuard(lineNumber, guardText));
    }

    private static string ParseQuotedGuard(int lineNumber, string guardText)
    {
        var match = GuardRegex().Match(guardText);

        if (!match.Success)
        {
            throw new ParseException(lineNumber, "Transition guard must be a quoted string.");
        }

        return Required(match, "guard");
    }

    private static StateType ParseStateType(int lineNumber, string rawType)
    {
        return rawType switch
        {
            "INITIAL" => StateType.Initial,
            "SIMPLE" => StateType.Simple,
            "COMPOUND" => StateType.Compound,
            "FINAL" => StateType.Final,
            _ => throw new ParseException(lineNumber, $"Unsupported state type '{rawType}'.")
        };
    }

    private static ActionType ParseActionType(int lineNumber, string rawType)
    {
        return rawType switch
        {
            "ENTRY_ACTION" => ActionType.EntryAction,
            "DO_ACTION" => ActionType.DoAction,
            "EXIT_ACTION" => ActionType.ExitAction,
            "TRANSITION_ACTION" => ActionType.TransitionAction,
            _ => throw new ParseException(lineNumber, $"Unsupported action type '{rawType}'.")
        };
    }

    private static string Required(Match match, string groupName)
    {
        return match.Groups[groupName].Value;
    }

    [GeneratedRegex("^STATE\\s+(?<id>\\S+)\\s+(?<parent>\\S+)\\s+\"(?<name>[^\"]*)\"\\s*:\\s*(?<type>[A-Z_]+)$")]
    private static partial Regex StateDefinitionRegex();

    [GeneratedRegex("^TRIGGER\\s+(?<id>\\S+)\\s+\"(?<description>[^\"]*)\"$")]
    private static partial Regex TriggerDefinitionRegex();

    [GeneratedRegex("^ACTION\\s+(?<owner>\\S+)\\s+\"(?<description>[^\"]*)\"\\s*:\\s*(?<type>[A-Z_]+)$")]
    private static partial Regex ActionDefinitionRegex();

    [GeneratedRegex("^TRANSITION\\s+(?<id>\\S+)\\s+(?<source>\\S+)\\s*->\\s*(?<destination>\\S+)(?<tail>.*)$")]
    private static partial Regex TransitionDefinitionRegex();

    [GeneratedRegex("^\"(?<guard>[^\"]*)\"$")]
    private static partial Regex GuardRegex();
}
