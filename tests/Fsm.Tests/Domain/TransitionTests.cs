using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Tests.Domain;

public class TransitionTests
{
    [Fact]
    public void TransitionCanRepresentTriggerGuardAndEffect()
    {
        var source = new SimpleState("off", "Off");
        var destination = new SimpleState("on", "On");
        var trigger = new Trigger("push_switch", "Push switch");
        var transition = new Transition("t1", source, destination, trigger, new Guard("time > 10s"));
        var effect = new TransitionAction(transition, "reset timer");

        transition.SetEffect(effect);

        Assert.Same(source, transition.Source);
        Assert.Same(destination, transition.Destination);
        Assert.Same(trigger, transition.Trigger);
        Assert.Equal("time > 10s", transition.Guard.Expression);
        Assert.Same(effect, transition.Effect);
    }

    [Fact]
    public void TransitionCanBeAutomatic()
    {
        var source = new SimpleState("source", "Source");
        var destination = new SimpleState("destination", "Destination");
        var transition = new Transition("t1", source, destination, trigger: null, Guard.Empty);

        Assert.True(transition.IsAutomatic);
    }

    [Fact]
    public void TransitionCanBeSelfTransition()
    {
        var state = new SimpleState("state", "State");
        var transition = new Transition("t1", state, state, trigger: null, Guard.Empty);

        Assert.True(transition.IsSelfTransition);
    }
}
