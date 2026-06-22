using Fsm.Domain;
using Fsm.Domain.States;

namespace Fsm.Building;

public sealed class CompoundStateCreator : IStateCreator
{
    public StateType StateType => StateType.Compound;

    public State Create(string id, string name, State? parent)
    {
        return new CompoundState(id, name, parent);
    }
}
