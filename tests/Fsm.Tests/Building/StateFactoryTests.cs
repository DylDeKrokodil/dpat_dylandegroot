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
}
