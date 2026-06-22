using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Tests.Domain;

public class StateActionTests
{
    [Fact]
    public void StateCanStoreStateActions()
    {
        var state = new SimpleState("state", "State");
        var action = new StateAction(state, "enter", ActionType.EntryAction);

        state.AddAction(action);

        Assert.Contains(action, state.Actions);
    }

    [Fact]
    public void TransitionActionCannotBeAttachedToState()
    {
        var state = new SimpleState("state", "State");

        var exception = Assert.Throws<DomainException>(() => new StateAction(state, "invalid", ActionType.TransitionAction));

        Assert.Contains("cannot be attached to a state", exception.Message);
    }
}
