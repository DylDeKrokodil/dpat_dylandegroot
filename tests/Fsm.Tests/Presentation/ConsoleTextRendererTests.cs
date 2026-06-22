using Fsm.Parsing;
using Fsm.Presentation;
using Fsm.Tests.Fixtures;

namespace Fsm.Tests.Presentation;

public class ConsoleTextRendererTests
{
    private readonly FsmTextParser _parser = new();
    private readonly ConsoleTextRenderer _renderer = new();

    [Fact]
    public void RenderDiagramIncludesStateHierarchyActionsAndTransitions()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("example_lamp.fsm"));

        var output = _renderer.RenderDiagram(diagram);

        Assert.Contains("FSM Diagram", output);
        Assert.Contains("powered (Compound): Powered up", output);
        Assert.Contains("off (Simple): Lamp is off", output);
        Assert.Contains("action entry: Start off timer", output);
        Assert.Contains("t2: off -> on on push_switch [time off > 10s] / reset off timer", output);
    }

    [Fact]
    public void RenderDiagramIncludesNestedCompoundStates()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("example_user_account.fsm"));

        var output = _renderer.RenderDiagram(diagram);

        Assert.Contains("created (Compound): Created", output);
        Assert.Contains("inactive (Compound): Inactive", output);
        Assert.Contains("unverified (Simple): Unverified", output);
    }

    [Fact]
    public void RenderStateIncludesRelatedTransitionsWhenDiagramIsProvided()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("example_lamp.fsm"));
        var state = diagram.FindState("powered")!;

        var output = _renderer.RenderState(diagram, state);

        Assert.Contains("powered (Compound): Powered up", output);
        Assert.Contains("Related transitions:", output);
        Assert.Contains("t4: powered -> final on power_off", output);
        Assert.Contains("t2: off -> on on push_switch", output);
    }

    [Fact]
    public void RenderTransitionCanRenderSingleTransition()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("example_lamp.fsm"));
        var transition = diagram.Transitions.Single(item => item.Id == "t3");

        var output = _renderer.RenderTransition(transition);

        Assert.Equal("- t3: on -> off on push_switch", output);
    }
}
