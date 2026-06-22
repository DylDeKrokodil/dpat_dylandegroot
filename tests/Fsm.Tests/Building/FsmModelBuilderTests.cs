using Fsm.Building;
using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Tests.Building;

public class FsmModelBuilderTests
{
    [Fact]
    public void BuilderCreatesStateHierarchy()
    {
        var builder = new FsmModelBuilder();

        var compound = builder.AddState("compound", "_", "Compound", StateType.Compound);
        var child = builder.AddState("child", "compound", "Child", StateType.Simple);

        var built = builder.Build();

        Assert.Same(compound, built.FindState("compound"));
        Assert.Same(child, built.FindState("child"));
        Assert.Contains(child, ((CompoundState)compound).Children);
    }

    [Fact]
    public void BuilderResolvesTransitionReferences()
    {
        var builder = new FsmModelBuilder();
        builder.AddState("source", "_", "Source", StateType.Simple);
        builder.AddState("destination", "_", "Destination", StateType.Simple);
        builder.AddTrigger("trigger", "Trigger");

        var transition = builder.AddTransition("t1", "source", "destination", "trigger", "ready");

        Assert.Equal("source", transition.Source.Id);
        Assert.Equal("destination", transition.Destination.Id);
        Assert.Equal("trigger", transition.Trigger?.Id);
        Assert.Equal("ready", transition.Guard.Expression);
    }

    [Fact]
    public void BuilderAttachesActionsToCorrectOwnerType()
    {
        var builder = new FsmModelBuilder();
        var state = builder.AddState("state", "_", "State", StateType.Simple);
        builder.AddState("destination", "_", "Destination", StateType.Simple);
        var transition = builder.AddTransition("t1", "state", "destination", triggerId: null, guard: "");

        var stateAction = builder.AddAction("state", "enter", ActionType.EntryAction);
        var transitionAction = builder.AddAction("t1", "effect", ActionType.TransitionAction);

        Assert.Contains((StateAction)stateAction, state.Actions);
        Assert.Same(transitionAction, transition.Effect);
    }

    [Fact]
    public void BuilderRejectsDuplicateElementIds()
    {
        var builder = new FsmModelBuilder();
        builder.AddState("same", "_", "State", StateType.Simple);

        var exception = Assert.Throws<ModelBuildingException>(() => builder.AddTrigger("same", "Trigger"));

        Assert.Contains("already exists", exception.Message);
    }
}
