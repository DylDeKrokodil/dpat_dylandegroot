using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Tests.Domain;

public class FsmDiagramTests
{
    [Fact]
    public void DiagramStoresAndFindsStatesAndTriggers()
    {
        var diagram = new FsmDiagram();
        var initial = new InitialState("initial", "Initial");
        var trigger = new Trigger("start", "Start");

        diagram.AddState(initial);
        diagram.AddTrigger(trigger);

        Assert.Same(initial, diagram.FindState("initial"));
        Assert.Same(trigger, diagram.FindTrigger("start"));
        Assert.Same(initial, diagram.InitialState);
    }

    [Fact]
    public void DiagramConnectsCompoundStateChildrenWhenStateIsAdded()
    {
        var diagram = new FsmDiagram();
        var parent = new CompoundState("parent", "Parent");
        var child = new SimpleState("child", "Child", parent);

        diagram.AddState(parent);
        diagram.AddState(child);

        Assert.Contains(child, parent.Children);
    }

    [Fact]
    public void DiagramExposesIncomingAndOutgoingTransitionQueries()
    {
        var diagram = new FsmDiagram();
        var source = new SimpleState("source", "Source");
        var destination = new SimpleState("destination", "Destination");
        var transition = new Transition("t1", source, destination, trigger: null, Guard.Empty);

        diagram.AddState(source);
        diagram.AddState(destination);
        diagram.AddTransition(transition);

        Assert.Equal([transition], diagram.GetOutgoingTransitions(source));
        Assert.Equal([transition], diagram.GetIncomingTransitions(destination));
    }
}
