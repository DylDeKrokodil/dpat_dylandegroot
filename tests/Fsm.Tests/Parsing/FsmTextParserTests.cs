using Fsm.Domain;
using Fsm.Domain.States;
using Fsm.Parsing;
using Fsm.Tests.Fixtures;

namespace Fsm.Tests.Parsing;

public class FsmTextParserTests
{
    [Fact]
    public void ParseFileBuildsLampExample()
    {
        var parser = new FsmTextParser();

        var diagram = parser.ParseFile(SampleFsmFiles.PathFor("example_lamp.fsm"));

        Assert.Equal(5, diagram.States.Count);
        Assert.Equal(3, diagram.Triggers.Count);
        Assert.Equal(4, diagram.Transitions.Count);
        Assert.Equal(3, diagram.States.SelectMany(state => state.Actions).Count());
        Assert.Single(diagram.Transitions, transition => transition.Effect is not null);
        Assert.IsType<CompoundState>(diagram.FindState("powered"));
    }

    [Fact]
    public void ParseFileBuildsNestedCompoundExample()
    {
        var parser = new FsmTextParser();

        var diagram = parser.ParseFile(SampleFsmFiles.PathFor("example_user_account.fsm"));

        var created = Assert.IsType<CompoundState>(diagram.FindState("created"));
        var inactive = Assert.IsType<CompoundState>(diagram.FindState("inactive"));
        var unverified = Assert.IsType<SimpleState>(diagram.FindState("unverified"));

        Assert.Equal(10, diagram.States.Count);
        Assert.Equal(10, diagram.Triggers.Count);
        Assert.Equal(12, diagram.Transitions.Count);
        Assert.Contains(inactive, created.Children);
        Assert.Contains(unverified, inactive.Children);
        Assert.True(unverified.IsNestedIn(created));
    }

    [Fact]
    public void ParseTransitionSupportsMissingGuardStringForCompatibilityWithSamples()
    {
        const string text = """
            STATE state1 _ "State1" : SIMPLE;
            STATE state2 _ "State2" : SIMPLE;
            TRIGGER mouse_click "mouse click";
            TRANSITION t1 state1 -> state2 mouse_click;
            """;

        var parser = new FsmTextParser();

        var diagram = parser.ParseText(text);

        var transition = Assert.Single(diagram.Transitions);
        Assert.Equal("mouse_click", transition.Trigger?.Id);
        Assert.True(transition.Guard.IsEmpty);
    }

    [Fact]
    public void ParseTransitionSupportsTriggerlessGuard()
    {
        const string text = """
            STATE state1 _ "State1" : SIMPLE;
            STATE state2 _ "State2" : SIMPLE;
            TRANSITION t1 state1 -> state2 "attempts >= 3";
            """;

        var parser = new FsmTextParser();

        var diagram = parser.ParseText(text);

        var transition = Assert.Single(diagram.Transitions);
        Assert.Null(transition.Trigger);
        Assert.Equal("attempts >= 3", transition.Guard.Expression);
    }

    [Fact]
    public void ParseStateReportsLineNumberForMalformedDefinition()
    {
        const string text = """
            STATE initial _ "" : INITIAL;
            STATE missing_parts;
            """;

        var parser = new FsmTextParser();

        var exception = Assert.Throws<ParseException>(() => parser.ParseText(text));

        Assert.Equal(2, exception.LineNumber);
    }
}
