using Fsm.Parsing;
using Fsm.Simulation;
using Fsm.Tests.Fixtures;

namespace Fsm.Tests.Simulation;

public class FsmSimulatorTests
{
    private readonly FsmTextParser _parser = new();

    [Fact]
    public void SimulatorStartsAtInitialState()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("example_lamp.fsm"));

        var simulator = new FsmSimulator(diagram);

        Assert.Equal("initial", simulator.CurrentState.Id);
        Assert.Contains("Simulation started", simulator.Log[0]);
    }

    [Fact]
    public void TryFollowMovesToDestinationAndLogsActions()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("example_lamp.fsm"));
        var simulator = new FsmSimulator(diagram);
        var transition = simulator.GetAvailableTransitions().Single(item => item.Id == "t1");

        var result = simulator.TryFollow(transition, guardAccepted: true);

        Assert.True(result.TransitionTaken);
        Assert.Equal("off", simulator.CurrentState.Id);
        Assert.Contains(result.Events, item => item.Contains("Current state is now 'off'."));
        Assert.Contains(result.Events, item => item.Contains("Entry action on 'off': Start off timer"));
    }

    [Fact]
    public void TryFollowCanRejectGuardAndStayInCurrentState()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("example_lamp.fsm"));
        var simulator = new FsmSimulator(diagram);
        simulator.TryFollow(simulator.GetAvailableTransitions().Single(item => item.Id == "t1"), guardAccepted: true);
        var guardedTransition = simulator.GetAvailableTransitions().Single(item => item.Id == "t2");

        var result = simulator.TryFollow(guardedTransition, guardAccepted: false);

        Assert.False(result.TransitionTaken);
        Assert.Equal("off", simulator.CurrentState.Id);
        Assert.Contains(result.Events, item => item.Contains("Guard"));
    }

    [Fact]
    public void AvailableTransitionsIncludeParentCompoundTransitions()
    {
        var diagram = _parser.ParseFile(SampleFsmFiles.PathFor("example_lamp.fsm"));
        var simulator = new FsmSimulator(diagram);
        simulator.TryFollow(simulator.GetAvailableTransitions().Single(item => item.Id == "t1"), guardAccepted: true);

        var availableTransitionIds = simulator.GetAvailableTransitions().Select(transition => transition.Id);

        Assert.Contains("t2", availableTransitionIds);
        Assert.Contains("t4", availableTransitionIds);
    }
}
