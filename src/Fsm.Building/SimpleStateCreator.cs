using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Building;

public sealed class SimpleStateCreator : IStateCreator
{
    public StateType StateType => StateType.Simple;

    public State Create(string id, string name, State? parent)
    {
        return new SimpleState(id, name, parent);
    }
}
