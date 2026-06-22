using Fsm.Building;
using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Tests.Building;

public class StateFactoryTests
{
    [Theory]
    [InlineData(StateType.Initial, typeof(InitialState))]
    [InlineData(StateType.Simple, typeof(SimpleState))]
    [InlineData(StateType.Compound, typeof(CompoundState))]
    [InlineData(StateType.Final, typeof(FinalState))]
    public void CreateMapsStateTypesToConcreteStates(StateType stateType, Type expectedType)
    {
        var factory = new StateFactory();

        var state = factory.Create("state", "State", stateType, parent: null);

        Assert.IsType(expectedType, state);
    }

    [Fact]
    public void CreateUsesRegisteredCreatorForStateType()
    {
        var creator = new RecordingStateCreator();
        var factory = new StateFactory([creator]);

        var state = factory.Create("state", "State", StateType.Simple, parent: null);

        Assert.True(creator.WasCalled);
        Assert.IsType<SimpleState>(state);
    }

    private sealed class RecordingStateCreator : IStateCreator
    {
        public bool WasCalled { get; private set; }

        public StateType StateType => StateType.Simple;

        public State Create(string id, string name, State? parent)
        {
            WasCalled = true;
            return new SimpleState(id, name, parent);
        }
    }
}
