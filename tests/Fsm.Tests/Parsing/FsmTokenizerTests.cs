using Fsm.Parsing;

namespace Fsm.Tests.Parsing;

public class FsmTokenizerTests
{
    [Fact]
    public void TokenizeIgnoresBlankLinesCommentLinesAndWhitespace()
    {
        const string text = """
            # comment

              STATE initial _ "" : INITIAL;
            TRIGGER start "Start";
            """;

        var tokenizer = new FsmTokenizer();

        var definitions = tokenizer.Tokenize(text);

        Assert.Equal([DefinitionType.State, DefinitionType.Trigger], definitions.Select(definition => definition.Type));
    }

    [Fact]
    public void TokenizeSupportsCrLfInputAndPreservesLineNumbers()
    {
        const string text = "STATE initial _ \"\" : INITIAL;\r\n\r\nTRIGGER start \"Start\";";

        var tokenizer = new FsmTokenizer();

        var definitions = tokenizer.Tokenize(text);

        Assert.Equal(1, definitions[0].LineNumber);
        Assert.Equal(3, definitions[1].LineNumber);
    }

    [Fact]
    public void TokenizeEnforcesDefinitionOrder()
    {
        const string text = """
            TRIGGER start "Start";
            STATE initial _ "" : INITIAL;
            """;

        var tokenizer = new FsmTokenizer();

        var exception = Assert.Throws<ParseException>(() => tokenizer.Tokenize(text));

        Assert.Contains("out of order", exception.Message);
    }

    [Fact]
    public void TokenizeRejectsMissingSemicolon()
    {
        const string text = "STATE initial _ \"\" : INITIAL";

        var tokenizer = new FsmTokenizer();

        var exception = Assert.Throws<ParseException>(() => tokenizer.Tokenize(text));

        Assert.Contains("missing terminating", exception.Message);
    }
}
