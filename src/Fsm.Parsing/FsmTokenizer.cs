using System.Text;

namespace Fsm.Parsing;

public sealed class FsmTokenizer
{
    private static readonly Dictionary<string, DefinitionType> DefinitionTypes = new(StringComparer.Ordinal)
    {
        ["STATE"] = DefinitionType.State,
        ["TRIGGER"] = DefinitionType.Trigger,
        ["ACTION"] = DefinitionType.Action,
        ["TRANSITION"] = DefinitionType.Transition
    };

    public IReadOnlyList<FsmDefinition> Tokenize(string text)
    {
        var definitions = new List<FsmDefinition>();
        var current = new StringBuilder();
        var currentLineNumber = 0;

        var normalizedText = text.ReplaceLineEndings("\n");
        var lines = normalizedText.Split('\n');

        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var lineNumber = lineIndex + 1;
            var line = lines[lineIndex];

            if (current.Length == 0 && IsIgnorableLine(line))
            {
                continue;
            }

            if (current.Length == 0)
            {
                currentLineNumber = lineNumber;
            }

            AppendLine(current, line);
            ExtractCompleteDefinitions(current, currentLineNumber, definitions);

            if (current.Length > 0 && currentLineNumber == 0)
            {
                currentLineNumber = lineNumber;
            }
        }

        if (!string.IsNullOrWhiteSpace(current.ToString()))
        {
            throw new ParseException(currentLineNumber, "Definition is missing terminating ';'.");
        }

        EnforceDefinitionOrder(definitions);

        return definitions;
    }

    private static bool IsIgnorableLine(string line)
    {
        var trimmed = line.Trim();

        return trimmed.Length == 0 || trimmed.StartsWith('#');
    }

    private static void AppendLine(StringBuilder builder, string line)
    {
        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(line.Trim());
    }

    private static void ExtractCompleteDefinitions(StringBuilder current, int currentLineNumber, List<FsmDefinition> definitions)
    {
        while (current.ToString().IndexOf(';', StringComparison.Ordinal) is var terminatorIndex && terminatorIndex >= 0)
        {
            var rawDefinition = current.ToString(0, terminatorIndex).Trim();
            current.Remove(0, terminatorIndex + 1);

            if (rawDefinition.Length == 0)
            {
                continue;
            }

            definitions.Add(new FsmDefinition(GetDefinitionType(rawDefinition, currentLineNumber), rawDefinition, currentLineNumber));
        }
    }

    private static DefinitionType GetDefinitionType(string rawDefinition, int lineNumber)
    {
        var keyword = rawDefinition.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

        if (keyword is not null && DefinitionTypes.TryGetValue(keyword, out var definitionType))
        {
            return definitionType;
        }

        throw new ParseException(lineNumber, $"Unknown definition type '{keyword ?? rawDefinition}'.");
    }

    private static void EnforceDefinitionOrder(IReadOnlyList<FsmDefinition> definitions)
    {
        var highestSeenOrder = -1;

        foreach (var definition in definitions)
        {
            var order = OrderOf(definition.Type);

            if (order < highestSeenOrder)
            {
                throw new ParseException(definition.LineNumber, $"Definition type '{definition.Type}' appears out of order.");
            }

            highestSeenOrder = Math.Max(highestSeenOrder, order);
        }
    }

    private static int OrderOf(DefinitionType definitionType)
    {
        return definitionType switch
        {
            DefinitionType.State => 0,
            DefinitionType.Trigger => 1,
            DefinitionType.Action => 2,
            DefinitionType.Transition => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(definitionType), definitionType, null)
        };
    }
}
