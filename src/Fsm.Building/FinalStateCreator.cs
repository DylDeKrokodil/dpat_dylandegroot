using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Building;

public sealed class FinalStateCreator : IStateCreator
{
    public StateType StateType => StateType.Final;

    public State Create(string id, string name, State? parent)
    {
        return new FinalState(id, name, parent);
    }
}
