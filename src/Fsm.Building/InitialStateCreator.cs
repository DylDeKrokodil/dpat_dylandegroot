using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Building;

public sealed class InitialStateCreator : IStateCreator
{
    public StateType StateType => StateType.Initial;

    public State Create(string id, string name, State? parent)
    {
        return new InitialState(id, name, parent);
    }
}
